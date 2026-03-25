using System;

namespace De.Hochstaetter.QuickSepa.Models
{
    public class SepaDestination(string beneficiaryName, Iban iban, string bic = null)
    {
        public string BeneficiaryName { get; } =
            !string.IsNullOrWhiteSpace(beneficiaryName)
                ? beneficiaryName
                : throw new ArgumentNullException(nameof(beneficiaryName), "Beneficiary name cannot be empty");

        public Iban Iban { get; } = iban ?? throw new ArgumentNullException(nameof(iban), "IBAN cannot be null");

        public string Bic { get; } = bic;
    }
}