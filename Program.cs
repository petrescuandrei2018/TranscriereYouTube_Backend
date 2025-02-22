using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Converters;
using System.IO;
using TranscriereYouTube_Backend.Interfaces;
using TranscriereYouTube_Backend.Utils;
using Xceed.Document.NET;

namespace TranscriereYouTube
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ✅ Încarcă fișierul appsettings.json din folderul Config
            var configPath = Path.Combine(Directory.GetCurrentDirectory(), "Config", "appsettings.json");
            builder.Configuration.SetBasePath(Directory.GetCurrentDirectory())
                                 .AddJsonFile(configPath, optional: false, reloadOnChange: true);

            // ✅ Înregistrăm Configuration în DI
            builder.Services.AddSingleton<IConfiguration>(builder.Configuration);

            // ✅ Înregistrăm serviciile necesare
            builder.Services.AddSingleton<IProcessRunner, ProcessRunner>();
            builder.Services.AddSingleton<ICommandFactory, CommandFactory>();
            builder.Services.AddSingleton<IDescarcatorService, DescarcatorService>();
            builder.Services.AddSingleton<IProcesorAudioService, ProcesorAudioService>();
            builder.Services.AddSingleton<IProcesorVideoService, ProcesorVideoService>();
            builder.Services.AddSingleton<ITranscriereService, TranscriereService>();
            builder.Services.AddSingleton<ITranscriereFacade, TranscriereFacade>();
            builder.Services.AddSingleton<ILoggerService, LoggerService>();
            builder.Services.AddSingleton<ITranscriereValidator, TranscriereValidator>();
            builder.Services.AddSingleton<IWebSocketService, WebSocketService>();
            builder.Services.AddSingleton<TestProcesExtern>();
            builder.Services.AddSingleton<IVideoDownloader, VideoDownloader>();

            builder.Services.AddScoped<IYtDlpService, YtDlpService>();
            builder.Services.AddScoped<IFFmpegService, FFmpegService>();
            builder.Services.AddScoped<IWhisperService, WhisperService>();
            builder.Services.AddScoped<IErrorHandler, ErrorHandler>();

            // ✅ Configurare controlere cu Newtonsoft.Json fără conversia enum-urilor
            builder.Services.AddControllers()
                .AddNewtonsoftJson(options =>
                {
                    // Eliminăm conversia automată a enum-urilor în string-uri
                    options.SerializerSettings.Converters.Remove(new StringEnumConverter());
                });

            builder.Services.AddEndpointsApiExplorer();

            // ✅ Configurare Swagger
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "API Transcriere YouTube",
                    Version = "v1",
                    Description = "API pentru transcriere video/audio folosind Whisper și FFmpeg"
                });

                // ✅ Adăugăm suport pentru comentarii XML în Swagger (dacă folosim)
                var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                if (File.Exists(xmlPath))
                {
                    options.IncludeXmlComments(xmlPath);
                }

                
            });

            // ✅ Integrare Swagger cu Newtonsoft.Json
            builder.Services.AddSwaggerGenNewtonsoftSupport();

            var app = builder.Build();

            // ✅ Configurare Swagger în mediu de dezvoltare
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "API Transcriere YouTube v1");
                    c.RoutePrefix = string.Empty; // Swagger UI la rădăcină
                });
            }

            // ✅ Middleware pentru routing și endpoints
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            // ✅ Rulează aplicația
            app.Run();
        }
    }
}
