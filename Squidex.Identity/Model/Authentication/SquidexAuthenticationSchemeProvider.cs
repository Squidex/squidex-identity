// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Facebook;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;
using Microsoft.AspNetCore.Authentication.Twitter;
using Microsoft.Extensions.Options;

namespace Squidex.Identity.Model.Authentication
{
    public sealed class SquidexAuthenticationSchemeProvider : AuthenticationSchemeProvider, IAuthenticationSchemeProvider
    {
        private readonly List<AuthenticationScheme> defaultSchemes = new List<AuthenticationScheme>();
        private readonly IOptions<AuthenticationOptions> options;
        private readonly IAuthenticationSchemeStore store;

        public SquidexAuthenticationSchemeProvider(
            IAuthenticationSchemeStore store,
            IEnumerable<IAuthenticationSchemeConfigurator> configurators,
            IOptions<AuthenticationOptions> options)
            : base(options)
        {
            this.options = options;

            foreach (var builder in options.Value.Schemes)
            {
                var scheme = builder.Build();

                if (!configurators.Any(x => x.HandlerType == scheme.HandlerType))
                {
                    defaultSchemes.Add(scheme);
                }
            }

            this.store = store;
        }

        public override async Task<IEnumerable<AuthenticationScheme>> GetRequestHandlerSchemesAsync()
        {
            var schemes = await GetSchemesCoreAsync();

            var result = schemes.Where(x => typeof(IAuthenticationRequestHandler).IsAssignableFrom(x.HandlerType));

            return result;
        }

        public override async Task<AuthenticationScheme> GetSchemeAsync(string name)
        {
            var schemes = await GetSchemesCoreAsync();

            var result = schemes.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.Ordinal));

            return result;
        }

        public async override Task<IEnumerable<AuthenticationScheme>> GetAllSchemesAsync()
        {
            var result = await GetSchemesCoreAsync();

            return result;
        }

        private async Task<IEnumerable<AuthenticationScheme>> GetSchemesCoreAsync()
        {
            var schemes = await store.GetSchemesAsync();

            var result = new List<AuthenticationScheme>(defaultSchemes);

            foreach (var scheme in schemes)
            {
                switch (scheme.Provider)
                {
                    case AuthenticationSchemeType.Facebook:
                        result.Add(new AuthenticationScheme(FacebookDefaults.AuthenticationScheme, "Facebook", typeof(FacebookHandler)));
                        break;
                    case AuthenticationSchemeType.Google:
                        result.Add(new AuthenticationScheme(GoogleDefaults.AuthenticationScheme, "Google", typeof(GoogleHandler)));
                        break;
                    case AuthenticationSchemeType.Microsoft:
                        result.Add(new AuthenticationScheme(MicrosoftAccountDefaults.AuthenticationScheme, "Microsoft", typeof(MicrosoftAccountHandler)));
                        break;
                    case AuthenticationSchemeType.Twitter:
                        result.Add(new AuthenticationScheme(TwitterDefaults.AuthenticationScheme, "Twitter", typeof(TwitterHandler)));
                        break;
                }
            }

            return result;
        }
    }
}