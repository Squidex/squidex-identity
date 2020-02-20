// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Squidex.Identity.Model
{
    public sealed class SettingsProvider : CachingProvider, ISettingsProvider
    {
        private readonly SquidexClientManagerFactory factory;
        private readonly IOptions<SettingsData> defaults;

        public SettingsProvider(IMemoryCache cache, IHttpContextAccessor httpContextAccessor, SquidexClientManagerFactory factory, IOptions<SettingsData> defaults)
            : base(cache, httpContextAccessor)
        {
            this.factory = factory;

            this.defaults = defaults;
        }

        public Task<SettingsData> GetSettingsAsync()
        {
            return GetOrAddAsync(nameof(SettingsProvider), async () =>
            {
                var apiClient =
                    factory.GetClientManager()
                        .CreateContentsClient<SettingsEntity, SettingsData>("settings");

                var settings = await apiClient.GetAsync(context: Context.Build());

                var result = settings.Items.FirstOrDefault()?.Data ?? new SettingsData();

                foreach (var property in result.GetType().GetProperties())
                {
                    if (property.GetValue(result) == null)
                    {
                        property.SetValue(result, property.GetValue(defaults.Value));
                    }
                }

                return result;
            });
        }
    }
}
