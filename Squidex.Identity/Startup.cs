// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.DataProtection.Repositories;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Squidex.ClientLibrary;
using Squidex.Identity.Extensions;
using Squidex.Identity.Model;
using Squidex.Identity.Services;
using Squidex.Identity.Stores.MongoDb;
using Westwind.AspNetCore.LiveReload;

namespace Squidex.Identity
{
    public sealed class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<SquidexOptions>(Configuration.GetSection("app"));
            services.Configure<SquidexOptionsPerHost>(Configuration.GetSection("app:hosts"));
            services.Configure<SettingsData>(Configuration.GetSection("defaultSettings"));

            services.AddSingleton<SquidexClientManagerFactory>();
            services.AddScoped(c => c.GetRequiredService<SquidexClientManagerFactory>().GetClientManager());

            services.AddMemoryCache();
            services.AddNonBreakingSameSiteCookies();

            services.AddLiveReload();
            services.AddLocalization(options =>
            {
                options.ResourcesPath = "Resources";
            });

            services.AddRouting(options =>
            {
                options.LowercaseUrls = true;
            });

            services.AddSingleton<IConfigureOptions<KeyManagementOptions>>(s =>
            {
                return new ConfigureOptions<KeyManagementOptions>(options =>
                {
                    options.XmlRepository = s.GetRequiredService<IXmlRepository>();
                });
            });

            services.AddDataProtection().SetApplicationName("SquidexIdentity");

            services.AddMyIdentity();
            services.AddMyIdentityServer();
            services.AddMyAuthentication();

            services.AddMvc()
                .AddViewLocalization(options =>
                {
                    options.ResourcesPath = "Resources";
                })
                .AddDataAnnotationsLocalization(options =>
                {
                    options.DataAnnotationLocalizerProvider = (type, factory) => factory.Create(typeof(AppResources));
                })
                .AddRazorPagesOptions(options =>
                {
                    options.Conventions.AuthorizeFolder("/Manage");
                    options.Conventions.AuthorizePage("/Logout");
                });

            services.AddSingleton<ISettingsProvider,
                SettingsProvider>();

            services.AddSingleton<IEmailSender,
                EmailSender>();

            var storeType = Configuration.GetValue<string>("store:type");

            if (string.Equals(storeType, "MongoDB", StringComparison.OrdinalIgnoreCase))
            {
                services.AddMongoDB(Configuration);
            }
            else
            {
                throw new ApplicationException("You have to define the store with 'store:type'. Allowed values: MongoDB");
            }
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseCookiePolicy();

            app.UseCors(builder => builder
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());

            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.All,
                ForwardLimit = null,
                RequireHeaderSymmetry = false
            });

            var cultures = GetCultures();

            app.UseRequestLocalization(options =>
            {
                options.DefaultRequestCulture = new RequestCulture(cultures[0]);
                options.SupportedCultures = cultures;
                options.SupportedUICultures = cultures;
            });

            if (env.IsDevelopment())
            {
                // app.UseLiveReload();

                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseStaticFiles();

            app.UseRouting();

            app.UseIdentityServer();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapRazorPages();
            });
        }

        private List<CultureInfo> GetCultures()
        {
            var result = new List<CultureInfo>();

            var cultures = Configuration.GetValue<string>("app:cultures");

            if (!string.IsNullOrWhiteSpace(cultures))
            {
                foreach (var culture in cultures.Split(','))
                {
                    try
                    {
                        result.Add(CultureInfo.GetCultureInfo(culture.Trim()));
                    }
                    catch
                    {
                        continue;
                    }
                }
            }

            if (result.Count == 0)
            {
                result.Add(CultureInfo.GetCultureInfo("en"));
            }

            return result;
        }
    }
}
