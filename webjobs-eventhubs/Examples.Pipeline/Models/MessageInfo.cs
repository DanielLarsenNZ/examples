using Examples.Pipeline.Commands;
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
            
        }
    }
}
