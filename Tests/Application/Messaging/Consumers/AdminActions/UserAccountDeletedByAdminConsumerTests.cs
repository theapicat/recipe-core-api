using Application.MediatR.Users;
using Application.Messaging.Consumers.AdminActions;
using Contracts.Events.AdminActions;
using MassTransit;
using MediatR;
using NSubstitute;
using Xunit;

namespace Tests.Application.Messaging.Consumers.AdminActions;

public class UserAccountDeletedByAdminConsumerTests
{
    [Fact]
    public async Task Consume_SendsDeleteAllUserDataCommand_ForTheEventsUserId()
    {
        var userId = Guid.NewGuid();
        var mediator = Substitute.For<IMediator>();
        var context = Substitute.For<ConsumeContext<UserAccountDeletedByAdminEvent>>();
        context.Message.Returns(new UserAccountDeletedByAdminEvent { UserId = userId, Email = "a@b.no", Name = "Ola", DeletedAt = DateTime.UtcNow });
        var consumer = new UserAccountDeletedByAdminConsumer(mediator);

        await consumer.Consume(context);

        await mediator.Received(1).Send(Arg.Is<DeleteAllUserDataCommand>(c => c.UserId == userId), Arg.Any<CancellationToken>());
    }
}
