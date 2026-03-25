using System;

namespace De.Hochstaetter.QuickSepa.Models;

public class Iban(string text, bool normalizeAndValidate = false) : Modulo97Base(text, normalizeAndValidate)
{
    protected override int AppendModuloValue(int currentRemainder, char c) => c switch
    {
        >= '0' and <= '9' => currentRemainder * 10 + c - '0',
        >= 'A' and <= 'Z' => currentRemainder * 100 + c - 'A' + 10,
        _ => throw new FormatException("IBAN contains incorrect characters"),
    } % 97;
}