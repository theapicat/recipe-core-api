using Application.MediatR.Users;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Persistence.Interfaces;
using Xunit;

namespace Tests.Application.MediatR.Users;

public class DeleteAllUserDataCommandHandlerTests
{
    [Fact]
    public async Task Handle_DeletesAllDataForTheGivenUser()
    {
        var userId = Guid.NewGuid();
        var eraser = Substitute.For<IUserDataEraser>();
        eraser.DeleteAllForUserAsync(userId).Returns(new UserDataDeletionResult(3, 1));
        var handler = new DeleteAllUserDataCommandHandler(eraser, Substitute.For<ILogger<DeleteAllUserDataCommandHandler>>());

        await handler.Handle(new DeleteAllUserDataCommand(userId), CancellationToken.None);

        await eraser.Received(1).DeleteAllForUserAsync(userId);
    }
}
