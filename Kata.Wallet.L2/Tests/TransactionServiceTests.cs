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

            _mocker.GetMock<ITransactionRepository>().Verify(x => x.Add(It.IsAny<Transaction>()));

            _mocker.GetMock<IUnitOfWork>()
                .Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}