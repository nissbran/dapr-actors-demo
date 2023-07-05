using Dapr.Actors.Client;
using Dapr.Actors.Runtime;
using NSubstitute;

namespace Actors.UnitTests;

public class CheckoutOrderTests
{
    [Fact]
    public async Task DemoTestForInitialize()
    {
        // Arrange
        var stateManager = Substitute.For<IActorStateManager>();
        var testHost = ActorHost.CreateForTest<CheckoutOrder>();
        var actor = new CheckoutOrder(
            testHost,
            stateManager,
            Substitute.For<IActorProxyFactory>());

        // Act
        await actor.Initialize("123");

        // Assert
        await stateManager.Received(1).SetStateAsync("state", Arg.Is<CheckoutState>(state => state.OrderId == "123"));
    }
}