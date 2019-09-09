using Examples.Pipeline.Commands;
using Examples.Pipeline.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Examples.Pipeline.Models
{
    public class MessageInfo
    {
        public MessageInfo()
        {
        }

        public MessageInfo(TransactionCommand command, string messageId, int deliverCount, DateTime enqueuedTimeUtc)
        {
            Command = command;
        }

        public TransactionCommand Command { get; set; }

        public string MessageId { get; set; }

        public int DeliverCount { get; set; }

        public DateTime EnqueuedTimeUtc { get; set; }

        /// <summary>
        /// Computes a Hash of this Message for de-duplication purposes.
        /// </summary>
        public string ComputeHash()
        {
            throw new NotSupportedException();
            // A unique MESSAGE is identified by Command.Id
            //return CryptoHelper.Sha256($"{TransactionId}:{AccountNumber}:{TransactionDateTime.Ticks}:{Amount}:{MerchantId}:{AuthorizationCode}");
        }
    }
}
