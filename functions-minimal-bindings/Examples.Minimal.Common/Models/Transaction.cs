using System;
using System.Collections.Generic;
using System.Text;

namespace Examples.Minimal.Models
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
    }
}
