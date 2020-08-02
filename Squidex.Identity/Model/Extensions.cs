// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using IdentityServer4.Models;

namespace Squidex.Identity.Model
{
    public static class Extensions
    {
        public static List<Secret> ToSecrets(this string[] value)
        {
            return value?.Select(x => new Secret(x.Sha256()))?.ToList() ?? new List<Secret>();
        }

        public static IEnumerable<T> OrDefault<T>(this IEnumerable<T> value)
        {
            return value ?? Enumerable.Empty<T>();
        }

        public static ICollection<T> OrDefault<T>(this ICollection<T> value)
        {
            return value ?? new List<T>();
        }

        public static string OrDefault<T>(this string value)
        {
            return value ?? string.Empty;
        }
    }
}
