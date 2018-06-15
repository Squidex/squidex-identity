// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using SharpPwned.NET;

namespace Squidex.Identity.Model
{
    public sealed class PwnedPasswordValidator : IPasswordValidator<UserEntity>
    {
        private const string ErrorCode = "PwnedError";
        private readonly HaveIBeenPwnedRestClient pwned;
        private readonly ILogger log;
        private readonly IStringLocalizer localizer;

        public PwnedPasswordValidator(HaveIBeenPwnedRestClient pwned,
            ILogger<PwnedEmailValidator> log, IStringLocalizer<AppResources> localizer)
        {
            this.pwned = pwned;

            this.log = log;
            this.localizer = localizer;
        }

        public async Task<IdentityResult> ValidateAsync(UserManager<UserEntity> manager, UserEntity user, string password)
        {
            try
            {
                var isBreached = await pwned.IsPasswordPwned(password);

                if (isBreached)
                {
                    var errorMessage = localizer["PwnedPasswordError"];

                    return IdentityResult.Failed(new IdentityError { Code = ErrorCode, Description = errorMessage });
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Failed to validate password with haveibeenpowned.com");
            }

            return IdentityResult.Success;
        }
    }
}
