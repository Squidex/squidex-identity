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
using IdentityServer4;
using IdentityServer4.Events;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using IdentityServer4.Validation;
using Microsoft.AspNetCore.Mvc;
using Squidex.Identity.Extensions;

#pragma warning disable SA1649 // File name should match first type name

namespace Squidex.Identity.Pages
{
    public sealed class ConsentModel : PageModelBase<ConsentModel>
    {
        private readonly IClientStore clientStore;
        private readonly IEventService events;
        private readonly IIdentityServerInteractionService interaction;

        public ConsentModel(
            IClientStore clientStore,
            IEventService events,
            IIdentityServerInteractionService interaction)
        {
            this.clientStore = clientStore;
            this.events = events;
            this.interaction = interaction;
        }

        [BindProperty]
        public ConsentInputModel Input { get; set; }

        [BindProperty(SupportsGet = true)]
        public string ReturnUrl { get; set; }

        public string ClientName { get; set; }

        public string ClientUrl { get; set; }

        public string ClientLogoUrl { get; set; }

        public bool AllowRememberConsent { get; set; }

        public List<ScopeViewModel> IdentityScopes { get; set; }

        public List<ScopeViewModel> ApiScopes { get; set; } = new List<ScopeViewModel>();

        public Task OnGet()
        {
            return SetupModelAsync(null);
        }

        public async Task<IActionResult> OnPost()
        {
            var request = await interaction.GetAuthorizationContextAsync(ReturnUrl);

            if (request == null)
            {
                throw new ApplicationException($"No consent request matching request: {ReturnUrl}");
            }

            ConsentResponse grantedConsent = null;

            if (string.Equals(Input.Button, "NO", StringComparison.OrdinalIgnoreCase))
            {
                grantedConsent = new ConsentResponse { Error = AuthorizationError.AccessDenied };

                await events.RaiseAsync(
                    new ConsentDeniedEvent(
                        User.GetSubjectId(),
                        request.Client.ClientId,
                        request.ValidatedResources.RawScopeValues));
            }
            else if (string.Equals(Input.Button, "YES", StringComparison.OrdinalIgnoreCase))
            {
                if (Input.ScopesConsented != null && Input.ScopesConsented.Any())
                {
                    var scopes = Input.ScopesConsented;

                    grantedConsent = new ConsentResponse
                    {
                        RememberConsent = Input.RememberConsent, ScopesValuesConsented = scopes.ToArray()
                    };

                    await events.RaiseAsync(
                        new ConsentGrantedEvent(
                            User.GetSubjectId(),
                            request.Client.ClientId,
                            request.ValidatedResources.RawScopeValues,
                            grantedConsent.ScopesValuesConsented,
                            grantedConsent.RememberConsent));
                }
                else
                {
                    ModelState.AddModelError(string.Empty, T["MustChooseOneErrorMessage"]);
                }
            }
            else
            {
                ModelState.AddModelError(string.Empty, T["InvalidSelectionErrorMessage"]);
            }

            if (grantedConsent != null)
            {
                await interaction.GrantConsentAsync(request, grantedConsent);

                return RedirectTo(ReturnUrl);
            }
            else
            {
                await SetupModelAsync(Input);
            }

            return Page();
        }

        private async Task SetupModelAsync(ConsentInputModel input)
        {
            var request = await interaction.GetAuthorizationContextAsync(ReturnUrl);

            if (request == null)
            {
                throw new ApplicationException($"No consent request matching request: {ReturnUrl}");
            }

            var client = await clientStore.FindEnabledClientByIdAsync(request.Client.ClientId);

            if (client == null)
            {
                throw new ApplicationException($"Invalid client id: {request.Client.ClientId}");
            }

            var resources = request.ValidatedResources.Resources.IdentityResources;

            Input ??= new ConsentInputModel();
            Input.ScopesConsented = input?.ScopesConsented ?? Enumerable.Empty<string>();

            AllowRememberConsent = client.AllowRememberConsent;

            ClientName = client.ClientName ?? client.ClientId;
            ClientUrl = client.ClientUri;
            ClientLogoUrl = client.LogoUri;

            IdentityScopes = resources.Select(x => CreateScopeViewModel(x, input == null || input.ScopesConsented.Contains(x.Name))).ToList();

            foreach (var parsedScope in request.ValidatedResources.ParsedScopes)
            {
                var apiScope = request.ValidatedResources.Resources.FindApiScope(parsedScope.ParsedName);
                if (apiScope != null)
                {
                    var scopeVm = CreateScopeViewModel(parsedScope, apiScope, input.ScopesConsented.Contains(parsedScope.RawValue) || input == null);

                    ApiScopes.Add(scopeVm);
                }
            }

            if (request.ValidatedResources.Resources.OfflineAccess)
            {
                ApiScopes.Add(
                    new ScopeViewModel
                    {
                        Name = IdentityServerConstants.StandardScopes.OfflineAccess,
                        DisplayName = T["OfflineAccessDisplayName"],
                        Description = T["OfflineAccessDescription"],
                        Emphasize = true,
                        Checked = input == null || input.ScopesConsented.Contains(IdentityServerConstants.StandardScopes.OfflineAccess)
                    });
            }
        }

        private ScopeViewModel CreateScopeViewModel(IdentityResource identity, bool check)
        {
            return new ScopeViewModel
            {
                Name = identity.Name,
                DisplayName = identity.DisplayName,
                Description = identity.Description,
                Emphasize = identity.Emphasize,
                Required = identity.Required,
                Checked = check || identity.Required
            };
        }

        private ScopeViewModel CreateScopeViewModel(ParsedScopeValue parsedScopeValue, ApiScope scope, bool check)
        {
            var displayName = scope.DisplayName ?? scope.Name;

            if (!string.IsNullOrWhiteSpace(parsedScopeValue.ParsedParameter))
            {
                displayName += ":" + parsedScopeValue.ParsedParameter;
            }

            return new ScopeViewModel
            {
                Name = scope.Name,
                DisplayName = displayName,
                Description = scope.Description,
                Emphasize = scope.Emphasize,
                Required = scope.Required,
                Checked = check || scope.Required
            };
        }
    }

    public sealed class ConsentInputModel
    {
        public IEnumerable<string> ScopesConsented { get; set; }

        public string Button { get; set; }

        public bool RememberConsent { get; set; }
    }

    public sealed class ScopeViewModel
    {
        public string Name { get; set; }

        public string DisplayName { get; set; }

        public string Description { get; set; }

        public bool Emphasize { get; set; }

        public bool Required { get; set; }

        public bool Checked { get; set; }
    }
}
