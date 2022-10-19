using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Tests;

public class MoqStartup : IStartup
{
	public void Configure(IApplicationBuilder app)
	{
	}

	IServiceProvider IStartup.ConfigureServices(IServiceCollection services)
	{
		return services.BuildServiceProvider();
	}
}