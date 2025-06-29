using Kata.Wallet.Domain;
using Microsoft.EntityFrameworkCore;

namespace Kata.Wallet.Database.Repositories
{
    public interface ITransactionRepository
    {
        Task Add(Domain.Transaction transaction);
        Task<IEnumerable<Transaction>> GetByWalletIdAsync(int walletId);
    }

    public class TransactionRepository : ITransactionRepository
    {
        private readonly DataContext _dbContext;

        public TransactionRepository(DataContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task Add(Domain.Transaction transaction)
        {
            await _dbContext.Transactions.AddAsync(transaction);
        }

        public async Task<IEnumerable<Transaction>> GetByWalletIdAsync(int walletId)
        {
            return await _dbContext.Transactions
                .Where(t => t.WalletOutgoing.Id == walletId || t.WalletIncoming.Id == walletId)
                .ToListAsync();
        }
    }
}
