// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Squidex.ClientLibrary;

namespace Squidex.Identity.Model.Authentication
{
    public sealed class AuthenticationSchemeStore : CachingProvider, IAuthenticationSchemeStore
    {
        private readonly SquidexClientManagerFactory factory;

        public AuthenticationSchemeStore(IMemoryCache cache, IHttpContextAccessor httpContextAccessor, SquidexClientManagerFactory factory)
            : base(cache, httpContextAccessor)
        {
            this.factory = factory;
        }

        public Task<List<AuthenticationSchemeData>> GetSchemesAsync()
        {
            return GetOrAddAsync(nameof(AuthenticationSchemeType), async () =>
            {
                var schemes = await GetSchemesCoreAsync();

                return schemes.Items
                    .Select(x => x.Data).GroupBy(x => x.Provider)
                    .Select(x => x.First())
                    .ToList();
            });
        }

        private Task<ContentsResult<AuthenticationSchemeEntity, AuthenticationSchemeData>> GetSchemesCoreAsync()
        {
            var client = factory.GetContentsClient<AuthenticationSchemeEntity, AuthenticationSchemeData>("authentication-schemes");

            return client.GetAsync(context: Context.Build());
        }
    }
}
