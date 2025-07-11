using AutoMapper;
using FluentAssertions;
using Kata.Wallet.Api.AutoMapper;
using Kata.Wallet.Database.Repositories;
using Kata.Wallet.Domain;
using Kata.Wallet.Dtos;
using Kata.Wallet.Services.Exceptions;
using Kata.Wallet.Services.Services;
using Moq;
using Moq.AutoMock;

namespace UnitTests
{
    public class TransactionServiceTests
    {
        AutoMocker _mocker;

        ITransactionService _service;

        [SetUp]
        public void Setup()
        {
            _mocker = new AutoMocker();

            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new AutoMapperProfile());
            });
            var realMapper = configuration.CreateMapper();

            _mocker.Use<IMapper>(realMapper);

            _service = _mocker.CreateInstance<TransactionService>();
        }

        [Test]
        public async Task Create_HappyPath()
        {
            var originWallet = new Wallet { Id = 1, Balance = 100, Currency = Currency.ARS, UserName = "Florencia Dagotto", UserDocument = "123456789" };
            var destinationWallet = new Wallet { Id = 2, Balance = 0, Currency = Currency.ARS, UserName = "Juan Medina", UserDocument = "987654321" };

            var transactionDto = new TransactionDto { Amount = 50, Date = DateTime.UtcNow, Description = "Money loan", DestinationWalletId = 2, OriginWalletId = 1 };

            _mocker.GetMock<IWalletRepository>().Setup(x => x.GetById(1)).ReturnsAsync(originWallet);
            _mocker.GetMock<IWalletRepository>().Setup(x => x.GetById(2)).ReturnsAsync(destinationWallet);

            await _service.Create(transactionDto);

            _mocker.GetMock<ITransactionRepository>().Verify(x => x.Add(It.IsAny<Transaction>()), Times.Once);

            _mocker.GetMock<IUnitOfWork>()
                .Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task Create_WhenOriginWalletDoesNotExist_ShouldThrowWalletDoesNotExistException()
        {
            var destinationWallet = new Wallet { Id = 2, Balance = 0, Currency = Currency.ARS, UserName = "Juan Medina", UserDocument = "987654321" };

            var transactionDto = new TransactionDto { Amount = 50, Date = DateTime.UtcNow, Description = "Money loan", DestinationWalletId = 2, OriginWalletId = 1 };

            _mocker.GetMock<IWalletRepository>().Setup(x => x.GetById(2)).ReturnsAsync(destinationWallet);

            var act = async () => await _service.Create(transactionDto);

            await act.Should().ThrowAsync<WalletDoesNotExistException>("Wallet with id 1 does not exist");

            _mocker.GetMock<ITransactionRepository>().Verify(x => x.Add(It.IsAny<Transaction>()), Times.Never);

            _mocker.GetMock<IUnitOfWork>()
                .Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task Create_WhenDestinationWalletDoesNotExist_ShouldThrowWalletDoesNotExistException()
        {
            var originWallet = new Wallet { Id = 1, Balance = 100, Currency = Currency.ARS, UserName = "Florencia Dagotto", UserDocument = "123456789" };

            var transactionDto = new TransactionDto { Amount = 50, Date = DateTime.UtcNow, Description = "Money loan", DestinationWalletId = 2, OriginWalletId = 1 };

            _mocker.GetMock<IWalletRepository>().Setup(x => x.GetById(1)).ReturnsAsync(originWallet);

            var act = async () => await _service.Create(transactionDto);

            await act.Should().ThrowAsync<WalletDoesNotExistException>("Wallet with id 2 does not exist");

            _mocker.GetMock<ITransactionRepository>().Verify(x => x.Add(It.IsAny<Transaction>()), Times.Never);

            _mocker.GetMock<IUnitOfWork>()
                .Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task Create_WhenCurrenciesDontMatch_ShouldThrowWalletsCurrenciesDoNotMatchException()
        {
            var originWallet = new Wallet { Id = 1, Balance = 100, Currency = Currency.ARS, UserName = "Florencia Dagotto", UserDocument = "123456789" };
            var destinationWallet = new Wallet { Id = 2, Balance = 0, Currency = Currency.EUR, UserName = "Juan Medina", UserDocument = "987654321" };

            var transactionDto = new TransactionDto { Amount = 50, Date = DateTime.UtcNow, Description = "Money loan", DestinationWalletId = 2, OriginWalletId = 1 };

            _mocker.GetMock<IWalletRepository>().Setup(x => x.GetById(1)).ReturnsAsync(originWallet);
            _mocker.GetMock<IWalletRepository>().Setup(x => x.GetById(2)).ReturnsAsync(destinationWallet);

            var act = async () => await _service.Create(transactionDto);

            await act.Should().ThrowAsync<WalletsCurrenciesDoNotMatchException>("Wallets' currencies don't match.");

            _mocker.GetMock<ITransactionRepository>().Verify(x => x.Add(It.IsAny<Transaction>()), Times.Never);

            _mocker.GetMock<IUnitOfWork>()
                .Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task Create_WhenWalletsAreTheSame_ShouldThrowTransactionMustBeBetweenDifferentAccountsException()
        {
            var originWallet = new Wallet { Id = 1, Balance = 100, Currency = Currency.ARS, UserName = "Florencia Dagotto", UserDocument = "123456789" };

            var transactionDto = new TransactionDto { Amount = 50, Date = DateTime.UtcNow, Description = "Money loan", DestinationWalletId = 1, OriginWalletId = 1 };

            _mocker.GetMock<IWalletRepository>().Setup(x => x.GetById(1)).ReturnsAsync(originWallet);

            var act = async () => await _service.Create(transactionDto);

            await act.Should().ThrowAsync<TransactionMustBeBetweenDifferentAccountsException>("Origin and destination wallet can not be the same.");

            _mocker.GetMock<ITransactionRepository>().Verify(x => x.Add(It.IsAny<Transaction>()), Times.Never);

            _mocker.GetMock<IUnitOfWork>()
                .Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task Create_WhenOriginHasInsufficientBalance_ShouldThrowInsufficientBalanceException()
        {
            var originWallet = new Wallet { Id = 1, Balance = 30, Currency = Currency.ARS, UserName = "Florencia Dagotto", UserDocument = "123456789" };
            var destinationWallet = new Wallet { Id = 2, Balance = 0, Currency = Currency.ARS, UserName = "Juan Medina", UserDocument = "987654321" };

            var transactionDto = new TransactionDto { Amount = 50, Date = DateTime.UtcNow, Description = "Money loan", DestinationWalletId = 2, OriginWalletId = 1 };

            _mocker.GetMock<IWalletRepository>().Setup(x => x.GetById(1)).ReturnsAsync(originWallet);
            _mocker.GetMock<IWalletRepository>().Setup(x => x.GetById(2)).ReturnsAsync(destinationWallet);

            var act = async () => await _service.Create(transactionDto);

            await act.Should().ThrowAsync<InsufficientBalanceException>("Wallet with id 1 does not have enough balance to make this operation.");

            _mocker.GetMock<ITransactionRepository>().Verify(x => x.Add(It.IsAny<Transaction>()), Times.Never);

            _mocker.GetMock<IUnitOfWork>()
                .Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }


        [Test]
        public async Task GetTransactionsFromWallet_HappyPath()
        {
            var wallet1 = new Wallet { Id = 1, Balance = 100, Currency = Currency.ARS, UserDocument = "123456789", UserName = "Florencia Dagotto" };
            var wallet2 = new Wallet { Id = 2, Balance = 200, Currency = Currency.USD, UserDocument = "12345678", UserName = "Milagros Bustos" };
            var wallet3 = new Wallet { Id = 3, Balance = 200, Currency = Currency.USD, UserDocument = "987654321", UserName = "Juan Medina" };

            var transaction1 = new Transaction { Amount = 50, Date = DateTime.UtcNow, Description = "Money loan", WalletIncoming = wallet2, WalletOutgoing = wallet1 };
            var transaction2 = new Transaction { Amount = 50, Date = DateTime.UtcNow, Description = "Money loan", WalletIncoming = wallet1, WalletOutgoing = wallet3 };
            var transaction3 = new Transaction { Amount = 50, Date = DateTime.UtcNow, Description = "Money loan", WalletIncoming = wallet3, WalletOutgoing = wallet2 };

            var filteredTransactions = new List<Transaction> { transaction1, transaction2 };

            _mocker.GetMock<ITransactionRepository>().Setup(x => x.GetByWalletId(wallet1.Id)).ReturnsAsync(filteredTransactions);

            await _service.GetTransactionsFromWallet(wallet1.Id);

            _mocker.GetMock<ITransactionRepository>().Verify(x => x.GetByWalletId(1), Times.Once);
        }
    }
}