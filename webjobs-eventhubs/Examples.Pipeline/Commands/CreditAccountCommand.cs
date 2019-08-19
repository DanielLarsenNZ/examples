namespace Examples.Pipeline.Commands
{
    public class CreditAccountCommand : TransactionCommand
    {
        public decimal CreditAmount { get; set; }

        public override string CommandType => nameof(CreditAccountCommand);
    }
}
