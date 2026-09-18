namespace Persistence.Exceptions;

/// <summary>
/// Exception som kastes når en forespurt spørring mangler for en gitt klasse.
/// </summary>
public class DbQueryMissingException<TClass> : PersistenceNotImplementedException<TClass>
{
    public DbQueryMissingException(string queryType)
        : base(queryType)
    {
    }

    public DbQueryMissingException(string queryType, Exception innerException)
        : base(queryType, innerException)
    {
    }
}