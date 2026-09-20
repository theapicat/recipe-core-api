using Microsoft.AspNetCore.Diagnostics;
using Npgsql;

namespace API.ExceptionHandlers;

// Oversetter forventede databasefeil til 409 i stedet for en ubehandlet 500: brudd på fremmednøkkel (raden er i bruk av
// andre data, eller peker på noe som ikke finnes) og på unikhet (raden finnes allerede). Alt annet får standard håndtering.
public class PostgresExceptionHandler(IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not PostgresException postgresException)
            return false;

        var title = postgresException.SqlState switch
        {
            PostgresErrorCodes.ForeignKeyViolation =>
                "Operasjonen bryter en relasjon: raden er i bruk av andre data, eller peker på noe som ikke finnes.",
            PostgresErrorCodes.UniqueViolation => "Raden finnes allerede.",
            _ => null
        };

        if (title is null)
            return false;

        httpContext.Response.StatusCode = StatusCodes.Status409Conflict;
        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = { Status = StatusCodes.Status409Conflict, Title = title }
        });
    }
}
