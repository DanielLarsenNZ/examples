using Examples.Pipeline.Models;
using System.Threading.Tasks;

namespace Examples.Pipeline.Data
{
    public interface ITransactionsRepository
    {
        Task AddTransaction(Transaction transaction);
    }
}