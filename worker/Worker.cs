using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace worker {

    public class Worker : BackgroundService 
    {
	public Worker(ILogger<Worker> logger)
	{
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
	    while (!stoppingToken.IsCancellationRequested)
	    {
		Console.WriteLine("Working...");
		await Task.Delay(1000, stoppingToken);
	    }
	}
    }
}
