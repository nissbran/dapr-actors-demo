﻿using Serilog;

namespace ClientApi;

internal static class HostingExtensions
{
    public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddDaprClient();
        
        builder.Services.AddActors(options =>
        {
        });

        return builder.Build();
    }

    public static WebApplication ConfigurePipeline(this WebApplication app)
    {
        //app.UseMiddleware<RequestResponseLoggingMiddleware>();
        app.UseSerilogRequestLogging();

        app.MapCheckoutApi();
        
        return app;
    }
}