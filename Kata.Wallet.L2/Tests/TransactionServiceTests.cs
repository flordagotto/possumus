using AutoMapper;
using Castle.Core.Logging;
using FluentAssertions;
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
    }
}