using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TranscriereYouTube.Services;
using TranscriereYouTube.Interfaces;
using TranscriereYouTube.Factories;
using TranscriereYouTube.Facades;

namespace TranscriereYouTube
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ✅ Adăugare servicii
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer(); // Necesită pentru Swagger
            builder.Services.AddSwaggerGen();

            // ✅ Înregistrare servicii
            builder.Services.AddScoped<IDescarcatorService, DescarcatorService>();
            builder.Services.AddScoped<IProcesorVideoService, ProcesorVideoService>();
            builder.Services.AddScoped<ITranscriereService, TranscriereService>();
            builder.Services.AddScoped<ITranscriereFacade, TranscriereFacade>();
            builder.Services.AddSingleton<ServiceFactory>();

            // Inițializare ServiceFactory cu IConfiguration
            var serviceFactory = new ServiceFactory(builder.Configuration);

            var app = builder.Build();

            // ✅ Configurare pipeline middleware

            // Activăm Swagger în toate mediile
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "API Transcriere YouTube v1");
                c.RoutePrefix = string.Empty; // Deschide Swagger pe root "/"
            });

            // Middleware pentru routing și controllere
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            // Rulează aplicația
            app.Run();
        }
    }
}
