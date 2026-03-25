using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace De.Hochstaetter.QuickSepa.Models
{
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

        public string Text
        {
            get;
            private set => SetField(ref field, value);
        }

        public string OriginalText { get; }

        protected abstract int AppendModuloValue(int currentRemainder, char c);

        public override string ToString() => OriginalText;

        public bool TryGetNormalized(out string normalizedText)
        {
            var builder = new StringBuilder(Text.Length);

            foreach (var c in from character in Text where !char.IsWhiteSpace(character) select char.ToUpperInvariant(character))
            {
                if (c is < '0' or > 'Z' or > '9' and < 'A')
                {
                    normalizedText = string.Empty;
                    return false;
                }

                builder.Append(c);
            }

            normalizedText = builder.ToString();
            return normalizedText.Length > 4;
        }

        public string GetNormalized() => TryGetNormalized(out var normalizedText)
            ? normalizedText
            : throw new FormatException("Must contain only letters, numbers and spaces. The minimum length is 5 non-space characters");

        public bool IsValid()
        {
            return TryGetNormalized(out var normalizedText) && HasValidCheckSum(normalizedText);
        }

        public void NormalizeAndValidate()
        {
            Text = GetNormalized();

            if (!HasValidCheckSum(Text))
            {
                throw new FormatException("The checksum is not valid");
            }
        }

        private bool HasValidCheckSum(string normalizedText)
        {
            return normalizedText.Skip(4).Concat(normalizedText.Take(4)).Aggregate(0, AppendModuloValue) == 1;
        }

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