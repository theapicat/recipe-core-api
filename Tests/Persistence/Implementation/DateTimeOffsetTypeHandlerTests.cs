using Npgsql;
using Persistence.Implementation;
using Xunit;

namespace Tests.Persistence.Implementation;

public class DateTimeOffsetTypeHandlerTests
{
    private readonly DateTimeOffsetTypeHandler _handler = new();

    [Fact]
    public void Parse_TreatsADateTimeFromNpgsqlAsUtc()
    {
        var parsed = _handler.Parse(new DateTime(2026, 9, 20, 12, 0, 0, DateTimeKind.Unspecified));

        Assert.Equal(TimeSpan.Zero, parsed.Offset);
        Assert.Equal(new DateTimeOffset(2026, 9, 20, 12, 0, 0, TimeSpan.Zero), parsed);
    }

    [Fact]
    public void Parse_PassesADateTimeOffsetThrough()
    {
        var value = new DateTimeOffset(2026, 9, 20, 12, 0, 0, TimeSpan.Zero);

        Assert.Equal(value, _handler.Parse(value));
    }

    [Fact]
    public void Parse_ThrowsForOtherTypes()
    {
        Assert.Throws<InvalidCastException>(() => _handler.Parse("2026-09-20"));
    }

    [Fact]
    public void SetValue_AlwaysSendsUtc_BecauseNpgsqlRejectsOtherOffsetsForTimestamptz()
    {
        var parameter = new NpgsqlParameter();

        _handler.SetValue(parameter, new DateTimeOffset(2026, 9, 20, 14, 0, 0, TimeSpan.FromHours(2)));

        var sent = Assert.IsType<DateTimeOffset>(parameter.Value);
        Assert.Equal(TimeSpan.Zero, sent.Offset);
        Assert.Equal(new DateTimeOffset(2026, 9, 20, 12, 0, 0, TimeSpan.Zero), sent);
    }
}
