// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4;
using IdentityServer4.Models;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Squidex.ClientLibrary;

namespace Squidex.Identity.Model
{
    public sealed class ClientStore : CachingProvider, IClientStore
    {
        private readonly SquidexClientManagerFactory factory;

        public ClientStore(IMemoryCache cache, IHttpContextAccessor httpContextAccessor, SquidexClientManagerFactory factory)
            : base(cache, httpContextAccessor)
        {
            this.factory = factory;
        }

        public async Task<Client> FindClientByIdAsync(string clientId)
        {
            var clientManager = factory.GetClientManager();

            var clients = await GetClientsAsync();
            var client = clients.Items.FirstOrDefault(x => x.Data.ClientId == clientId);

            if (client == null)
            {
                return null;
            }

            return client.ToClient(clientManager);
        }

        private Task<ContentsResult<ClientEntity, ClientData>> GetClientsAsync()
        {
            return GetOrAddAsync("Clients", () =>
            {
                var client = factory.GetContentsClient<ClientEntity, ClientData>("clients");

                return client.GetAsync(context: Context.Build());
            });
        }
    }
}
