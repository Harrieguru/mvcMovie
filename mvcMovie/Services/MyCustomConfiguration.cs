
namespace mvcMovie.Services
{
    public class MyCustomConfiguration
    {
        public string AWSRegion { get; set; }
        public string Connection2RDS { get; set; }
        public string AWSAccessKey { get; set; }
        public string AWSSecretKey { get; set; }
        public string AllowedHosts { get; set; }

        // No constructor needed, as you will be populating properties manually
    }
}

