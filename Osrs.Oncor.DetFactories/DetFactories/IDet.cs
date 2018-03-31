using System;

namespace Osrs.Oncor.DetFactories
{
    public interface IDet
    {
        Guid Id { get; }
        string Owner { get; }
    }
}
