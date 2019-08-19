using Examples.Pipeline.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Examples.Pipeline.Data
{
    public class TransactionsRepository : ITransactionsRepository
    {
        public static ConcurrentDictionary<Guid, Transaction> _transactionData = new ConcurrentDictionary<Guid, Transaction>();
        public static ConcurrentDictionary<string, decimal> _accountBalanceData = new ConcurrentDictionary<string, decimal>();
        private readonly ILogger<TransactionsRepository> _log;


        public TransactionsRepository(ILogger<TransactionsRepository> logger)
        {
            _log = logger;
        }

        public async Task AddTransaction(Transaction transaction)
        {
            _transactionData.TryAdd(transaction.Id, transaction);
            await UpdateAccountBalance(transaction);
        }

        async Task UpdateAccountBalance(Transaction transaction)
        {
            _accountBalanceData.TryAdd(transaction.AccountNumber, 0);

            for (int i = 1; i < 4; i++)
            {
                // Try update balance three times
                decimal balance = _accountBalanceData[transaction.AccountNumber];
                decimal newBalance = balance + transaction.Amount;
                if (_accountBalanceData.TryUpdate(transaction.AccountNumber, newBalance, balance)) return;
                _log.LogInformation($"UpdateAccountBalance: Failed attempt {i}: _accountBalanceData.TryUpdate(...{transaction.AccountNumberLast4Digits}, {newBalance}, {balance})");
                await Task.Delay(10);
            }

            var exception = new InvalidOperationException($"Failed to update balance after three attempts. Account number = ...{transaction.AccountNumberLast4Digits}");
            _log.LogError(exception, exception.Message);
            throw exception;
        }
    }
}
