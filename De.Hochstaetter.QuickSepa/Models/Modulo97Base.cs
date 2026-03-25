using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace De.Hochstaetter.QuickSepa.Models
{
    /// <summary>
    /// Common base class for IBAN and creditor reference
    /// </summary>
    public abstract class Modulo97Base : INotifyPropertyChanged
    {
        protected Modulo97Base(string text, bool normalizeAndValidate = false)
        {
            OriginalText = Text = text ?? throw new ArgumentNullException(nameof(text), "Parameter cannot be null"); ;

            if (normalizeAndValidate)
            {
                NormalizeAndValidate();
            }
        }

        /// <summary>
        /// The IBAN or creditor reference as a <see langword="string"/>. If you normalize at construction time or call <see cref="NormalizeAndValidate"/>, this property contains the "normalized" form, i.e. whitespaces removed and letters converted to uppercase.
        /// The property supports <see cref="INotifyPropertyChanged"/> and can be used in MVVM. As long as you do not normalize explicitly, this property is the same as <see cref="OriginalText"/> so the getter never throws an <see cref="Exception"/>.
        /// </summary>
        public string Text
        {
            get;
            protected set => SetField(ref field, value);
        }

        /// <summary>
        /// The IBAN or creditor reference as a <see langword="string"/>. This property is always exactly the same as used in the constructor (as entered by the user).
        /// </summary>
        public string OriginalText { get; }

        /// <summary>
        /// Overridden for easier debugging
        /// </summary>
        /// <returns>The <see cref="Text"/> property</returns>
        public override string ToString() => Text;

        /// <summary>
        /// Gets the "normalized" form (whitespaces removed and letters converted to uppercase). Does not modify <see cref="Text"/>.
        /// </summary>
        /// <remarks>The checksum is not validated by this method. Use <see cref="IsValid"/> if you need this.</remarks>
        /// <param name="normalizedText">The normalized form if the method returns <see langword="true"/> and undefined otherwise</param>
        /// <returns><see langword="true"/> if successful or <see langword="false"/> if <see cref="OriginalText"/> contains any errors.</returns>
        public virtual bool TryGetNormalized(out string normalizedText)
        {
            if (Text != OriginalText)
            {
                normalizedText = Text;
                return true;
            }

            if (!TryNormalizeText(OriginalText, out normalizedText)) return false;
            return normalizedText.Length > 4 && char.IsLetter(normalizedText[0]) && char.IsLetter(normalizedText[1]) && char.IsDigit(normalizedText[2]) && char.IsDigit(normalizedText[3]);
        }

        protected static bool TryNormalizeText(string text, out string normalizedText)
        {
            var builder = new StringBuilder(text.Length);

            foreach (var c in from character in text where !char.IsWhiteSpace(character) select char.ToUpperInvariant(character))
            {
                if (c is < '0' or > 'Z' or > '9' and < 'A')
                {
                    normalizedText = string.Empty;
                    return false;
                }

                builder.Append(c);
            }

            normalizedText = builder.ToString();
            return true;
        }


        /// <summary>
        /// Gets the "normalized" form (whitespaces removed and letters converted to uppercase). Does not modify <see cref="Text"/>.
        /// </summary>
        /// <remarks>The checksum is not validated by this method. Use <see cref="IsValid"/> if you need this.</remarks>
        /// <returns>The normalized form</returns>
        /// <exception cref="FormatException">The <see cref="OriginalText"/> contains errors</exception>
        public string GetNormalized() => TryGetNormalized(out var normalizedText)
            ? normalizedText
            : throw new FormatException("Must contain only letters, numbers and spaces. The minimum length is 5 non-space characters");


        public bool IsValid()
        {
            return TryGetNormalized(out var normalizedText) && HasValidCheckSum(normalizedText);
        }

        /// <summary>
        /// Normalizes (removes whitespaces, and converts all letters to uppercase) <see cref="Text"/> and validates the checksum. <see cref="Text"/> supports <see cref="INotifyPropertyChanged"/>, e.g. for MVVM.
        /// See also <seealso cref="OriginalText"/> to always get the text as used in the constructor (as entered by the user). <see cref="OriginalText"/> is immutable.
        /// </summary>
        /// <exception cref="FormatException"></exception>
        public void NormalizeAndValidate()
        {
            Text = GetNormalized();

            if (!HasValidCheckSum(Text))
            {
                throw new FormatException("The checksum is not valid");
            }
        }

        private static bool HasValidCheckSum(string normalizedText)
        {
            return normalizedText.Skip(4).Concat(normalizedText.Take(4)).Aggregate(0, UpdateRemainder) == 1;
        }

        protected static int UpdateRemainder(int currentRemainder, char c) => c switch
        {
            >= '0' and <= '9' => currentRemainder * 10 + c - '0',
            >= 'A' and <= 'Z' => currentRemainder * 100 + c - 'A' + 10,
            _ => throw new FormatException("incorrect characters detected"),
        } % 97;

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}