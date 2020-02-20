// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Squidex.Identity.Extensions;
using Squidex.Identity.Services;

#pragma warning disable SA1649 // File name should match first type name

namespace Squidex.Identity.Pages
{
    public sealed class ForgotPasswordModel : PageModelBase<ForgotPasswordModel>
    {
        private readonly IEmailSender emailSender;

        public ForgotPasswordModel(IEmailSender emailSender)
        {
            this.emailSender = emailSender;
        }

        [BindProperty]
        public ForgotPasswordInputModel Input { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByEmailAsync(Input.Email);

                if (user != null && await UserManager.IsEmailConfirmedAsync(user))
                {
                    var callbackCode = await UserManager.GeneratePasswordResetTokenAsync(user);
                    var callbackUrl = Url.ResetPasswordCallbackLink(user.Id.ToString(), callbackCode, Request.Scheme);

                    await emailSender.SendResetPasswordAsync(Input.Email, callbackUrl);
                }

                return RedirectToPage("./ForgotPasswordConfirmation");
            }

            return Page();
        }
    }

    public sealed class ForgotPasswordInputModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = nameof(Email))]
        public string Email { get; set; }
    }
}
