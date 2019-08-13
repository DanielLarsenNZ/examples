using Examples.Minimal.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Examples.Minimal.Commands
{
    /// <remarks>Adapted from https://docs.microsoft.com/en-us/azure/architecture/patterns/cqrs#example </remarks>
    public class TransactionsCommandHandler :
        ITransactionsCommandHandler,
        ICommandHandler<DebitAccountCommand>,
        ICommandHandler<CreditAccountCommand> 
    {
        //private readonly IRepository<Transaction> repository;

        public TransactionsCommandHandler()
        {
            //this.repository = repository;
        }

        public async Task Handle(DebitAccountCommand command)
        {
            //...
        }

        public async Task Handle(CreditAccountCommand command)
        {
            //var product = repository.Find(command.ProductId);
            //if (product != null)
            //{
            //    product.RateProduct(command.UserId, command.Rating);
            //    repository.Save(product);
            //}
        }
    }
}
