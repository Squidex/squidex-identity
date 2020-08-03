// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using IdentityServer4;
using IdentityServer4.Models;
using Squidex.ClientLibrary;

namespace Squidex.Identity.Model
{
    public sealed class ClientEntity : Content<ClientData>
    {
        public Client ToClient(ISquidexClientManager clientManager)
        {
            ICollection<string> scopes = new List<string>();

            if (Data.AllowedScopes?.Length > 0)
            {
                scopes = new HashSet<string>(Data.AllowedScopes.OrDefault())
                {
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    IdentityServerConstants.StandardScopes.Email,
                    DefaultResources.Permissions.Scope
                };
            }

            var result = new Client
            {
                AllowAccessTokensViaBrowser = true,
                AllowedCorsOrigins = Data.AllowedCorsOrigins.OrDefault(),
                AllowedGrantTypes = Data.AllowedGrantTypes.OrDefault(),
                AllowedScopes = scopes,
                AllowOfflineAccess = Data.AllowOfflineAccess,
                ClientId = Data.ClientId,
                ClientName = Data.ClientName,
                ClientSecrets = Data.ClientSecrets.ToSecrets(),
                ClientUri = Data.ClientUri,
                Enabled = !Data.Disabled,
                EnableLocalLogin = true,
                LogoUri = clientManager.GenerateImageUrl(Data.Logo),
                PostLogoutRedirectUris = Data.PostLogoutRedirectUris.OrDefault(),
                RedirectUris = Data.RedirectUris.OrDefault(),
                RequireConsent = Data.RequireConsent
            };

            return result;
        }
    }
}
