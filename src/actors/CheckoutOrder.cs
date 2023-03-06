using Dapr.Actors;
using Dapr.Actors.Runtime;
using Microsoft.Extensions.Logging;

namespace Actors;

public class CheckoutOrder : Actor, ICheckoutOrder
{
    public CheckoutOrder(ActorHost host) : base(host)
    {
    }

    protected override Task OnActivateAsync()
    {
        Logger.LogInformation("Activate new checkout");
        return Task.CompletedTask;
    }

    protected override Task OnPostActorMethodAsync(ActorMethodContext actorMethodContext)
    {
        //Logger.LogInformation("Post actor Counter: {counter}", _state?.Counter);
        return Task.CompletedTask;
    }

    protected override Task OnDeactivateAsync()
    {
        //Logger.LogInformation("DeActivate checkout, current counter: {counter}", _state?.Counter);
        return Task.CompletedTask;
    }

    public async Task Initialize(string orderId)
    {
        var state = new CheckoutState(Id.GetId());
        Logger.LogInformation("Initialize, {orderId}", orderId);
        state.OrderId = orderId;
        await StateManager.SetStateAsync("state", state);
    }

    public async Task AddItem(string message)
    {
        var state = await StateManager.GetStateAsync<CheckoutState>("state");
        Logger.LogInformation("Item added, {message}", message);
        state.Counter++;
        await StateManager.SetStateAsync("state", state);
    }

    public async Task<int> GetCount()
    {
        var state = await StateManager.GetStateAsync<CheckoutState>("state");
        return state.Counter;
    }
}

public record CheckoutState(string Id)
{
    public string OrderId { get; set; } = string.Empty;
    public int Counter { get; set; }
};


public interface ICheckoutOrder : IActor
{
    Task Initialize(string orderId);
    Task AddItem(string message);
    
    Task<int> GetCount();
}
