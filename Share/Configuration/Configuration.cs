using Microsoft.Extensions.Configuration;

namespace Mzr.Share.Configuration
{
    public class Configuration
    {
        public DatabaseConfiguration Database { get; set; }
        public SelfProxyConfiguration SelfProxy { get; set; }
        public KDLProxyConfiguration KDLProxy { get; set; } = new();

        public Configuration(IConfiguration configuration)
        {
            var database = configuration.GetRequiredSection("Database").Get<DatabaseConfiguration>();
            if (database is null)
            {
                throw new NullReferenceException(nameof(database));
            }
            Database = database;

            var selfProxy = configuration.GetRequiredSection("SelfProxy").Get<SelfProxyConfiguration>();
            if (selfProxy is null)
            {
                throw new NullReferenceException(nameof(selfProxy));
            }
            SelfProxy = selfProxy;

            Validate();
        }

        public void Validate()
        {
            if (Database == null
                || SelfProxy == null)
                throw new ArgumentNullException("Bili Database Configuration missing.");

            Database.Validate();
            SelfProxy.Validate();
        }
    }
}
