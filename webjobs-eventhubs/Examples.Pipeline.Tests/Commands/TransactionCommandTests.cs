using Examples.Pipeline.Commands;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Examples.Pipeline.Tests
{
    [TestClass]
    public class TransactionCommandTests
    {
        [TestMethod]
        public void ComputeHash_OnMyMachine_SameResult()
        {
            // Asserts that ComputeHash for a Transaction returns the expected result.
            // This test is to protect against machine / framework variations and changes, as well as code changes

            // arrange
            const string expected = "E33CAA7C7F181C2A421293E27AD6EB96D121DAA9AEFA21F80D3B1FBF7279F4A0";
            var command = new CreditAccountCommand
            {
                Id = Guid.NewGuid(),
                AccountNumber = "1234567890123456",
                Amount = 1234.56m,
                AuthorizationCode = "abcd1234",
                CreditAmount = 1234.56m,
                Filename = $"{Guid.NewGuid()}.csv",
                MerchantId = "a1b2c3d4e5f6",
                TransactionDateTime = new DateTime(2019, 9, 8, 11, 45, 0, DateTimeKind.Utc),
                TransactionId = "1000123456"
            };

            // act
            string actual = command.ComputeHash();

            // assert
            Assert.AreEqual(expected, actual, true);
        }
    }
}
