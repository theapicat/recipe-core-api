using System.Data;
using Dapper;

namespace Persistence.Implementation;

// Npgsql leverer timestamptz som DateTime (UTC), og Dapper konverterer ikke selv til DateTimeOffset.
// Skriving sender alltid UTC, siden Npgsql avviser andre offsets for timestamptz.
public class DateTimeOffsetTypeHandler : SqlMapper.TypeHandler<DateTimeOffset>
{
    public override void SetValue(IDbDataParameter parameter, DateTimeOffset value)
        => parameter.Value = value.ToUniversalTime();

    public override DateTimeOffset Parse(object value) => value switch
    {
        DateTimeOffset dateTimeOffset => dateTimeOffset,
        DateTime dateTime => new DateTimeOffset(DateTime.SpecifyKind(dateTime, DateTimeKind.Utc)),
        _ => throw new InvalidCastException($"Kan ikke konvertere {value.GetType().Name} til DateTimeOffset.")
    };
}
