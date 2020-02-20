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
        private readonly ConcurrentDictionary<string, SquidexClientManager> clientManagers = new ConcurrentDictionary<string, SquidexClientManager>();
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

        public SquidexClientManager GetClientManager()
        {
            var host = httpContextAccessor?.HttpContext?.Request.Host.ToString();

            if (string.IsNullOrWhiteSpace(host))
            {
                return appDefault;
            }

            if (appPerHost.TryGetValue(host, out var options))
            {
                return clientManagers.GetOrAdd(host, x => new SquidexClientManager(options));
            }

            return appDefault;
        }
    }
}
