namespace mongodb_dotnet_example.Models
{

    public class AppSettings : IAppSettings
    {
        public string JwtSecretKey { get; set; }
    }

    public interface IAppSettings
    {
        string JwtSecretKey { get; set; }
    }
}