using AutoMapper;
using Kata.Wallet.Api.AutoMapper;
using Kata.Wallet.Database;
using Kata.Wallet.Database.Repositories;
using Kata.Wallet.Domain;
using Microsoft.EntityFrameworkCore;

namespace IntegrationTests
{
    public class TransactionTests
    {
        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<DataContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var dbContext = new DataContext(options);

            var walletRepository = new WalletRepository(dbContext);
            var transactionRepository = new TransactionRepository(dbContext);

            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<AutoMapperProfile>();
            });

            var mapper = mapperConfig.CreateMapper();
        }

        //[Test]
        //public async Task CreateTransaction_IntegrationTest()
        //{
        //    var options = new DbContextOptionsBuilder<YourDbContext>()
        //        .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
        //        .Options;

        //    var dbContext = new YourDbContext(options);

        //    var walletRepository = new WalletRepository(dbContext);
        //    var transactionRepository = new TransactionRepository(dbContext);
        //    var unitOfWork = new UnitOfWork(dbContext, walletRepository, null);

        //    var mapperConfig = new MapperConfiguration(cfg => cfg.AddProfile<YourMappingProfile>());
        //    var mapper = mapperConfig.CreateMapper();

        //    var service = new TransactionService(transactionRepository, walletRepository, unitOfWork, mapper, null);

        //    var originWallet = new Wallet { Id = 1, Balance = 100, Currency = Currency.ARS, UserName = "Florencia", UserDocument = "123456789" };
        //    var destinationWallet = new Wallet { Id = 2, Balance = 50, Currency = Currency.ARS, UserName = "Juan", UserDocument = "987654321" };

        //    await dbContext.Wallets.AddRangeAsync(originWallet, destinationWallet);
        //    await dbContext.SaveChangesAsync();

        //    var dto = new TransactionDto
        //    {
        //        OriginWalletId = 1,
        //        DestinationWalletId = 2,
        //        Amount = 30,
        //        Description = "Integration test"
        //    };

        //    await service.Create(dto);

        //    var updatedOrigin = await dbContext.Wallets.FindAsync(1);
        //    var updatedDestination = await dbContext.Wallets.FindAsync(2);
        //    var transactions = await dbContext.Transactions.ToListAsync();

        //    Assert.AreEqual(70, updatedOrigin.Balance);
        //    Assert.AreEqual(80, updatedDestination.Balance);
        //    Assert.AreEqual(1, transactions.Count);
        //    Assert.AreEqual(30, transactions[0].Amount);
        //}

    }
}