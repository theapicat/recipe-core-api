namespace Persistence.Exceptions;

/// <summary>
/// Exception som kastes når en forespurt kommandohåndterer/kommando mangler for en gitt klasse.
/// </summary>
public class DbCommandMissingException<TClass> : PersistenceNotImplementedException<TClass>
{
    public DbCommandMissingException(string commandType)
        : base(commandType)
    {
    }

    public DbCommandMissingException(string commandType, Exception innerException)
        : base(commandType, innerException)
    {
    }
}