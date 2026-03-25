using System;
using System.Linq;

namespace De.Hochstaetter.QuickSepa.Models;

public class CreditorReference : Modulo97Base
{
    /// <summary>
    /// Represents a Creditor reference
    /// </summary>
    /// <param name="text">
    /// <para>The creditor reference as a <see langword="string"/>.</para>
    /// <para>Restrictions (only checked if <paramref name="normalizeAndValidate"/> is <see langword="true"/>)
    /// <list type="bullet">
    /// <item>
    /// <term>Allowed characters</term>
    /// <description>May only contain white-spaces, ASCII letters (upper or lower case) and ASCII numeric digits</description>
    /// </item>
    /// <item>
    /// <term>Minimum requirement</term>
    /// <description>Must contain at least 5 non-whitespace characters.</description>
    /// </item>
    /// <item>
    /// <term>Start</term>
    /// <description>The first characters must be "RF" followed by two numeric digits. The numeric digits are the checksum.</description>
    /// </item>
    /// <item>
    /// <term>Checksum</term>
    /// <description>The checksum must be correct.</description>
    /// </item>
    /// </list></para> </param>
    /// <param name="normalizeAndValidate">If set to <see langword="true"/> the IBAN is validated and a <see cref="FormatException"/> is thrown on any error.
    /// Set to <see langword="false"/> to validate later by calling <see cref="Modulo97Base.NormalizeAndValidate"/> and keep construction footprint low.</param>
    /// <exception cref="ArgumentNullException"><paramref name="text"/> is <see langword="null"/></exception>
    public CreditorReference(string text, bool normalizeAndValidate = false) : base(text, normalizeAndValidate)
    {
        if (normalizeAndValidate && !Text.StartsWith("RF"))
        {
            throw new FormatException("Creditor reference must start with RF");
        }
    }

    /// <inheritdoc/>
    public override bool TryGetNormalized(out string normalizedText)
    {
        normalizedText = string.Empty;
        return Text.StartsWith("RF") && base.TryGetNormalized(out normalizedText);
    }

    public static CreditorReference FromBareReference(string text)
    {
        if (!TryNormalizeText(text, out var normalizedText))
        {
            throw new FormatException("Credit reference contains illegal characters");
        }

        var checksum = 98 - normalizedText.Concat("RF00").Aggregate(0, UpdateRemainder);
        return new CreditorReference($"RF{checksum:D2}{normalizedText}");
    }
}