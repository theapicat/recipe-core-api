using API.ExceptionHandlers;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using Npgsql;
using Xunit;

namespace Tests.API.ExceptionHandlers;

public class PostgresExceptionHandlerTests
{
    private static PostgresException Postgres(string sqlState) => new("feil", "ERROR", "ERROR", sqlState);

    private static (PostgresExceptionHandler Handler, IProblemDetailsService Service) Create()
    {
        var service = Substitute.For<IProblemDetailsService>();
        service.TryWriteAsync(Arg.Any<ProblemDetailsContext>()).Returns(new ValueTask<bool>(true));
        return (new PostgresExceptionHandler(service), service);
    }

    [Theory]
    [InlineData(PostgresErrorCodes.ForeignKeyViolation)]
    [InlineData(PostgresErrorCodes.UniqueViolation)]
    public async Task Handles_ForeignKeyAndUniqueViolations_As409(string sqlState)
    {
        var (handler, service) = Create();
        var context = new DefaultHttpContext();

        var handled = await handler.TryHandleAsync(context, Postgres(sqlState), CancellationToken.None);

        Assert.True(handled);
        Assert.Equal(StatusCodes.Status409Conflict, context.Response.StatusCode);
        await service.Received(1).TryWriteAsync(Arg.Any<ProblemDetailsContext>());
    }

    [Fact]
    public async Task IgnoresOtherPostgresErrors()
    {
        var (handler, service) = Create();

        var handled = await handler.TryHandleAsync(new DefaultHttpContext(), Postgres(PostgresErrorCodes.SyntaxError), CancellationToken.None);

        Assert.False(handled);
        await service.DidNotReceive().TryWriteAsync(Arg.Any<ProblemDetailsContext>());
    }

    [Fact]
    public async Task IgnoresNonPostgresExceptions()
    {
        var (handler, _) = Create();

        var handled = await handler.TryHandleAsync(new DefaultHttpContext(), new InvalidOperationException(), CancellationToken.None);

        Assert.False(handled);
    }
}
