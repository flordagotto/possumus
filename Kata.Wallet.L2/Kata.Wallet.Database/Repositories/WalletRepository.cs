using Microsoft.EntityFrameworkCore;

namespace Kata.Wallet.Database.Repositories
{
    public interface IWalletRepository
    {
        Task Add(Domain.Wallet wallet);
        Task<Domain.Wallet?> GetById(int id);
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
    }
}
