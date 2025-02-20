public interface ICommandFactory
{
    string CreateYtDlpCommand(string videoUrl, string outputPath);
    string CreateFfmpegCommand(string videoPath, string audioOutputPath);
    string CreateWhisperCommand(string audioPath, string language);
}
