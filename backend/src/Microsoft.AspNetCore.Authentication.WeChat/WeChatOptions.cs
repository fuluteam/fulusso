// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.Authentication.WeChat
{
    /// <summary>
    /// Configuration options for <see cref="WeChatHandler"/>.
    /// </summary>
    public class WeChatOptions : OAuthOptions
    {
        /// <summary>
        /// Initializes a new <see cref="WeChatOptions"/>.
        /// </summary>
        public WeChatOptions()
        {
            CallbackPath = new PathString("/signin-wechat");
            AuthorizationEndpoint = WeChatDefaults.AuthorizationEndpoint;
            TokenEndpoint = WeChatDefaults.TokenEndpoint;
            UserInformationEndpoint = WeChatDefaults.UserInformationEndpoint;
            Scope.Add("snsapi_login");
            //Scope.Add("snsapi_userinfo");

            ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "openid");
            ClaimActions.MapJsonKey(ClaimTypes.Name, "nickname");
        }

        /// <summary>
        /// access_type. Set to 'offline' to request a refresh token.
        /// </summary>
        public string AccessType { get; set; }
    }
}
