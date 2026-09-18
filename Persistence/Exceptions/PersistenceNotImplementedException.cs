namespace Persistence.Exceptions;

/// <summary>
/// Basisklasse for exceptions som kastes når funksjonalitet i persistence-laget ikke er implementert
/// for en gitt klasse.
/// </summary>
public abstract class PersistenceNotImplementedException<TClass> : Exception
{
    protected PersistenceNotImplementedException(string memberName)
        : base($"{memberName} in {typeof(TClass).Name} was missing!")
    {
    }

    protected PersistenceNotImplementedException(string memberName, Exception innerException)
        : base($"{memberName} in {typeof(TClass).Name} was missing!", innerException)
    {
    }
}
