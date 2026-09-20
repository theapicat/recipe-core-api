using Application.Naming;
using Xunit;

namespace Tests.Application.Naming;

public class NameNormalizerTests
{
    [Theory]
    [InlineData("Agurk", "agurk")]
    [InlineData("  Nøtter   OG Frø ", "nøtter og frø")]
    [InlineData("ÆØÅ", "æøå")]
    [InlineData("Kalsium\t(Ca)", "kalsium (ca)")]
    public void Normalize_TrimsCollapsesWhitespaceAndLowercases(string input, string expected) =>
        Assert.Equal(expected, NameNormalizer.Normalize(input));

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Normalize_ReturnsEmpty_ForNullOrBlank(string? input) =>
        Assert.Equal(string.Empty, NameNormalizer.Normalize(input));
}
