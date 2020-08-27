using System;
using CRMToPostgresDataPipeline.Infrastructure;
using CRMToPostgresDataPipeline.lib;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CRMToPostgresDataPipeline
{
    public static class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public static void ConfigureServices(this IServiceCollection services)
        {
            services.AddScoped<IGetRecordsFromCrm, GetRecordsFromCrm>();
            services.AddScoped<IMapResponseToObject, MapResponseToObject>();
            services.AddScoped<ILoadRecordsIntoDatabase, LoadRecordsIntoDatabase>();

            services.AddHttpClient();

            var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING");

            services.AddDbContext<ResidentContactContext>(options => options.UseNpgsql(connectionString));
        }
    }
}