// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.Stores;
using Squidex.ClientLibrary;

namespace Squidex.Identity.Model
{
    public sealed class ClientStore : IClientStore
    {
        private readonly SquidexClient<ClientEntity, ClientData> apiClient;
        private readonly SquidexClientManager apiClientManager;

        public ClientStore(SquidexClientManager clientManager)
        {
            apiClient = clientManager.GetClient<ClientEntity, ClientData>("clients");
            apiClientManager = clientManager;
        }

        public async Task<Client> FindClientByIdAsync(string clientId)
        {
            var clients = await apiClient.GetAsync(filter: $"data/clientId/iv eq '{clientId}'", context: Context.Build());

            var client = clients.Items.FirstOrDefault();

            if (client == null)
            {
                return null;
            }

            return new Client
            {
                AllowAccessTokensViaBrowser = true,
                AllowedCorsOrigins = client.Data.AllowedCorsOrigins.OrDefault(),
                AllowedGrantTypes = client.Data.AllowedGrantTypes.OrDefault(),
                AllowedScopes = client.Data.AllowedScopes.OrDefault(),
                AllowOfflineAccess = client.Data.AllowOfflineAccess,
                ClientId = clientId,
                ClientName = client.Data.ClientName,
                ClientSecrets = client.Data.ClientSecrets.ToSecrets(),
                ClientUri = client.Data.ClientUri,
                LogoUri = apiClientManager.GenerateImageUrl(client.Data.Logo),
                RedirectUris = client.Data.RedirectUris.OrDefault(),
                RequireConsent = client.Data.RequireConsent,
                PostLogoutRedirectUris = client.Data.PostLogoutRedirectUris.OrDefault()
            };
        }
    }
}
