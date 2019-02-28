using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FlexProxy
{
    public class Program
    {
        static public IConfigurationRoot Configuration { get; set; }

        public static void Main(string[] args)
        {
            var argsString = string.Join(" ", args);

            try
            {
                Configuration = new ConfigurationBuilder()
                    .AddCommandLine(args)
                    .Build();

                CreateWebHostBuilder(args).Build().Run();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error starting Web Listener: \r\n Args: \r\n {argsString} \r\n {e}");
            }    
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseKestrel()
                .UseConfiguration(Configuration)
                .UseUrls(Configuration["urls"])
                .UseStartup<Startup>();
    }
}
