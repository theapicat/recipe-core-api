using Application.MediatR.Users;
using Application.Messaging.Consumers.SystemActions;
using Contracts.Events.SystemActions;
using MassTransit;
using MediatR;
using NSubstitute;
using Xunit;

namespace Tests.Application.Messaging.Consumers.SystemActions;

public class AccountDeletedBySystemConsumerTests
{
    [Fact]
    public async Task Consume_SendsDeleteAllUserDataCommand_ForTheEventsUserId()
    {
        var userId = Guid.NewGuid();
        var mediator = Substitute.For<IMediator>();
        var context = Substitute.For<ConsumeContext<UserAccountDeletedBySystemEvent>>();
        context.Message.Returns(new UserAccountDeletedBySystemEvent
        {
            UserId = userId, Email = "a@b.no", Name = "Ola", DeletionReason = "Inaktiv", DeletedAt = DateTime.UtcNow
        });
        var consumer = new AccountDeletedBySystemConsumer(mediator);

        await consumer.Consume(context);

        await mediator.Received(1).Send(Arg.Is<DeleteAllUserDataCommand>(c => c.UserId == userId), Arg.Any<CancellationToken>());
    }
}
