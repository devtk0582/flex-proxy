using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FlexProxy.ExceptionHandlerMiddleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
        }

        internal void ConfigureOptions(IServiceCollection services)
        {

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

            app.UseCustomExceptionHandler();

            app.Run(async context =>
            {
                await context.Response.WriteAsync("Test Response");
            });
        }
    }
}
