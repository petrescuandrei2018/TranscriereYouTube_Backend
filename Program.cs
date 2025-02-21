using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TranscriereYouTube_Backend.Interfaces;
using TranscriereYouTube_Backend.Utils;

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

            // Înregistrăm serviciile necesare
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


            // Configurare controlere și Swagger
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configurare Swagger
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "API Transcriere YouTube v1");
                c.RoutePrefix = string.Empty;
            });

            // Middleware pentru routing
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
