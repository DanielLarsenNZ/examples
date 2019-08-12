using System;
using System.Collections.Generic;
using System.Text;

namespace Examples.Minimal.Functions.Commands
{
    public abstract class TransactionCommand : ICommand
    {
        public Guid Id { get; set; }

        public abstract string CommandType { get; }

        public string TransactionId { get; set; }

        public string AccountNumber { get; set; }

        public DateTime TransactionDateTime { get; set; }

        public string MerchantId { get; set; }

        public string AuthorizationCode { get; set; }
    }
}
