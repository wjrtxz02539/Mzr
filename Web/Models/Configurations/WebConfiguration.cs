using Mzr.Share.Configuration;

namespace Mzr.Web.Models.Configurations
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
