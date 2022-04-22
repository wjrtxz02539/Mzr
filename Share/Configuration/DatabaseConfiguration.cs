using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Mzr.Share.Configuration
{
    public class DatabaseConfiguration
    {
        public string Url { get; set; } = null!;
        public string DatabaseName { get; set; } = null!;

        public DatabaseConfiguration()
        { }

        public void Validate()
        {
            if (string.IsNullOrEmpty(Url) || string.IsNullOrEmpty(DatabaseName))
                throw new ArgumentException("Database Configuration missing.");
        }
    }
}
