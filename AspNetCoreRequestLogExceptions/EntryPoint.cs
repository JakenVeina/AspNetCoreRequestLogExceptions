using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AspNetCoreRequestLogExceptions
{
    public static class EntryPoint
    {
        public static void Main()
        {
            using var host = new HostBuilder()
                .ConfigureAppConfiguration(builder => builder
                    .AddJsonFile("appsettings.json"))
                .ConfigureLogging((context, builder) =>
                {
                    builder.AddConfiguration(context.Configuration.GetSection("Logging"));

                    //builder.AddProvider(new ForegroundLoggerProvider());
                    builder.AddProvider(new BackgroundLoggerProvider());
                })
                .ConfigureWebHost(builder => builder
                    .UseKestrel()
                    .Configure(builder => builder
                        .Use(next => context => context.Response.WriteAsync("Hello World!"))))
                .Build();

            host.Run();
        }
    }
}
