// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using IdentityServer4.Stores;
using IdentityServer4.Validation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Facebook;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;
using Microsoft.AspNetCore.Authentication.Twitter;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using SharpPwned.NET;
using Squidex.Identity.Model;
using Squidex.Identity.Model.Authentication;
using Squidex.Identity.Stores.MongoDb;

namespace Squidex.Identity
{
    public static class ConfigExtensions
    {
        public static void AddMyIdentityServer(this IServiceCollection services)
        {
            services.AddIdentityServer(options =>
            {
                options.Events.RaiseSuccessEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseErrorEvents = true;
                options.Events.RaiseInformationEvents = true;
                options.UserInteraction.LoginUrl = "/login";
                options.UserInteraction.LogoutUrl = "/logout";
                options.UserInteraction.ErrorUrl = "/error";
                options.UserInteraction.ConsentUrl = "/consent";
            })
            .AddAppAuthRedirectUriValidator()
            .AddAspNetIdentity<UserEntity>()
            .AddClientConfigurationValidator<DefaultClientConfigurationValidator>()
            .AddClientStore<ClientStore>()
            .AddInMemoryCaching()
            .AddResourceStore<ResourceStore>();

            /*
            services.AddSingleton<MongoKeyStore>();

            services.AddSingleton<ISigningCredentialStore>(
                c => c.GetRequiredService<MongoKeyStore>());

            services.AddSingleton<IValidationKeysStore>(
                c => c.GetRequiredService<MongoKeyStore>());*/
        }

        public static void AddMyIdentity(this IServiceCollection services)
        {
            services.AddIdentity<UserEntity, RoleEntity>()
                .AddUserStore<UserStore>()
                .AddRoleStore<RoleStore>()
                .AddDefaultTokenProviders();

            services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.Name = ".sq.id.auth";
                options.LoginPath = "/login";
                options.LogoutPath = "/logout";
                options.AccessDeniedPath = "/accessdenied";
            });

            services.AddSingleton<HaveIBeenPwnedRestClient>();

            services.AddSingleton<IPasswordValidator<UserEntity>,
                PwnedPasswordValidator>();

            services.AddSingleton<IEmailValidator,
                PwnedEmailValidator>();
        }

        public static void AddMyAuthentication(this IServiceCollection services)
        {
            services.AddAuthentication()
                .AddFacebook()
                .AddGoogle()
                .AddMicrosoftAccount()
                .AddTwitter();

            services.AddSingleton<IAuthenticationSchemeProvider,
                SquidexAuthenticationSchemeProvider>();

            services.AddSingleton<IAuthenticationSchemeStore,
                AuthenticationSchemeStore>();

            services.AddAuthenticationConfigurator<FacebookOptions, FacebookHandler>(
                AuthenticationSchemeType.Facebook, Factories.OAuth<FacebookOptions>);

            services.AddAuthenticationConfigurator<GoogleOptions, GoogleHandler>(
                AuthenticationSchemeType.Google, Factories.OAuth<GoogleOptions>);

            services.AddAuthenticationConfigurator<MicrosoftAccountOptions, MicrosoftAccountHandler>(
                AuthenticationSchemeType.Microsoft, Factories.OAuth<MicrosoftAccountOptions>);

            services.AddAuthenticationConfigurator<TwitterOptions, TwitterHandler>(
                AuthenticationSchemeType.Twitter, Factories.Twitter);
        }
    }
}
