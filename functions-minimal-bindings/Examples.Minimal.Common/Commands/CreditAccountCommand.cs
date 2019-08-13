using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace Examples.Minimal.Commands
{
    public class CreditAccountCommand : TransactionCommand
    {
        public Decimal CreditAmount { get; set; }

        public override string CommandType => nameof(CreditAccountCommand);
    }
}
