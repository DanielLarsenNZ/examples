using Examples.Pipeline.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Examples.Pipeline.Data
{
    public class TransactionsRepository : ITransactionsRepository
    {
        /// <summary>
        /// Mock Transaction table
        /// </summary>
        public static ConcurrentDictionary<Guid, Transaction> _transactionData = new ConcurrentDictionary<Guid, Transaction>();

        /// <summary>
        /// Mock Account Balances table
        /// </summary>
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
            // Try add the zero balance
            _accountBalanceData.TryAdd(transaction.AccountNumber, 0);

            for (int i = 1; i < 4; i++)
            {
                // Try update balance three times
                decimal balance = _accountBalanceData[transaction.AccountNumber];
                decimal newBalance = balance + transaction.Amount;

                // TryUpdate() compares the new balance with the last retrived balance to enforce concurrency
                // If TryUpdate() succeeds (returns true) this method returns. If fails, retries up to 3 times.
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
