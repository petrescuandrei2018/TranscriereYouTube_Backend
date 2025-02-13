using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TranscriereYouTube.Services;
using TranscriereYouTube.Interfaces;
using TranscriereYouTube.Factories;
using TranscriereYouTube.Facades;

namespace TranscriereYouTube
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddScoped<IDescarcatorService, DescarcatorService>();
            services.AddScoped<IProcesorVideoService, ProcesorVideoService>();
            services.AddScoped<ITranscriereService, TranscriereService>();
            services.AddScoped<ITranscriereFacade, TranscriereFacade>();
            services.AddSingleton<ServiceFactory>();
            services.AddScoped<ITranscriereService, TranscriereService>();

        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}