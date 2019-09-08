using Examples.Pipeline.Helpers;
using System;

namespace Examples.Pipeline.Commands
{
    public abstract class TransactionCommand : ICommand
    {
        /// <summary>
        /// Uniquely identifies a Command
        /// </summary>
        public Guid Id { get; set; }

        public abstract string CommandType { get; }

        public string Filename { get; set; }

        /// <summary>
        /// A Transaction identifier supplied by the source system
        /// </summary>
        public string TransactionId { get; set; }

        public string AccountNumber { get; set; }

        public DateTime TransactionDateTime { get; set; }

        public decimal Amount { get; set; }

        /// <summary>
        /// A Merchant identifier supplied by the source system
        /// </summary>
        public string MerchantId { get; set; }

        public string AuthorizationCode { get; set; }

        /// <summary>
        /// Computes a Hash of this Transaction for de-duplication purposes.
        /// </summary>
        public string ComputeHash()
        {
            // A unique TRANSACTION is identified by TransactionId + AccountNumber + TransactionDateTime + Amount + MerchantId + AuthorizationCode
            return CryptoHelper.Sha256($"{TransactionId}:{AccountNumber}:{TransactionDateTime.Ticks}:{Amount}:{MerchantId}:{AuthorizationCode}");
        }
    }
}
