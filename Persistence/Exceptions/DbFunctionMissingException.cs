namespace Persistence.Exceptions;

/// <summary>
/// Exception som kastes når en database-funksjon (SQL) ikke er lagt til/implementert ennå for en gitt klasse.
/// </summary>
public class DbFunctionMissingException<TClass> : PersistenceNotImplementedException<TClass>
{
    public DbFunctionMissingException(string functionName)
        : base(functionName)
    {
    }

    public DbFunctionMissingException(string functionName, Exception innerException)
        : base(functionName, innerException)
    {
    }
}
