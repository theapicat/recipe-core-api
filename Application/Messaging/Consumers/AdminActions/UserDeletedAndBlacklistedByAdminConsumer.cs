using Application.MediatR.Users;
using Contracts.Events.AdminActions;
using MassTransit;
using MediatR;

namespace Application.Messaging.Consumers.AdminActions;

public class UserDeletedAndBlacklistedByAdminConsumer(IMediator mediator) : IConsumer<UserDeletedAndBlacklistedByAdminEvent>
{
    public Task Consume(ConsumeContext<UserDeletedAndBlacklistedByAdminEvent> context) =>
        mediator.Send(new DeleteAllUserDataCommand(context.Message.UserId));
}
