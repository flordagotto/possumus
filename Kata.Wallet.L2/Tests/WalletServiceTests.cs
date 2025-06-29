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
    public class WalletServiceTests
    {
        AutoMocker _mocker;

        IWalletService _service;

        [SetUp]
        public void Setup()
        {
            _mocker = new AutoMocker();

            _service = _mocker.CreateInstance<WalletService>();
        }

        [Test]
        public async Task Create_HappyPath()
        {
            var walletDto = new WalletDto { Id = 1, Balance = 100, Currency = Currency.ARS, UserDocument = "123456789", UserName = "Florencia Dagotto" };

            await _service.Create(walletDto);

            _mocker.GetMock<IWalletRepository>().Verify(x => x.Add(It.IsAny<Wallet>()));
        }

        [Test]
        public async Task Create_WhenWalletAlreadyExists_ShouldThrowWalletAlreadyExistsException()
        {
            var walletDto = new WalletDto { Id = 1, Balance = 100, Currency = Currency.ARS, UserDocument = "123456789", UserName = "Florencia Dagotto" };
            var wallet = new Wallet { Id = 1, Balance = 50, Currency = Currency.ARS, UserDocument = "987654321", UserName = "Juan Medina" };

            _mocker.GetMock<IWalletRepository>()
                .Setup(x => x.GetById(1))
                .ReturnsAsync(wallet);


            var act = async () => await _service.Create(walletDto);

            await act.Should().ThrowAsync<WalletAlreadyExistsException>("The wallet with id 1 already exists");

            _mocker.GetMock<IWalletRepository>().Verify(x => x.Add(It.IsAny<Wallet>()), Times.Never);
        }

        [Test]
        public async Task GetAll_HappyPath()
        {
            var userDocument = "123456789";

            var wallet1 = new Wallet { Id = 1, Balance = 100, Currency = Currency.ARS, UserDocument = "123456789", UserName = "Florencia Dagotto" };
            var wallet2 = new Wallet { Id = 2, Balance = 200, Currency = Currency.USD, UserDocument = "123456789", UserName = "Florencia Dagotto" };
            var wallet3 = new Wallet { Id = 3, Balance = 200, Currency = Currency.USD, UserDocument = "987654321", UserName = "Juan Medina" };

            var filteredWallets = new List<Wallet> { wallet1, wallet2 };

            _mocker.GetMock<IWalletRepository>().Setup(x => x.GetAll(It.IsAny<string>(), It.IsAny<Currency>())).ReturnsAsync(filteredWallets);

            await _service.GetAll(userDocument, null);

            _mocker.GetMock<IWalletRepository>().Verify(x => x.GetAll(userDocument, null), Times.Once);
        }
    }
}