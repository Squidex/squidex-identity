// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using IdentityServer4.Models;
using Microsoft.Extensions.Localization;

namespace Squidex.Identity.Model
{
    public static class DefaultResources
    {
        public class Permissions : IdentityResource
        {
            public static readonly string Scope = "permissions";

            public Permissions(IStringLocalizer<AppResources> localizer)
            {
                Name = Scope;
                DisplayName = localizer.GetString("Permissions");
                Required = true;
                UserClaims.Add(Scope);
            }
        }
    }
}
