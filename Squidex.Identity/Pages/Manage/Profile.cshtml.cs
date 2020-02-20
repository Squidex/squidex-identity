// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Squidex.Identity.Extensions;

#pragma warning disable SA1649 // File name should match first type name

namespace Squidex.Identity.Pages.Manage
{
    public sealed class ProfileModel : ManagePageModelBase<ProfileModel>
    {
        [BindProperty]
        public ChangeProfileInputModel Input { get; set; }

        public bool IsEmailConfirmed { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            Input = new ChangeProfileInputModel { Email = UserInfo.Data.Email };

            IsEmailConfirmed = await UserManager.IsEmailConfirmedAsync(UserInfo);

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                if (!string.Equals(Input.Email, UserInfo.Data.Email, StringComparison.OrdinalIgnoreCase))
                {
                    var result = await UserManager.SetEmailAsync(UserInfo, Input.Email);

                    if (!result.Succeeded)
                    {
                        throw new ApplicationException($"Unexpected error occurred setting email for user with ID '{UserInfo.Id}'.");
                    }

                    StatusMessage = T["ProfileUpdated"];
                }

                return RedirectToPage();
            }

            return Page();
        }

        public IActionResult OnPostSendVerificationEmail()
        {
            if (ModelState.IsValid)
            {
                StatusMessage = T["VerificationEmailSent"];

                return RedirectToPage();
            }

            return Page();
        }
    }

    public sealed class ChangeProfileInputModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = nameof(Email))]
        public string Email { get; set; }
    }
}
