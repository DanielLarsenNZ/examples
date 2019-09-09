using Microsoft.Azure.ServiceBus;
using System;
using System.Collections.Generic;
using System.Text;

namespace Examples.Pipeline.Functions.Extensions
{
    public static class MessageExtensions
    {
        /// <summary>
        /// Computes a Hash of this Transaction for de-duplication purposes.
        /// </summary>
        public static string ComputeHash(this Message message)
        {
            // If this message has been forwarded to another namespace it will have an Original Message Id
            // A globally unique message is identified by its OriginalMessageId
            return (string)message.UserProperties["OriginalMessageId"] ?? message.MessageId;
        }
    }
}
