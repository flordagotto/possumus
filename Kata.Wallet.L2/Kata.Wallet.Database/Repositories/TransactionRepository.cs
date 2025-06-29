namespace Kata.Wallet.Database.Repositories
{
    public interface ITransactionRepository
    {
        Task Add(Domain.Transaction transaction);
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
    }
}
