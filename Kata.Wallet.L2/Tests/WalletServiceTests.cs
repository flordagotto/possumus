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
    }
}