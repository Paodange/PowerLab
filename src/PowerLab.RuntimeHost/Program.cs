using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;

namespace PowerLab.RuntimeHost
{
    internal static class Program
    {
        public static Task Main(string[] args)
        {
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
            WebApplication app = builder.Build();

            return app.RunAsync();
        }
    }
}
