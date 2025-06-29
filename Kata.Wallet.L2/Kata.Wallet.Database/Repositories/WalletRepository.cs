namespace Kata.Wallet.Database.Repositories
{
    public interface IWalletRepository
    {
        Task Add(Domain.Wallet wallet);
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
    }
}
