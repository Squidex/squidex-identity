// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.Events;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Squidex.Identity.Extensions;

#pragma warning disable SA1649 // File name should match first type name

namespace Squidex.Identity.Pages
{
    public sealed class LoginModel : PageModelBase<LoginModel>
    {
        private readonly IIdentityServerInteractionService interaction;

        [BindProperty]
        public LoginInputModel Input { get; set; }

        [BindProperty(SupportsGet = true)]
        public string ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public LoginModel(IIdentityServerInteractionService interaction)
        {
            this.interaction = interaction;
        }

        public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
        {
            ExternalLogins = (await SignInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            await next();
        }

        public async Task OnGetAsync()
        {
           // await HttpContext.SignOutAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var context = await interaction.GetAuthorizationContextAsync(ReturnUrl);

            if (ModelState.IsValid)
            {
                var result = await SignInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, true);

                if (result.Succeeded)
                {
                    var user = await UserManager.FindByNameAsync(Input.Email);

                    await Events.RaiseAsync(new UserLoginSuccessEvent(
                        user.Data.Username,
                        user.Id.ToString(),
                        user.Data.Username,
                        true,
                        clientId: context?.Client.ClientId));

                    return RedirectTo(ReturnUrl);
                }

                if (result.IsLockedOut)
                {
                    return RedirectToPage("./Lockout");
                }

                await Events.RaiseAsync(new UserLoginFailureEvent(Input.Email, "invalid credentials",
                    true,
                    clientId: context?.Client.ClientId));

                ModelState.AddModelError(string.Empty, T["InvalidLoginAttempt"]);
            }

            return Page();
        }
    }

    public sealed class LoginInputModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = nameof(Email))]
        public string Email { get; set; }

        [Required]
        [Display(Name = nameof(Password))]
        public string Password { get; set; }

        [Required]
        [Display(Name = nameof(RememberMe))]
        public bool RememberMe { get; set; }
    }
}
