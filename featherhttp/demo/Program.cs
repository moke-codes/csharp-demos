using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace demo
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var app = WebApplication.Create(args);

            app.MapGet("/{name?}",  async http =>
            {
                http.Request.RouteValues.TryGet("name", out string name);
                name ??= "World";
    
                await http.Response.WriteAsync($"Hello {name}!");
            });

            await app.RunAsync();
        }
    }
}
