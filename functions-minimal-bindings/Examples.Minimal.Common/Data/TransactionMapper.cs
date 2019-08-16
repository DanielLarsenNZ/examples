using Examples.Minimal.Commands;
using Examples.Minimal.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Examples.Minimal.Data
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
