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

            // 1️⃣ ✅ Adăugare servicii
            builder.Services.AddControllers();
            builder.Services.AddSwaggerGen();
            builder.Services.AddScoped<IDescarcatorService, DescarcatorService>();
            builder.Services.AddScoped<IProcesorVideoService, ProcesorVideoService>();
            builder.Services.AddScoped<ITranscriereService, TranscriereService>();
            builder.Services.AddScoped<ITranscriereFacade, TranscriereFacade>();
            builder.Services.AddSingleton<ServiceFactory>();

            var app = builder.Build();

            // 2️⃣ ✅ Configurare pipeline middleware
            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "API Transcriere YouTube v1");
                    c.RoutePrefix = string.Empty; // Deschide direct pe root
                });
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.Run();
        }
    }
}
