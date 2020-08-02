// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Localization;
using Squidex.ClientLibrary;

namespace Squidex.Identity.Model
{
    public sealed class ResourceStore : CachingProvider, IResourceStore
    {
        private readonly IStringLocalizer<AppResources> localizer;
        private readonly SquidexClientManagerFactory factory;

        public ResourceStore(IMemoryCache cache, IHttpContextAccessor httpContextAccessor, SquidexClientManagerFactory factory, IStringLocalizer<AppResources> localizer)
            : base(cache, httpContextAccessor)
        {
            this.factory = factory;

            this.localizer = localizer;
        }

        public Task<Resources> GetAllResourcesAsync()
        {
            return GetResourcesAsync();
        }

        public async Task<IEnumerable<ApiScope>> FindApiScopesByNameAsync(IEnumerable<string> scopeNames)
        {
            var resources = await GetResourcesAsync();

            return resources.ApiScopes.Where(x => scopeNames.Contains(x.Name));
        }

        public async Task<IEnumerable<IdentityResource>> FindIdentityResourcesByScopeNameAsync(IEnumerable<string> scopeNames)
        {
            var resources = await GetResourcesAsync();

            return resources.IdentityResources.Where(x => scopeNames.Contains(x.Name));
        }

        public async Task<IEnumerable<ApiResource>> FindApiResourcesByScopeNameAsync(IEnumerable<string> scopeNames)
        {
            var resources = await GetResourcesAsync();

            return resources.ApiResources.Where(x => x.Scopes.Any(s => scopeNames.Contains(s)));
        }

        public async Task<IEnumerable<ApiResource>> FindApiResourcesByNameAsync(IEnumerable<string> apiResourceNames)
        {
            var resources = await GetResourcesAsync();

            return resources.ApiResources.Where(x => apiResourceNames.Contains(x.Name));
        }

        private Task<Resources> GetResourcesAsync()
        {
            return GetOrAddAsync(nameof(ResourceStore), async () =>
            {
                var taskForApiResources = GetApiResourcesAsync(factory);
                var taskForApiScopes = GetApiScopesAsync(factory);
                var taskForIdentityResources = GetIdentityResourcesAsync(factory);

                await Task.WhenAll(
                    taskForApiResources,
                    taskForApiScopes,
                    taskForIdentityResources);

                var identityResources = ConvertIdentityResources(taskForIdentityResources);
                var apiScopes = ConvertApiScopes(taskForApiScopes);
                var apiResources = ConvertApiResources(taskForApiResources, apiScopes);

                return new Resources(identityResources, apiResources, apiScopes.Values.ToList());
            });
        }

        private static List<ApiResource> ConvertApiResources(Task<ContentsResult<ApiResourceEntity, ApiResourceData>> task, Dictionary<Guid, ApiScope> apiScopes)
        {
            return task.Result.Items.Select(x => x.ToResource(apiScopes)).ToList();
        }

        private static Dictionary<Guid, ApiScope> ConvertApiScopes(Task<ContentsResult<ApiScopeEntity, ApiScopeData>> task)
        {
            return task.Result.Items.ToDictionary(x => x.Id, x => x.ToScope());
        }

        private List<IdentityResource> ConvertIdentityResources(Task<ContentsResult<IdentityResourceEntity, IdentityResourceData>> task)
        {
            var identityResources = task.Result.Items.Select(x => x.ToResource()).ToList();

            identityResources.Add(new IdentityResources.OpenId());
            identityResources.Add(new IdentityResources.Profile());
            identityResources.Add(new IdentityResources.Email());
            identityResources.Add(new IdentityResources.Phone());
            identityResources.Add(new DefaultResources.Permissions(localizer));

            return identityResources;
        }

        private static Task<ContentsResult<IdentityResourceEntity, IdentityResourceData>> GetIdentityResourcesAsync(SquidexClientManagerFactory factory)
        {
            var client = factory.GetContentsClient<IdentityResourceEntity, IdentityResourceData>("identity-resources");

            return client.GetAsync(context: Context.Build());
        }

        private static Task<ContentsResult<ApiResourceEntity, ApiResourceData>> GetApiResourcesAsync(SquidexClientManagerFactory factory)
        {
            var client = factory.GetContentsClient<ApiResourceEntity, ApiResourceData>("api-resources");

            return client.GetAsync(context: Context.Build());
        }

        private static Task<ContentsResult<ApiScopeEntity, ApiScopeData>> GetApiScopesAsync(SquidexClientManagerFactory factory)
        {
            var client = factory.GetContentsClient<ApiScopeEntity, ApiScopeData>("api-scopes");

            return client.GetAsync(context: Context.Build());
        }
    }
}
