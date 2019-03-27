// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Identity;

namespace Squidex.Identity.Model
{
    public sealed class UserLogin
    {
        public string LoginProvider { get; set; }

        public string DisplayName { get; set; }

        public string ProviderKey { get; set; }

        public static implicit operator UserLoginInfo(UserLogin src)
        {
            return src.ToInfo();
        }

        public static implicit operator UserLogin(UserLoginInfo src)
        {
            return new UserLogin { LoginProvider = src.LoginProvider, DisplayName = src.ProviderDisplayName, ProviderKey = src.ProviderKey };
        }

        public UserLoginInfo ToInfo()
        {
            return new UserLoginInfo(LoginProvider, this.ProviderKey, this.DisplayName);
        }
    }
}