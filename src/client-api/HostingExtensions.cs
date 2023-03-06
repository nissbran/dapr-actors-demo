using System.Net;
using Actors;
using Dapr.Actors;
using Dapr.Actors.Client;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace ClientApi;

internal static class HostingExtensions
{
    public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddDaprClient();
        builder.Services.AddActors(_ => {});
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        return builder.Build();
    }

    public static WebApplication ConfigurePipeline(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseSerilogRequestLogging();
        
        app.MapPost("api/checkout", async (InitializeCheckout request,[FromServices] IActorProxyFactory factory) =>
        {
            var actorId = ActorId.CreateRandom();
            var order = factory.CreateActorProxy<ICheckoutOrder>(actorId, "CheckoutOrder");
            await order.Initialize(request.OrderId);
            return TypedResults.Created($"api/checkout/{actorId}", new
            {
                Id = actorId.ToString()
            });
        });
        
        app.MapPost("api/checkout/{id}/item", async (string id, AddItemRequest request, [FromServices] IActorProxyFactory factory) =>
        {
            var actorId = new ActorId(id);
            var order = factory.CreateActorProxy<ICheckoutOrder>(actorId, "CheckoutOrder");
            await order.AddItem(request.Name);
            return TypedResults.Ok();
        });
        
        app.MapGet("api/checkout/{id}", async (string id, [FromServices] IActorProxyFactory factory) =>
        {
            var actorId = new ActorId(id);
            var order = factory.CreateActorProxy<ICheckoutOrder>(actorId, "CheckoutOrder");
            var result = await order.GetCount();
            return TypedResults.Ok(new CountResponse(result));
        });
        
        return app;
    }
}

public record InitializeCheckout(string OrderId);
public record AddItemRequest(string Name);
public record CountResponse(int Count);