using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

var webApp = WebApplication.Create(args);

webApp.MapGet("/", async http => await http.Response.WriteAsync($"Hello world!"));

await webApp.RunAsync();