using Application.MediatR.Users;
using Application.Messaging.Consumers.UserActions;
using Contracts.Events.UserActions;
using MassTransit;
using MediatR;
using NSubstitute;
using Xunit;

namespace Tests.Application.Messaging.Consumers.UserActions;

public class AccountDeletedByUserConsumerTests
{
    [Fact]
    public async Task Consume_SendsDeleteAllUserDataCommand_ForTheEventsUserId()
    {
        var userId = Guid.NewGuid();
        var mediator = Substitute.For<IMediator>();
        var context = Substitute.For<ConsumeContext<UserAccountDeletedByUserEvent>>();
        context.Message.Returns(new UserAccountDeletedByUserEvent { UserId = userId, Email = "a@b.no", Name = "Ola", DeletedAt = DateTime.UtcNow });
        var consumer = new AccountDeletedByUserConsumer(mediator);

        await consumer.Consume(context);

        await mediator.Received(1).Send(Arg.Is<DeleteAllUserDataCommand>(c => c.UserId == userId), Arg.Any<CancellationToken>());
    }
}
