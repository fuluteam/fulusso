// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.AspNetCore.Authentication.DingTalk
{
    /// <summary>
    /// Default values for Google authentication
    /// </summary>
    public static class DingTalkDefaults
    {
        public const string AuthenticationScheme = "ding";

        public static readonly string DisplayName = "ding";

        public static readonly string AuthorizationEndpoint = "https://oapi.dingtalk.com/connect/qrconnect";

        public static readonly string TokenEndpoint = "https://oapi.dingtalk.com/sns/gettoken";

        public static readonly string UserInformationEndpoint = "https://oapi.dingtalk.com/sns/getuserinfo_bycode";
    }
}
