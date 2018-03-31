using System;
using System.IO;

namespace Osrs.Oncor.DetFactories
{
    public interface IDetFactory
    {
        IDet Create(Guid fileId, string owner);
        IDet Open(Stream stream);
        IDet Open(string filename);
    }
}
