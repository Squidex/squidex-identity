// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;

namespace Squidex.Identity.Model
{
    public sealed class ApiResourceData
    {
        public string Name { get; set; }

        public string DisplayName { get; set; }

        public string Description { get; set; }

        public string[] UserClaims { get; set; }

        public bool Enabled { get; set; }

        public List<Guid> Scopes { get; set; }
    }
}
