// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Concurrent;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Squidex.ClientLibrary;

namespace Squidex.Identity.Model
{
    public sealed class SquidexClientManagerFactory
    {
        private readonly SquidexClientManager appDefault;
        private readonly SquidexOptionsPerHost appPerHost;
        private readonly ConcurrentDictionary<string, object> cache = new ConcurrentDictionary<string, object>();
        private readonly IHttpContextAccessor httpContextAccessor;

        public SquidexClientManagerFactory(
            IOptions<SquidexOptions> appDefault,
            IOptions<SquidexOptionsPerHost> appPerHost,
            IHttpContextAccessor httpContextAccessor)
        {
            this.appDefault = new SquidexClientManager(appDefault.Value);
            this.appPerHost = appPerHost.Value;
            this.httpContextAccessor = httpContextAccessor;
        }

        public IContentsClient<TEntity, TData> GetContentsClient<TEntity, TData>(string schemaName)
            where TEntity : Content<TData>
            where TData : class, new()
        {
            var cacheKey = $"{schemaName}_{GetHostName()}";

            return (IContentsClient<TEntity, TData>)cache.GetOrAdd(cacheKey, x =>
            {
                var clientManager = GetClientManager();

                return clientManager.CreateContentsClient<TEntity, TData>(schemaName);
            });
        }

        public ISquidexClientManager GetClientManager()
        {
            var cacheKey = GetHostName();

            if (string.IsNullOrWhiteSpace(cacheKey))
            {
                return appDefault;
            }

            if (appPerHost.TryGetValue(cacheKey, out var options))
            {
                return (ISquidexClientManager)cache.GetOrAdd(cacheKey, x => new SquidexClientManager(options));
            }

            return appDefault;
        }

        private string GetHostName()
        {
            return httpContextAccessor?.HttpContext?.Request.Host.ToString().Replace(":", "_");
        }
    }
}
