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
    public sealed class PwnedEmailValidator : IEmailValidator
    {
        private const string ErrorCode = "PwnedError";
        private readonly HaveIBeenPwnedRestClient pwned;
        private readonly ILogger log;
        private readonly IStringLocalizer localizer;

        public PwnedEmailValidator(HaveIBeenPwnedRestClient pwned,
            ILogger<PwnedEmailValidator> log, IStringLocalizer<AppResources> localizer)
        {
            this.pwned = pwned;

            this.log = log;
            this.localizer = localizer;
        }

        public async Task<IdentityResult> ValidateAsync(string email)
        {
            try
            {
                var breaches = await pwned.GetAccountBreaches(email);

                if (breaches.Count > 0)
                {
                    var errorMessage = localizer["PwnedEmailError"];

                    return IdentityResult.Failed(new IdentityError { Code = ErrorCode, Description = errorMessage });
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Failed to validate email with haveibeenpowned.com");
            }

            return IdentityResult.Success;
        }
    }
}
