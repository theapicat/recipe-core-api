using Application.MediatR.Users;
using Contracts.Events.SystemActions;
using MassTransit;
using MediatR;

namespace Application.Messaging.Consumers.SystemActions;

public class AccountDeletedBySystemConsumer(IMediator mediator) : IConsumer<UserAccountDeletedBySystemEvent>
{
    public Task Consume(ConsumeContext<UserAccountDeletedBySystemEvent> context) =>
        mediator.Send(new DeleteAllUserDataCommand(context.Message.UserId));
}
