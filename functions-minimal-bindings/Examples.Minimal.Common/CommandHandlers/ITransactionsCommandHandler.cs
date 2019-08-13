using System.Threading.Tasks;

namespace Examples.Minimal.Commands
{
    public interface ITransactionsCommandHandler
    {
        Task Handle(CreditAccountCommand command);
        Task Handle(DebitAccountCommand command);
    }
}