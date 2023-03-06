using Actors;
using Serilog;

namespace Worker;

internal static class HostingExtensions
{
    public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddDaprClient();
        builder.Services.AddActors(options =>
        {
            options.Actors.RegisterActor<CheckoutOrder>();
            
            options.ActorIdleTimeout = TimeSpan.FromMinutes(60);
            options.ActorScanInterval = TimeSpan.FromSeconds(30);
            options.RemindersStoragePartitions = 7;
            options.DrainRebalancedActors = true;
        });
        return builder.Build();
    }

    public static WebApplication ConfigurePipeline(this WebApplication app)
    {
        app.UseSerilogRequestLogging();

        app.MapActorsHandlers();

        return app;
    }
}