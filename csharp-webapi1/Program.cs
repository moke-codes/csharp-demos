using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

Host.CreateDefaultBuilder(args)
    .ConfigureWebHostDefaults(webBuilder =>
    {
        webBuilder
            .ConfigureServices(services => services.AddControllers())
            .Configure(app => {
                app
                    .UseRouting()
                    .UseEndpoints(endpoints => {
                        [HttpGet] 
                        Task HelloWorld(HttpContext ctx) => 
                            ctx.Response.WriteAsync("Hello World!");

                        endpoints.Map("/", HelloWorld);
                        endpoints.MapControllers();
            });
        });
    })
    .Build()
    .Run();