public interface IDescarcatorService
{
    Task<Result<string>> DescarcaVideoAsync(string videoUrl);
}
