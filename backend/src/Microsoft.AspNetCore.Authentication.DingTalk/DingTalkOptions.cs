// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.Authentication.DingTalk
{
    /// <summary>
    /// Configuration options for <see cref="DingTalkHandler"/>.
    /// </summary>
    public class DingTalkOptions : OAuthOptions
    {
        /// <summary>
        /// Initializes a new <see cref="DingTalkOptions"/>.
        /// </summary>
        public DingTalkOptions()
        {
            CallbackPath = new PathString("/signin-ding");
            AuthorizationEndpoint = DingTalkDefaults.AuthorizationEndpoint;
            TokenEndpoint = DingTalkDefaults.TokenEndpoint;
            UserInformationEndpoint = DingTalkDefaults.UserInformationEndpoint;
            Scope.Add("snsapi_login");
            ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "openid");
            ClaimActions.MapJsonKey(ClaimTypes.Name, "nick");
        }

        /// <summary>
        /// access_type. Set to 'offline' to request a refresh token.
        /// </summary>
        public string AccessType { get; set; }
    }
}
