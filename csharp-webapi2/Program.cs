using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

Host.CreateDefaultBuilder(args)
    .ConfigureWebHostDefaults(webBuilder =>
    {
        webBuilder.Configure(app => {
            app .UseRouting()
                .UseEndpoints(endpoints => {
                    endpoints.MapGet("/", async ctx => 
                        await ctx.Response.WriteAsync("Hello World!"));
            });
        });
    })
    .Build()
    .Run();