using SleveenAdmin.Foundation.DI;

namespace SleveenAdmin.Foundation.Ids;

public sealed class GuidV7Generator : IIdGenerator, ISingleton
{
    public Guid Create() => Guid.CreateVersion7();
}
