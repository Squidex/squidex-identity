// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Identity.Model
{
    public sealed class ApiScopeData
    {
        public string Name { get; set; }

        public string DisplayName { get; set; }

        public string Description { get; set; }

        public string[] UserClaims { get; set; }

        public bool Required { get; set; }

        public bool Enabled { get; set; }

        public bool Emphasize { get; set; }
    }
}
