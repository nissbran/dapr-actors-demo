using Actors;
using Dapr.Actors;
using Dapr.Actors.Client;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace ClientApi;

public static class CheckoutApi
{
    public static void MapCheckoutApi(this WebApplication app)
    {
        var group = app.MapGroup("api/checkout");
        
        group.MapPost("", InitializeCheckout);
        group.MapPost("{id}/item", AddItem);
        group.MapGet("{id}", GetCheckout);
        group.MapGet("{id}/exception", GetCheckoutException);
        
    }

    private static async Task<IResult> GetCheckout(string id, [FromServices] IActorProxyFactory factory)
    {
        var order = factory.GetCheckoutActor(id);
        var count = await order.GetCount();
        return TypedResults.Ok(new CountResponse(count));
    }

    private static async Task<IResult> AddItem(string id, AddItemRequest request, [FromServices] IActorProxyFactory factory)
    {
        var order = factory.GetCheckoutActor(id);
        await order.AddItem(request.Name);
        return TypedResults.Ok();
    }

    private static async Task<IResult> InitializeCheckout(InitializeCheckout request, [FromServices] IActorProxyFactory factory)
    {
        var checkoutId = Guid.NewGuid().ToString();
        var order = factory.GetCheckoutActor(checkoutId);
        await order.Initialize(request.OrderId);
        return TypedResults.Created($"api/checkout/{checkoutId}", new
        {
            Id = checkoutId
        });
    }
    
    private static async Task<IResult> GetCheckoutException(string id, [FromServices] IActorProxyFactory factory)
    {
        var order = factory.GetCheckoutActor(id);
        try
        {
            await order.ExceptionTest();
        }
        catch (ActorMethodInvocationException e)
        {
            Log.Error(e, "ExceptionTest");
            Log.Error(e.InnerException,"InnerException");
            if (e.InnerException is ActorInvokeException { ActualExceptionType: "System.NotImplementedException" } inner)
            {
                Log.Error(inner.ActualExceptionType);
                return TypedResults.BadRequest();
            }
        }
        return TypedResults.Ok();
    }
    
    private static ICheckoutOrder GetCheckoutActor(this IActorProxyFactory factory, string? id = null)
    {
        var actorId = id != null ? new ActorId(id) : ActorId.CreateRandom();
        var order = factory.CreateActorProxy<ICheckoutOrder>(actorId, "CheckoutOrder");
        return order;
    }
}

public record InitializeCheckout(string OrderId);
public record AddItemRequest(string Name);
public record CountResponse(int Count);