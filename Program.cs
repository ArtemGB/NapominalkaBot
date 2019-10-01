using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Server.Kestrel;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using System.Net;
using System.Diagnostics;
using FluentScheduler;

namespace VkBot
{
    public class Program
    {
        public static void Main(string[] args)
        {
            JobManager.Initialize(new ReminRegistry()); //Регистрируем задачу таймера напоминаний.
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost (string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
            .UseKestrel(options =>
            {
                options.Limits.MaxConcurrentConnections = 1000;
                options.Limits.MaxRequestBodySize = 10 * 1024;
                options.Limits.MinRequestBodyDataRate =
                    new MinDataRate(bytesPerSecond: 100, gracePeriod: TimeSpan.FromSeconds(10));
                options.Limits.MinResponseDataRate =
                    new MinDataRate(bytesPerSecond: 100, gracePeriod: TimeSpan.FromSeconds(10));
                options.Listen(IPAddress.Loopback, 5000);
                options.Limits.KeepAliveTimeout = TimeSpan.FromDays(10);
            })
            .Build();
    }
}
