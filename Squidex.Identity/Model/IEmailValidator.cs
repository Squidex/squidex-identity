// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace Squidex.Identity.Model
{
    public interface IEmailValidator
    {
        Task<IdentityResult> ValidateAsync(string email);
    }
}
