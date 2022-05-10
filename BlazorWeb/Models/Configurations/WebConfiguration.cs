using Mzr.Share.Configuration;

namespace BlazorWeb.Models.Configurations
{
    public class WebConfiguration
    {
        public DatabaseConfiguration Database { get; set; } = new();
        public List<long> MonitorUserIds { get; set; } = new();
        public int FileExportConcurrency { get; set; } = 5;
        public int FileExportQueueLength { get; set; } = 30;

        public void Validate()
        {
            Database.Validate();
        }
    }
}
