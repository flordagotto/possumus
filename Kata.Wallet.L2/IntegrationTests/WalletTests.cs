using AutoMapper;
using FluentAssertions;
using IntegrationTests.Utils;
using Kata.Wallet.Api.AutoMapper;
using Kata.Wallet.Database;
using Kata.Wallet.Database.Repositories;
using Kata.Wallet.Domain;
using Kata.Wallet.Dtos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace IntegrationTests
{
    public class WalletTests
    {
        private HttpClient _client = null!;
        private CustomWebApplicationFactory<Program> _factory = null!;
        private JsonSerializerOptions _options = null!;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<DataContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var dbContext = new DataContext(options);

            var walletRepository = new WalletRepository(dbContext);

            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<AutoMapperProfile>();
            });

            var mapper = mapperConfig.CreateMapper();

            _factory = new CustomWebApplicationFactory<Program>();
            _client = _factory.CreateClient();
            _options = new JsonSerializerOptions
            {
                Converters = { new JsonStringEnumConverter() },
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        [Test]
        public async Task CreateWallet_ShouldPersistInDatabase()
        {
            // Arrange
            var wallet = new WalletDto
            {
                Id = 5,
                Balance = 10000m,
                UserDocument = "23456789",
                UserName = "Camila Perez",
                Currency = Currency.ARS
            };

            // Act
            var response = await _client.PostAsJsonAsync("api/wallet", wallet);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);

            var rawJson = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<WalletDto>(rawJson, _options);

            result.Should().NotBeNull();
            result.Id.Should().Be(5);
            result.Balance.Should().Be(10000m);
            result.UserDocument.Should().Be("23456789");
            result.UserName.Should().Be("Camila Perez");
            result.Currency.Should().Be(Currency.ARS);
        }

        [Test]
        public async Task CreateWallet_WhenWalletAlreadyExists_ShouldReturnBadRequest()
        {
            // Arrange
            var wallet = new WalletDto
            {
                Id = 1,
                Balance = 10000m,
                UserDocument = "23456789",
                UserName = "Camila Perez",
                Currency = Currency.ARS
            };

            // Act
            var response = await _client.PostAsJsonAsync("api/wallet", wallet);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var rawJson = await response.Content.ReadAsStringAsync();
            
            rawJson.Should().NotBeNull();
            rawJson.Should().Contain("The wallet with id 1 already exists");
        }

        [Test]
        public async Task GetAll_ShouldReturnAllWallets()
        {
            // Act
            var response = await _client.GetAsync("api/wallet");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var rawJson = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<List<WalletDto>>(rawJson, _options);

            result.Should().NotBeNull();
            result.Count.Should().Be(4);
        }

        [TearDown]
        public void TearDown()
        {
            _client.Dispose();
            _factory.Dispose();
        }
    }
}