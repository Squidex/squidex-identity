// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using IdentityServer4.Models;
using Squidex.ClientLibrary;

namespace Squidex.Identity.Model
{
    public sealed class ApiResourceEntity : Content<ApiResourceData>
    {
        public ApiResource ToResource(Dictionary<Guid, ApiScope> scopes)
        {
            var result = new ApiResource(
                Data.Name,
                Data.DisplayName.OrDefault(),
                Data.UserClaims.OrDefault())
            {
                Enabled = Data.Enabled
            };

            result.Scopes = new List<string>();
            
            if (Data.Scopes != null)
            {
                foreach (var scopeId in Data.Scopes)
                {
                    if (scopes.TryGetValue(scopeId, out var scope))
                    {
                        result.Scopes.Add(scope.Name);
                    }
                }
            }

            result.Description = Data.Description.OrDefault();

            return result;
        }
    }
}
