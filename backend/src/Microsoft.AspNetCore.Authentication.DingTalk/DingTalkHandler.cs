// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace Microsoft.AspNetCore.Authentication.DingTalk
{
    public class DingTalkHandler : OAuthHandler<DingTalkOptions>
    {
        public DingTalkHandler(IOptionsMonitor<DingTalkOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock) : base(options, logger, encoder, clock)
        {

        }

        /// <summary>
        /// 1.构造认证的URI
        /// </summary>
        protected override string BuildChallengeUrl(AuthenticationProperties properties, string redirectUri)
        {
            var state = Options.StateDataFormat.Protect(properties);

            var baseUri = $"{Request.Scheme}{Uri.SchemeDelimiter}{Request.Host}{Request.PathBase}";

            var currentUri = $"{baseUri}{Request.Path}{Request.QueryString}";

            if (string.IsNullOrEmpty(properties.RedirectUri))
            {
                properties.RedirectUri = currentUri;
            }

            var queryStrings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                {"response_type", "code"},
                {"appid", Uri.EscapeDataString(Options.ClientId)},
                {"redirect_uri", redirectUri},
                {"state", Uri.EscapeDataString(state)}
            };

            var scope = string.Join(",", Options.Scope);

            queryStrings.Add("scope", Uri.EscapeDataString(scope));

            var authorizationEndpoint = QueryHelpers.AddQueryString(Options.AuthorizationEndpoint, queryStrings);
            return authorizationEndpoint;
        }

        /// <summary>
        /// 2.向认证服务器申请令牌
        /// </summary>
        /// <returns></returns>
        protected override async Task<HandleRequestResult> HandleRemoteAuthenticateAsync()
        {

            var state = Request.Query["state"];
            var properties = Options.StateDataFormat.Unprotect(state);

            if (properties == null)
                return HandleRequestResult.Fail("The oauth state was missing or invalid.");
            if (!ValidateCorrelationId(properties))
                return HandleRequestResult.Fail("Correlation failed.", properties);

            var code = Request.Query["code"];
            if (StringValues.IsNullOrEmpty(code))
                return HandleRequestResult.Fail("Code was not found.", properties);

            var redirectUri = !string.IsNullOrEmpty(Options.CallbackPath) ?
                Options.CallbackPath.Value : BuildRedirectUri(Options.CallbackPath);

            var context = new OAuthCodeExchangeContext(properties, code, redirectUri);

            var tokens = await ExchangeCodeAsync(context);

            if (tokens.Error != null)
                return HandleRequestResult.Fail(tokens.Error, properties);
            if (string.IsNullOrEmpty(tokens.AccessToken))
                return HandleRequestResult.Fail("Failed to retrieve access token.", properties);
            var identity = new ClaimsIdentity(ClaimsIssuer);

            //if (Options.SaveTokens)
            //{
            var authenticationTokenList = new List<AuthenticationToken>
                {
                    new AuthenticationToken
                    {
                        Name = "access_token",
                        Value = tokens.AccessToken
                    }
                };
            if (!string.IsNullOrEmpty(tokens.RefreshToken))
            {
                authenticationTokenList.Add(new AuthenticationToken
                {
                    Name = "refresh_token",
                    Value = tokens.RefreshToken
                });
            }

            if (!string.IsNullOrEmpty(tokens.TokenType))
            {
                authenticationTokenList.Add(new AuthenticationToken
                {
                    Name = "token_type",
                    Value = tokens.TokenType
                });
            }

            if (!string.IsNullOrEmpty(tokens.ExpiresIn) && int.TryParse(tokens.ExpiresIn, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
            {
                var dateTimeOffset = Clock.UtcNow + TimeSpan.FromSeconds(result);
                authenticationTokenList.Add(new AuthenticationToken()
                {
                    Name = "expires_at",
                    Value = dateTimeOffset.ToString("o", CultureInfo.InvariantCulture)
                });
            }

            authenticationTokenList.Add(new AuthenticationToken
            {
                Name = "code",
                Value = code
            });

            properties.StoreTokens(authenticationTokenList);

            var ticket = await CreateTicketAsync(identity, properties, tokens);
            return ticket == null ? HandleRequestResult.Fail("Failed to retrieve user information from remote server.", properties) : HandleRequestResult.Success(ticket);
        }

        /// <summary>
        ///2.1子步骤>>向认证服务器申请令牌
        /// </summary>
        protected override async Task<OAuthTokenResponse> ExchangeCodeAsync(OAuthCodeExchangeContext context)
        {
            var tokenRequestParameters = new Dictionary<string, string>
            {
                {"appid", Options.ClientId},
                {"appsecret", Options.ClientSecret}
            };

            var userInfoEndpoint = QueryHelpers.AddQueryString(Options.TokenEndpoint, tokenRequestParameters);

            var response = await Backchannel.GetAsync(userInfoEndpoint, Context.RequestAborted);

            return response.IsSuccessStatusCode ? OAuthTokenResponse.Success(JsonDocument.Parse(await response.Content.ReadAsStringAsync())) : OAuthTokenResponse.Failed(new Exception("OAuth token failure"));
        }

        /// <summary>
        /// 3.通过令牌获取用户并创建Ticket
        /// </summary>
        protected override async Task<AuthenticationTicket> CreateTicketAsync(
            ClaimsIdentity identity,
            AuthenticationProperties properties,
            OAuthTokenResponse tokens)
        {
            var code = properties.GetTokenValue("code");

            var timestamp = ((DateTime.Now.Ticks - new DateTime(1970, 1, 1).ToLocalTime().Ticks) / 10000).ToString();

            var signature = HMacSha256.Compute(timestamp, Options.ClientSecret);

            var parameters = new Dictionary<string, string>
            {
                {  "accessKey", Options.ClientId},
                {  "timestamp", timestamp},
                {  "signature",signature}
            };

            var userInfoEndpoint = QueryHelpers.AddQueryString(Options.UserInformationEndpoint, parameters);

            var response = await Backchannel.PostAsync(userInfoEndpoint, new StringContent($"{{ \"tmp_auth_code\":\"{code}\"}}"), Context.RequestAborted);

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"An error occurred when retrieving Ding user information ({response.StatusCode}). Please check if the authentication information is correct.");
            }

            using (var payload = JsonDocument.Parse(await response.Content.ReadAsStringAsync()))
            {
                payload.RootElement.TryGetProperty("errcode", out var resCode);

                if (resCode.GetInt32() != 0)
                {
                    throw new HttpRequestException($"An error occurred when retrieving Ding user information ({response.StatusCode}). Please check if the authentication information is correct.");
                }

                payload.RootElement.TryGetProperty("user_info", out var userData);

                var context = new OAuthCreatingTicketContext(new ClaimsPrincipal(identity), properties, Context, Scheme,
                    Options, Backchannel, tokens, userData);

                context.RunClaimActions();
                await Events.CreatingTicket(context);

                context.Properties.ExpiresUtc = DateTimeOffset.Now.AddMinutes(15);
                return new AuthenticationTicket(context.Principal, context.Properties, Scheme.Name);
            }
        }


    }
}
