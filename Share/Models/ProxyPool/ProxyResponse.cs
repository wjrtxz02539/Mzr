using MongoDB.Bson;
using MongoDB.Bson.IO;
using Mzr.Share.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Mzr.Share.Models.ProxyPool
{
    public class ProxyResponse
    {
        [JsonConverter(typeof(ObjectIdJsonConverter))]
        public ObjectId Id { get; set; }
        public string Host { get; set; } = null!;
        public int Port { get; set; } = 0;
        public ProxyTypeEnum Type { get; set; }

        public DateTime AddTime { get; set; }
        public ProxyStatus Http { get; set; } = new();
        public ProxyStatus Https { get; set; } = new();

        public DateTime LastCheckTime { get; set; }

        public int CheckFailCount { get; set; } = 0;

        public int CheckSuccessCount { get; set; } = 0;
        public bool Checking { get; set; } = false;

        public string Source { get; set; } = null!;
        public string? Username { get; set; }
        public string? Password { get; set; }

        [JsonIgnore]
        public string Url
        {
            get
            {
                if (string.IsNullOrEmpty(Host))
                    throw new ArgumentException($"[{Source}] Host is empty.");
                if (Port == 0)
                    throw new ArgumentException($"[{Source}][{Host}] Port not set.");
                return $"{Schema}{Host}:{Port}";
            }
        }

        [JsonIgnore]
        public string Schema
        {
            get
            {
                return Type switch
                {
                    ProxyTypeEnum.Http => "http://",
                    ProxyTypeEnum.Socks5 => "socks5://",
                    ProxyTypeEnum.Socks4 => "socks4://",
                    ProxyTypeEnum.Socks4a => "socks4a://",
                    ProxyTypeEnum.Https => "http://",
                    _ => throw new ArgumentException($"Unsupport Proxy Type {Enum.GetName(Type)}."),
                };
            }
        }
    }

    public class ProxyStatus
    {
        public bool Status { get; set; } = false;
        public long Latency { get; set; } = -1;
    }

    public enum ProxyTypeEnum
    {
        Http,
        Https,
        Socks4,
        Socks4a,
        Socks5
    }
}
