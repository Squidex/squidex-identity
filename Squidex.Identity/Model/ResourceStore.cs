﻿// ==========================================================================
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
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Localization;
using Squidex.ClientLibrary;

namespace Squidex.Identity.Model
{
    public sealed class ResourceStore : CachingProvider, IResourceStore
    {
        private readonly SquidexClient<ResourceEntity, ResourceData> apiApiResources;
        private readonly SquidexClient<ResourceEntity, ResourceData> apiIdentityResources;
        private readonly IStringLocalizer<AppResources> localizer;

        public ResourceStore(IMemoryCache cache, SquidexClientManager clientManager, IStringLocalizer<AppResources> localizer)
            : base(cache)
        {
            apiApiResources = clientManager.GetClient<ResourceEntity, ResourceData>("api-resources");
            apiIdentityResources = clientManager.GetClient<ResourceEntity, ResourceData>("identity-resources");

            this.localizer = localizer;
        }

        public async Task<Resources> GetAllResourcesAsync()
        {
            var resources = await GetResourcesAsync();

            return new Resources(resources.IdentityResources, resources.ApiResources);
        }

        public async Task<ApiResource> FindApiResourceAsync(string name)
        {
            var resources = await GetResourcesAsync();

            return resources.ApiResources.FirstOrDefault(x => x.Name == name);
        }

        public async Task<IEnumerable<ApiResource>> FindApiResourcesByScopeAsync(IEnumerable<string> scopeNames)
        {
            var resources = await GetResourcesAsync();

            return resources.ApiResources.Where(x => scopeNames.Contains(x.Name));
        }

        public async Task<IEnumerable<IdentityResource>> FindIdentityResourcesByScopeAsync(IEnumerable<string> scopeNames)
        {
            var resources = await GetResourcesAsync();

            return resources.IdentityResources.Where(x => scopeNames.Contains(x.Name));
        }

        private Task<(List<IdentityResource> IdentityResources, List<ApiResource> ApiResources)> GetResourcesAsync()
        {
            return GetOrAddAsync(nameof(ResourceStore), async () =>
            {
                var taskForApiResources = apiApiResources.GetAsync(context: Context.Build());
                var taskForIdentityResources = apiIdentityResources.GetAsync(context: Context.Build());

                await Task.WhenAll(taskForApiResources, taskForIdentityResources);

                var identityResources = taskForIdentityResources.Result.Items.Select(x =>
                {
                    var identityResource = new IdentityResource(x.Data.Name, x.Data.DisplayName, x.Data.UserClaims.OrDefault())
                    {
                        Description = x.Data.Description,
                        Emphasize = true,
                        Required = x.Data.Required
                    };

                    return identityResource;
                }).ToList();

                identityResources.Add(new IdentityResources.OpenId());
                identityResources.Add(new IdentityResources.Profile());
                identityResources.Add(new IdentityResources.Email());
                identityResources.Add(new IdentityResources.Phone());
                identityResources.Add(new DefaultResources.Permissions(localizer));

                var apiResources = taskForApiResources.Result.Items.Select(x =>
                {
                    var apiResource = new ApiResource(x.Data.Name, x.Data.DisplayName, x.Data.UserClaims.OrDefault())
                    {
                        Description = x.Data.Description
                    };

                    apiResource.Scopes.First().Description = x.Data.Description;

                    return apiResource;
                }).ToList();

                return (identityResources, apiResources);
            });
        }
    }
}
