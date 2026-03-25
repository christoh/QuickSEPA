using System;
using System.Linq;

namespace De.Hochstaetter.QuickSepa.Models;

/// <summary>
/// Represents an IBAN
/// </summary>
/// <param name="text">
/// <para>The IBAN as a <see langword="string"/>.</para>
/// <para>Restrictions (only checked if <paramref name="normalizeAndValidate"/> is <see langword="true"/>)
/// <list type="bullet">
/// <item>
/// <term>Allowed characters</term>
/// <description>May only contain white-spaces, ASCII letters (upper or lower case) and ASCII numeric digits</description>
/// </item>
/// <item>
/// <term>Minimum requirement</term>
/// <description>Must contain at least 5 non-whitespace characters. A real IBAN has at least 15 non-whitespace characters but here we support 5 and more.</description>
/// </item>
/// <item>
/// <term>Start</term>
/// <description>The first two non-whitespace characters must be letters followed by two numeric digits. The numeric digits are the checksum.</description>
/// </item>
/// <item>
/// <term>Checksum</term>
/// <description>The checksum must be correct.</description>
/// </item>
/// </list></para> </param>
/// <param name="normalizeAndValidate">If set to <see langword="true"/> the IBAN is validated and a <see cref="FormatException"/> is thrown on any error.
/// Set to <see langword="false"/> to validate later and keep construction footprint low.</param>
/// <exception cref="ArgumentNullException"><paramref name="text"/> is <see langword="null"/></exception>
public class Iban(string text, bool normalizeAndValidate = false) : Modulo97Base(text, normalizeAndValidate)
{
    public static Iban FromCountryCodeAndBban(string countryCode, string bban)
    {
        if (!TryNormalizeText(countryCode, out var normalizedCountryCode) || normalizedCountryCode.Length != 2)
        {
            throw new FormatException("Country code must be exactly 2 ASCII letters");
        }

        if (!TryNormalizeText(bban, out var normalizedBban) || normalizedBban.Length < 1)
        {
            throw new FormatException("BBAN must have at least one ASCII letter or ASCII digit and no illegal characters");
        }

        var checksum = 98 - normalizedBban.Concat(normalizedCountryCode).Concat("00").Aggregate(0, UpdateRemainder);
        return new Iban($"{normalizedCountryCode}{checksum:D2}{normalizedBban}");
    }
}

