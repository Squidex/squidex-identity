// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Globalization;
using Squidex.ClientLibrary;

namespace Squidex.Identity.Model
{
    public static class Context
    {
        public static QueryContext Build()
        {
            return QueryContext.Default.Flatten().WithLanguages(CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
        }
    }
}
