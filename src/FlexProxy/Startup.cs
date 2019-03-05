using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FlexProxy.Core;
using FlexProxy.Core.Models;
using FlexProxy.Core.Services;
using FlexProxy.ExceptionHandlerMiddleware;
using FlexProxy.HealthCheckMiddleware;
using FlexProxy.RequestTracerMiddleware;
using FlexProxy.RobotsMiddleware;
using FlexProxy.SessionHandlerMiddleware;
using FlexProxy.WebProxyMiddleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FlexProxy
{
    public class Startup
    {
        private readonly ILogger _logger;
        private readonly ILoggerFactory _loggerFactory;
        private IServiceProvider _serviceProvider;
        private string[] _args;

        static public IConfigurationRoot Configuration { get; set; }

        public Startup(IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
            loggerFactory.AddConsole();
            _logger = loggerFactory.CreateLogger<Startup>();
            _args = Environment.GetCommandLineArgs().Skip(1).ToArray();
            _logger.LogDebug($"Startup args: {string.Join(" ", _args)}");

            var builder = new ConfigurationBuilder()
                .AddCommandLine(_args)
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile($"appsettings.json")
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", true);

            Configuration = builder.Build();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            ConfigureOptions(services);

            services.AddSingleton<IRequestTraceLoggerService, RequestTraceLoggerService>();
            services.AddResponseCompression();
            services.AddWebProxy();
        }

        internal void ConfigureOptions(IServiceCollection services)
        {
            services.Configure<HostMappingOptions>(options =>
            {
                options.ServingHost = Configuration["ServingHost"];
                options.ServingScheme = Configuration["ServingScheme"];
                options.DownstreamHost = Configuration["DownstreamHost"];
                options.DownstreamScheme = Configuration["DownstreamScheme"];
            });

            services.Configure<GzipCompressionProviderOptions>(options =>
            {
                options.Level = CompressionLevel.Fastest;
            });

            services.Configure<ResponseCompressionOptions>(options =>
            {
                options.Providers.Add<GzipCompressionProvider>();
                options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[] { "text/javascript" });
            });

            services.Configure<WebProxyOptions>(options =>
            {
                options.InternalCookies = new List<string> { Configuration["EventSessionCookieName"] };
            });

            services.Configure<SessionHandlerOptions>(options =>
            {
                options.EventSessionCookieName = Configuration["EventSessionCookieName"];
            });

            services.Configure<HealthCheckOptions>(options =>
            {
                options.Localhost = IPAddress.Parse("127.0.0.1");
                options.MaxResponseTimeInSeconds = int.Parse(Configuration["MaxResponseTimeInSeconds"]);
            });

            services.Configure<RequestTraceLoggerServiceOptions>(options =>
            {
                options.LoggerConfigName = Constants.REQUEST_TRACELOGGER_NAME;
                options.Interval = Constants.REQUEST_TRACELOGGER_INTERVAL;
                options.ProcessName = Configuration["ServingHost"];
            });
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseHealthCheck();
            app.UseRequestTracer();
            app.UseCustomExceptionHandler();
            app.UseRobots();
            app.UseResponseCompression();
            app.UseSessionHandler();
            app.UseWebProxy();
        }
    }
}
