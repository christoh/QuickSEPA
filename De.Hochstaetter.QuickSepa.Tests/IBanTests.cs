using De.Hochstaetter.QuickSepa.Models;

namespace De.Hochstaetter.QuickSepa.Tests;

public class IbanTests
{
    [Theory]
    [InlineData("GB82 WEST 1234 5698 7654 32\t")]
    [InlineData("IE64 IRCE 9205 0112 3456 78")]
    [InlineData("BI1320001100010000123456789")]
    [InlineData("de07 1234 1234 1234 1234 12")]
    [InlineData("DE68210501700012345678")]
    public void TestIBan_Success(string ibanString)
    {
        var iban = new Iban(ibanString);
        Assert.True(iban.IsValid());
    }

    [Theory]
    [InlineData("GB18 WEST 1234 5698 7654 32\t")]
    [InlineData("IE53 IRCE 9205 0112 3456 78")]
    [InlineData("BI9620001100010000123456789")]
    [InlineData("de44 1234 1234 1234 1234 12")]
    [InlineData("DE21210501700012345678")]
    public void TestIBan_Failure_Checksum(string ibanString)
    {
        var iban = new Iban(ibanString);
        Assert.False(iban.IsValid());
    }
}