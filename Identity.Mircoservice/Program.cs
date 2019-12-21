using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;

namespace Identity.Mircoservice
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var builder = new HostBuilder()
              .ConfigureServices((hostContext, services) =>
              {
                  services.AddHostedService<IdentityService>();
              });

            await builder
              .RunConsoleAsync();
        }
    }
}
