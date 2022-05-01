using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            Database = configuration.GetRequiredSection("Database").Get<DatabaseConfiguration>();

            SelfProxy = configuration.GetRequiredSection("SelfProxy").Get<SelfProxyConfiguration>();

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
