﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Examples.Minimal.Functions.Commands
{
    public class DebitAccountCommand : TransactionCommand
    {
        public override string CommandType => nameof(DebitAccountCommand);

        public Decimal DebitAmount { get; set; }
    }
}
