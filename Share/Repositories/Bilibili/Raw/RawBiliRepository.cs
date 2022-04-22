using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Mzr.Share.Interfaces.Bilibili.Raw;
using Mzr.Share.Models.Bilibili.Raw;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Mzr.Share.Repositories.Bilibili.Raw
{
    public class RawBiliRepository<T> : IRawBiliRepository<T> where T : RawBiliBase
    {
        public readonly ILogger Logger;
        public readonly IHost Host;

        public RawBiliRepository(ILogger logger, IHost host)
        {
            Host = host;
            Logger = logger;
        }

        public void Dispose()
        {
        }
    }
}
