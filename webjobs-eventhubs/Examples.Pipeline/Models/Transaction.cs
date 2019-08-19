using System;

namespace Examples.Pipeline.Models
{
    public class Transaction
    {
        public Guid Id { get; set; }

        public string AccountNumber { get; set; }

        public DateTime TransactionDateTime { get; set; }

        public decimal Amount { get; set; }

        public string MerchantId { get; set; }

        public string AuthorizationCode { get; set; }

        public bool IsDebit { get; set; }

        //[JsonIgnore]
        public string AccountNumberLast4Digits
        {
            get
            {
                if (string.IsNullOrEmpty(AccountNumber) || AccountNumber.Length <= 4) return AccountNumber;
                return AccountNumber.Substring(AccountNumber.Length - 4);
            }
        }
    }
}
