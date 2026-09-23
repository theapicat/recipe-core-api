using Application.MediatR.Users;
using Contracts.Events.AdminActions;
using MassTransit;
using MediatR;

namespace Application.Messaging.Consumers.AdminActions;

public class UserAccountDeletedByAdminConsumer(IMediator mediator) : IConsumer<UserAccountDeletedByAdminEvent>
{
    public Task Consume(ConsumeContext<UserAccountDeletedByAdminEvent> context) =>
        mediator.Send(new DeleteAllUserDataCommand(context.Message.UserId));
}
