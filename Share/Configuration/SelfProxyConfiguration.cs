namespace Mzr.Share.Configuration
{
    public class SelfProxyConfiguration
    {
        public string Url { get; set; } = null!;
        public int Latency { get; set; } = 15000;
        public int SuccessCount = 1;
        public int FailureCount = 0;
        public bool OnlyHttps = false;

        public void Validate()
        {
            if (string.IsNullOrEmpty(Url)
                || Latency < 0
                || SuccessCount <= 0
                || FailureCount < 0)
                throw new ArgumentException("Failed to parse SelfProxyConfiguration.");
        }
    }
}
