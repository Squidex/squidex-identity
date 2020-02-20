// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;

namespace Squidex.Identity.Model
{
    public abstract class CachingProvider
    {
        private readonly IMemoryCache cache;
        private readonly IHttpContextAccessor httpContextAccessor;

        protected CachingProvider(IMemoryCache cache, IHttpContextAccessor httpContextAccessor)
        {
            this.cache = cache;

            this.httpContextAccessor = httpContextAccessor;
        }

        protected async Task<T> GetOrAddAsync<T>(object key, Func<Task<T>> provider)
        {
            var cacheKey = $"{key}_{CultureInfo.CurrentCulture}";

            var host = httpContextAccessor.HttpContext?.Request?.Host.ToString();

            if (!string.IsNullOrWhiteSpace(host))
            {
                cacheKey += $"_{host}";
            }

            if (!cache.TryGetValue<T>(cacheKey, out var result))
            {
                result = await provider();

                cache.Set(key, result, Debugger.IsAttached ? TimeSpan.FromSeconds(1) : TimeSpan.FromMinutes(10));
            }

            return result;
        }
    }
}
