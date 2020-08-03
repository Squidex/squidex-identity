// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Identity.Model
{
    public sealed class ClientData
    {
        public string ClientId { get; set; }

        public string ClientName { get; set; }

        public string ClientUri { get; set; }

        public string[] AllowedScopes { get; set; }

        public string[] ClientSecrets { get; set; }

        public string[] Logo { get; set; }

        public string[] AllowedCorsOrigins { get; set; }

        public string[] AllowedGrantTypes { get; set; }

        public string[] RedirectUris { get; set; }

        public string[] PostLogoutRedirectUris { get; set; }

        public bool AllowOfflineAccess { get; set; }

        public bool RequireConsent { get; set; }

        public bool Disabled { get; set; }
    }
}
