using System.Diagnostics.CodeAnalysis;
using De.Hochstaetter.QuickSepa.Models;
using QRCoder;

namespace De.Hochstaetter.QuickSepa.Tests;

[SuppressMessage("ReSharper", "StringLiteralTypo")]
public class UnitTest1
{
    [Fact]
    public void Test1()
    {
        var account = new SepaDestination("Hans Wurst", new Iban("IE64 IRCE 9205 0112 3456 78"), "REVOLT21");
        const decimal amount = 1000.00m;
        const string message = "Money for nothing";
        const string creditorReference = "RF45G72UUR";

        var text =
            $"""
            BCD
            002
            1
            SCT
            {account.Bic}
            {account.BeneficiaryName}
            {account.Iban}
            EUR{amount:F2}
            
            {creditorReference}
            {message}
            """.Replace("\r","");

        var x = QRCodeGenerator.GenerateQrCode(text, QRCodeGenerator.ECCLevel.M, true, true);
        var code = new AsciiQRCode(x);
        var output = code.GetGraphic(1);
    }
}