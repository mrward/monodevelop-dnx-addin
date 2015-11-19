
using System;
using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Http;
using Microsoft.Extensions.DependencyInjection;

namespace ${Namespace}
{
	public class Startup
	{
		public void ConfigureServices(IServiceCollection services)
		{
		}

		public void Configure(IApplicationBuilder app)
		{
			app.Run(async (context) => {
				await context.Response.WriteAsync("Hello World!");
			});
		}

		public static void Main(string[] args)
		{
			WebApplication.Run<Startup>(args);
		}
	}
}

