namespace Examples.Pipeline.Commands
{
    public class DebitAccountCommand : TransactionCommand
    {
        public override string CommandType => nameof(DebitAccountCommand);

        public decimal DebitAmount { get; set; }
    }
}
