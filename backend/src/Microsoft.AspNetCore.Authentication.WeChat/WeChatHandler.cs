// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace Microsoft.AspNetCore.Authentication.WeChat
{
    public class WeChatHandler : OAuthHandler<WeChatOptions>
    {
        public WeChatHandler(IOptionsMonitor<WeChatOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock) : base(options, logger, encoder, clock)
        {

        }

        protected override string BuildChallengeUrl(AuthenticationProperties properties, string redirectUri)
        {
            var state = Options.StateDataFormat.Protect(properties);

            var baseUri = $"{Request.Scheme}{Uri.SchemeDelimiter}{Request.Host}{Request.PathBase}";

            var currentUri = $"{baseUri}{Request.Path}{Request.QueryString}";


            if (string.IsNullOrEmpty(properties.RedirectUri))
            {
                properties.RedirectUri = currentUri;
            }

            //if (!string.IsNullOrEmpty(Options.CallbackPath))
            //{
            //    redirectUri = Options.CallbackPath;
            //}

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

            //var openId = tokens.Response.RootElement.GetString("openid");

            if (Options.SaveTokens)
            {
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
                properties.StoreTokens(authenticationTokenList);
            }

            var ticket = await CreateTicketAsync(identity, properties, tokens);
            return ticket == null ? HandleRequestResult.Fail("Failed to retrieve user information from remote server.", properties) : HandleRequestResult.Success(ticket);
        }


        protected override async Task<OAuthTokenResponse> ExchangeCodeAsync(OAuthCodeExchangeContext context)
        {

            var tokenRequestParameters = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("appid", Options.ClientId),
                new KeyValuePair<string, string>("secret", Options.ClientSecret),
                new KeyValuePair<string, string>("code", context.Code),
                new KeyValuePair<string, string>("grant_type", "authorization_code"),
            };

            var urlEncodedContent = new FormUrlEncodedContent(tokenRequestParameters);

            var response =
                await Backchannel.PostAsync(Options.TokenEndpoint, urlEncodedContent, Context.RequestAborted);

            //var response = await Backchannel.SendAsync(new HttpRequestMessage(HttpMethod.Post, Options.TokenEndpoint)
            //{
            //    Headers = {
            //        Accept = {
            //            new MediaTypeWithQualityHeaderValue("application/json")
            //        }
            //    },
            //    Content = urlEncodedContent
            //}, Context.RequestAborted);

            return response.IsSuccessStatusCode ? OAuthTokenResponse.Success(JsonDocument.Parse(await response.Content.ReadAsStringAsync())) : OAuthTokenResponse.Failed(new Exception("OAuth token failure"));
        }

        protected override async Task<AuthenticationTicket> CreateTicketAsync(
            ClaimsIdentity identity,
            AuthenticationProperties properties,
            OAuthTokenResponse tokens)
        {
            var openId = tokens.Response.RootElement.GetString("openid");

            var parameters = new Dictionary<string, string>
            {
                {  "openid", openId},
                {  "access_token", tokens.AccessToken }
            };
            var userInfoEndpoint = QueryHelpers.AddQueryString(Options.UserInformationEndpoint, parameters);
            var response = await Backchannel.GetAsync(userInfoEndpoint, Context.RequestAborted);

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"An error occurred when retrieving WeChat user information ({response.StatusCode}). Please check if the authentication information is correct.");
            }

            using (var payload = JsonDocument.Parse(await response.Content.ReadAsStringAsync()))
            {
                var context = new OAuthCreatingTicketContext(new ClaimsPrincipal(identity), properties, Context, Scheme,
                    Options, Backchannel, tokens, payload.RootElement);

                context.RunClaimActions();
                await Events.CreatingTicket(context);

                context.Properties.ExpiresUtc = DateTimeOffset.Now.AddMinutes(15);
                return new AuthenticationTicket(context.Principal, context.Properties, Scheme.Name);
            }
        }
    }
}
