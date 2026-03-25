using System;

namespace De.Hochstaetter.QuickSepa.Models;

public class Iban(string text, bool normalizeAndValidate = false) : Modulo97Base(text, normalizeAndValidate)
{
    protected override int GetModuloValue(char c) => c switch
    {
        >= '0' and <= '9' => unchecked(c - 48),
        >= 'A' and <= 'Z' => unchecked(c - 55),
        _ => throw new FormatException("IBAN contains incorrect characters"),
    };
}