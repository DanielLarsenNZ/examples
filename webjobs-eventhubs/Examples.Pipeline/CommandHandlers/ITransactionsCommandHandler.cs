using System.Threading.Tasks;

namespace Examples.Pipeline.Commands
{
    public interface ITransactionsCommandHandler
    {
        Task Handle(CreditAccountCommand command);
        Task Handle(DebitAccountCommand command);
    }
}