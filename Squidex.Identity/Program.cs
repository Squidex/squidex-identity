// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Net;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Squidex.Identity
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .ConfigureKestrel((context, serverOptions) =>
                {
                    if (context.HostingEnvironment.IsDevelopment() || context.Configuration.GetValue<bool>("devMode:enable"))
                    {
                        serverOptions.Listen(
                            IPAddress.Any,
                            3501,
                            listenOptions => listenOptions.UseHttps("../dev/squidex-dev.pfx", "password"));
                    }
                })
                .UseStartup<Startup>()
                .Build();
    }
}
