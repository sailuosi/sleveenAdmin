namespace SleveenAdmin.System.Author;

public static class AuthorServices
{
    internal static AuthCacheService Cache { get; private set; } = null!;

    public static void Configure(AuthCacheService cache) => Cache = cache;
}
