using Application.MediatR.Users;
using Contracts.Events.UserActions;
using MassTransit;
using MediatR;

namespace Application.Messaging.Consumers.UserActions;

public class AccountDeletedByUserConsumer(IMediator mediator) : IConsumer<UserAccountDeletedByUserEvent>
{
    public Task Consume(ConsumeContext<UserAccountDeletedByUserEvent> context) =>
        mediator.Send(new DeleteAllUserDataCommand(context.Message.UserId));
}
