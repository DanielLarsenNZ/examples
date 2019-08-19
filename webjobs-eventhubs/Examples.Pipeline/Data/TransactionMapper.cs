using Examples.Pipeline.Commands;
using Examples.Pipeline.Models;
using System;

namespace Examples.Pipeline.Data
{
    public class TransactionMapper
    {
        public static Transaction MapToTransaction(CreditAccountCommand command)
        {
            var transaction = MapToTransaction((TransactionCommand)command);
            transaction.Amount = command.CreditAmount;
            transaction.IsDebit = false;
            return transaction;
        }

        public static Transaction MapToTransaction(DebitAccountCommand command)
        {
            var transaction = MapToTransaction((TransactionCommand)command);
            transaction.Amount = -command.DebitAmount;
            transaction.IsDebit = true;
            return transaction;
        }

        public static Transaction MapToTransaction(TransactionCommand command)
        {
            return new Transaction
            {
                AccountNumber = command.AccountNumber,
                AuthorizationCode = command.AuthorizationCode,
                Id = Guid.NewGuid(),
                MerchantId = command.MerchantId,
                TransactionDateTime = command.TransactionDateTime
            };
        }
    }
}
