using SleveenAdmin.Foundation.DI;

namespace SleveenAdmin.Foundation.Ids;

[Singleton]
public sealed class GuidV7Generator : IIdGenerator
{
    public Guid Create() => Guid.CreateVersion7();
}
