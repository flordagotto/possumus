using Kata.Wallet.Domain;
using Microsoft.EntityFrameworkCore;

namespace Kata.Wallet.Database.Repositories
{
    public interface IWalletRepository
    {
        Task Add(Domain.Wallet wallet);
        Task<Domain.Wallet?> GetById(int id);
        Task<List<Domain.Wallet>> GetAll(string? document, Currency? currency);
    }

    public class WalletRepository : IWalletRepository
    {
        private readonly DataContext _dbContext;

        public WalletRepository(DataContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task Add(Domain.Wallet wallet)
        {
            await _dbContext.Wallets.AddAsync(wallet);

            await _dbContext.SaveChangesAsync();
        }

        public async Task<Domain.Wallet?> GetById(int id) => await _dbContext.Wallets.FirstOrDefaultAsync(x => x.Id == id);

        public async Task<List<Domain.Wallet>> GetAll(string? document, Currency? currency)
        {
            IQueryable<Domain.Wallet> query = _dbContext.Wallets
                .Include(t => t.IncomingTransactions)
                .Include(t => t.OutgoingTransactions);

            if (!string.IsNullOrEmpty(document))
                query = query.Where(t => t.UserDocument == document);

            if (currency.HasValue)
                query = query.Where(t => t.Currency == currency.Value);

            return await query.ToListAsync();
        }
    }
}
