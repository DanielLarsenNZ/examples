using Examples.Minimal.Commands;
using Examples.Minimal.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Examples.Minimal.Data
{
    public class TransactionsRepository
    {
        public static ConcurrentDictionary<Guid, Transaction> _transactionData = new ConcurrentDictionary<Guid, Transaction>();
        public static ConcurrentDictionary<string, decimal> _accountBalanceData = new ConcurrentDictionary<string, decimal>();

        public void AddTransaction(Transaction transaction)
        {            
            _transactionData.TryAdd(transaction.Id, transaction);
            UpdateAccountBalance(transaction);
        }

        void UpdateAccountBalance(Transaction transaction)
        {
            _accountBalanceData.TryAdd(transaction.AccountNumber, 0);

            for (int i = 0; i < 3; i++)
            {
                // Try update balance three times
                decimal balance = _accountBalanceData[transaction.AccountNumber];
                decimal newBalance = balance + transaction.Amount;
                if (_accountBalanceData.TryUpdate(transaction.AccountNumber, newBalance, balance)) return;
                //TODO: Log the retry
            }

            throw new InvalidOperationException("Failed to update balance after three attempts");
        }
    }
}
