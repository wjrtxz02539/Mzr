using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mzr.Share.Models.Bilibili.Raw;

namespace Mzr.Share.Interfaces.Bilibili.Raw
{
    public interface IRawBiliRepository<T> : IDisposable where T : RawBiliBase
    {
    }
}
