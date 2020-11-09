// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.WeChat;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class WeChatExtensions
    {
        public static AuthenticationBuilder AddWeChat(this AuthenticationBuilder builder)
            => builder.AddWeChat(WeChatDefaults.AuthenticationScheme, _ => { });

        public static AuthenticationBuilder AddWeChat(this AuthenticationBuilder builder, Action<WeChatOptions> configureOptions)
            => builder.AddWeChat(WeChatDefaults.AuthenticationScheme, configureOptions);

        public static AuthenticationBuilder AddWeChat(this AuthenticationBuilder builder, string authenticationScheme, Action<WeChatOptions> configureOptions)
            => builder.AddWeChat(authenticationScheme, WeChatDefaults.DisplayName, configureOptions);

        public static AuthenticationBuilder AddWeChat(this AuthenticationBuilder builder, string authenticationScheme, string displayName, Action<WeChatOptions> configureOptions)
            => builder.AddOAuth<WeChatOptions, WeChatHandler>(authenticationScheme, displayName, configureOptions);
    }
}
