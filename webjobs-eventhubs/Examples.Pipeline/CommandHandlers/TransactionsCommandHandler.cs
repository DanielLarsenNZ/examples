using Examples.Pipeline.Data;
using System.Threading.Tasks;

namespace Examples.Pipeline.Commands
{
    /// <remarks>Adapted from https://docs.microsoft.com/en-us/azure/architecture/patterns/cqrs#example </remarks>
    public class TransactionsCommandHandler :
        ITransactionsCommandHandler,
        ICommandHandler<DebitAccountCommand>,
        ICommandHandler<CreditAccountCommand>
    {
        private readonly ITransactionsRepository _repository;

        public TransactionsCommandHandler(ITransactionsRepository repository)
        {
            _repository = repository;
        }

        public async Task Handle(DebitAccountCommand command)
        {
            var transaction = TransactionMapper.MapToTransaction(command);
            await _repository.AddTransaction(transaction);
        }

        public async Task Handle(CreditAccountCommand command)
        {
            var transaction = TransactionMapper.MapToTransaction(command);
            await _repository.AddTransaction(transaction);
        }
    }
}
