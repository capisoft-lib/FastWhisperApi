using Microsoft.Extensions.DependencyInjection;

namespace Capisoft.AI.TranscribeApp;

public static class ServiceHelper
{
    public static IServiceProvider Services { get; private set; } = default!;

    public static void Configure(IServiceProvider serviceProvider)
    {
        Services = serviceProvider;
    }

    public static T GetRequiredService<T>() where T : notnull
    {
        return Services.GetRequiredService<T>();
    }
}
