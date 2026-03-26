using System.Runtime.InteropServices.JavaScript;
using System.Text;
using De.Hochstaetter.QuickSepa.Models;

namespace De.Hochstaetter.QuickSepa.Tests;

public class IbanTests(ITestOutputHelper testOutputHelper)
{
    [Theory]
    [InlineData("GB82 WEST 1234 5698 7654 32\t")]
    [InlineData("IE64 IRCE 9205 0112 3456 78")]
    [InlineData("BI1320001100010000123456789")]
    [InlineData("de07 1234 12\r34 1234 1234 12")]
    [InlineData("DE68210501700012345678")]
    public void TestIBan_Success(string ibanString)
    {
        var iban = new Iban(ibanString);
        Assert.True(iban.IsValid());
    }

    [Fact]
    public void Test_Iban_Success_Benchmark()
    {
        const string ibanString = "de07 1234 12\r34 1234 1234 12";
        // Ensure JIT compilation
        var iban = new Iban(ibanString);
        Assert.True(iban.IsValid());

        // Start benchmark
        var startTime = DateTime.UtcNow;
        const int amount = 100_000_000;

        Parallel.For(0, amount, _ =>
        {
            iban = new Iban(ibanString);
            Assert.True(iban.IsValid());
        });

        var duration = (DateTime.UtcNow - startTime).TotalSeconds;
        testOutputHelper.WriteLine($"Tested {amount:N0} IBANs in {duration:N3} seconds.");
        testOutputHelper.WriteLine($"Throughput: {amount / duration / 1e6:N1} Mega IBANs/s");
        testOutputHelper.WriteLine($"Average duration per IBAN: {duration * 1e9 / amount} ns");
    }

    [Fact]
    public void Test_Iban_Generator_Success_Benchmark()
    {
        const int amount = 10_000_000;
        var random = new Random(unchecked((int)DateTime.Now.Ticks));
        var bbanArray = new string[amount];

        Parallel.For(0, amount, i =>
        {
            var stringBuilder = new StringBuilder(18);
            for (var j = 0; j < 18; j++)
            {
                stringBuilder.Append((char)(random.Next(0, 10) + '0'));
            }

            bbanArray[i] = stringBuilder.ToString();
        });

        // Start benchmark
        var startTime = DateTime.UtcNow;

        Parallel.For(0, amount, i =>
        {
            var iban = Iban.FromCountryCodeAndBban("de", bbanArray[i]);
            Assert.Equal(22,iban.Text.Length);
        });

        var duration = (DateTime.UtcNow - startTime).TotalSeconds;
        testOutputHelper.WriteLine($"Generated {amount:N0} IBANs in {duration:N3} seconds.");
        testOutputHelper.WriteLine($"Throughput: {amount / duration / 1e6:N1} Mega IBANs/s");
        testOutputHelper.WriteLine($"Average duration per IBAN: {duration * 1e9 / amount} ns");
    }

    [Theory]
    [InlineData("GB82 WEST 1234 5698 7654 32\t")]
    [InlineData("IE64 IRCE 9205 0112 3456 78")]
    [InlineData("BI13200011000\n10000123456789")]
    [InlineData("de07 1234 1234 1234 1234 12")]
    [InlineData("DE68210501700012345678")]
    public void Test_Bban_Works(string ibanText)
    {
        var countryCode = ibanText[..2];
        var bban = ibanText[4..];
        var iban = Iban.FromCountryCodeAndBban(countryCode, bban);
        var normalizedIbanText = new string(ibanText.Where(c => !char.IsWhiteSpace(c)).Select(char.ToUpperInvariant).ToArray());
        Assert.Equal(normalizedIbanText, iban.Text);
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

    [Theory]
    [InlineData("RF45G72UUR")]
    [InlineData("RF6518K5")]
    public void TestCreditorReference_Success(string text)
    {
        var creditorReference = new CreditorReference(text);
        Assert.True(creditorReference.IsValid());
    }

    [Theory]
    [InlineData("RF35C4")]
    [InlineData("RF214377")]
    public void TestCreditorReference_Failure_Checksum(string text)
    {
        var creditorReference = new CreditorReference(text);
        Assert.False(creditorReference.IsValid());
    }

    [Theory]
    [InlineData("RF65 18K5")]
    [InlineData("RF45g72u UR")]
    public void TestCreditorReference_Success_From_Bare(string bareReference)
    {
        var creditorReference = CreditorReference.FromBareReference(bareReference[4..]);
        Assert.Equal(new string(bareReference.Where(c => !char.IsWhiteSpace(c)).ToArray()).ToUpperInvariant(), creditorReference.Text);
    }

    [Theory]
    [InlineData("GB18 WEST 1234 5698 7654 32\t")]
    [InlineData("IE53 IRCE 9205 0112 3456 78")]
    [InlineData("BI9620001100010000123456789")]
    [InlineData("de44 1234 1234 1234 1234 12")]
    [InlineData("DE21210501700012345678")]
    public void TestCreditorReference_Failure_Does_Not_Start_With_RF(string text)
    {
        var creditorReference = new CreditorReference(text);
        Assert.False(creditorReference.IsValid());
    }
}