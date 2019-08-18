using Examples.Pipeline.Data;
using Examples.Pipeline.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Examples.Pipeline.Commands
{
    /// <remarks>Adapted from https://docs.microsoft.com/en-us/azure/architecture/patterns/cqrs#example </remarks>
    public class TransactionsCommandHandler :
        ITransactionsCommandHandler,
        ICommandHandler<DebitAccountCommand>,
        ICommandHandler<CreditAccountCommand> 
    {
        private readonly TransactionsRepository _repository = new TransactionsRepository();

        public TransactionsCommandHandler()
        {
        }

        public async Task Handle(DebitAccountCommand command)
        {
            var transaction = TransactionMapper.MapToTransaction(command);
            _repository.AddTransaction(transaction);
        }

        public async Task Handle(CreditAccountCommand command)
        {
            var transaction = TransactionMapper.MapToTransaction(command);
            _repository.AddTransaction(transaction);
        }
    }
}
