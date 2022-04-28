using Mzr.Share.Configuration;

namespace BlazorWeb.Models.Configurations
{
    public class WebConfiguration
    {
        public DatabaseConfiguration Database { get; set; } = new();
        public List<long> MonitorUserIds { get; set; } = new();

        public void Validate()
        {
            Database.Validate();
        }
    }
}
