using Dapr.Actors;
using Dapr.Actors.Client;
using Dapr.Actors.Runtime;
using Microsoft.Extensions.Logging;

namespace Actors;

public class CheckoutOrder : Actor, ICheckoutOrder
{
    private readonly IActorProxyFactory _proxyFactory;

    public CheckoutOrder(ActorHost host, IActorStateManager stateManager, IActorProxyFactory proxyFactory) : base(host)
    {
        _proxyFactory = proxyFactory;
        StateManager = stateManager;
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

    public Task<string> CheckoutId => Task.FromResult(Id.GetId());

    public async Task Initialize(string orderId)
    {
        Logger.LogInformation("Initialize with actor id: {id} and order id: {orderId}", Id, orderId);
        var state = new CheckoutState
        {
            Id = Id.ToString(),
            OrderId = orderId
        };
        await StateManager.SetStateAsync("state", state);
    }

    public async Task AddItem(string message)
    {
        var state = await GetState();
        Logger.LogInformation("Item added, {message}", message);
        state.Counter++;
        await StateManager.SetStateAsync("state", state);
    }

    public Task ExceptionTest()
    {
        throw new NotImplementedException();
    }

    public async Task CompleteAndSendOrder()
    {
        var state = await GetState();
        if (state.OrderCompleted)
        {
            Logger.LogInformation("Order already completed");
        }
        else
        {
            state.OrderCompleted = true;
            state.CounterCompleted++;
            
            await StateManager.SetStateAsync("state", state);
            await StateManager.SaveStateAsync();
            Logger.LogInformation("Order completed");
        }
        
        var proxy = _proxyFactory.CreateActorProxy<ICheckoutOrder>(Id, "CheckoutOrder");
        await proxy.SendOrder();
    }

    public async Task SendOrder()
    {
        var state = await GetState();
        if (!state.OrderSent)
        {
            await Task.Delay(1000);
            if (Random.Shared.Next(0, 10) > 5)
            {
                state.OrderSent = true;
                state.CounterSent++;
                await StateManager.SetStateAsync("state", state);
                await StateManager.SaveStateAsync();
                Logger.LogInformation("Order sent");
            }
            else
            {
                Logger.LogInformation("Order not sent");
                throw new Exception("Order not sent");
            }
        }
        else
        {
            Logger.LogInformation("Order already sent");
        }
    }

    public async Task<int> GetCount()
    {
        var state = await GetState();
        Logger.LogInformation("State is {state}", state);
        return state.Counter;
    }
    
    private async Task<CheckoutState> GetState()
    {
        try
        {
            var state = await StateManager.GetStateAsync<CheckoutState>("state");

            Logger.LogInformation("State is {@state}", state);
            return state;
        }
        catch (Exception e)
        {
            Logger.LogError(e, "Error getting state");
            throw;
        }
    }
}

public class CheckoutState
{
    public string OrderId { get; set; } = string.Empty;
    public int Counter { get; set; }
    public string Id { get; set; }
    
    public bool OrderCompleted { get; set; }
    public int CounterCompleted { get; set; }
    public bool OrderSent { get; set; }
    public int CounterSent { get; set; }
}

public interface ICheckoutOrder : IActor
{
    Task Initialize(string orderId);
    Task AddItem(string message);
    Task ExceptionTest();
    
    Task CompleteAndSendOrder();
    
    Task SendOrder();
    
    Task<int> GetCount();
}
