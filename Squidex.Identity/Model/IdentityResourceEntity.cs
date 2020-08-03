// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Linq;
using IdentityServer4.Models;
using Squidex.ClientLibrary;

namespace Squidex.Identity.Model
{
    public sealed class IdentityResourceEntity : Content<IdentityResourceData>
    {
        public IdentityResource ToResource()
        {
            var result = new IdentityResource(
                Data.Name,
                Data.DisplayName ?? string.Empty,
                Data.UserClaims ?? Enumerable.Empty<string>())
            {
                Enabled = !Data.Disabled,
                Emphasize = Data.Emphasize,
                Required = Data.Required
            };

            return result;
        }
    }
}
