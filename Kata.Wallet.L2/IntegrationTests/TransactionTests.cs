using FluentAssertions;
using IntegrationTests.Utils;
using Kata.Wallet.Database;
using Kata.Wallet.Domain;
using Kata.Wallet.Dtos;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace IntegrationTests
{
    public class TransactionTests
    {
        private CustomWebApplicationFactory<Program> _factory = null!;
        private HttpClient _client = null!;
        private JsonSerializerOptions _options = null!;

        [SetUp]
        public void Setup()
        {
            _factory = new CustomWebApplicationFactory<Program>();
            _client = _factory.CreateClient();

            _options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringEnumConverter() }
            };
        }

        [Test]
        public async Task CreateTransaction_ShouldUpdateBalances()
        {
            // Arrange
            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DataContext>();
                context.Wallets.AddRange(
                    new Wallet { Id = 10, Balance = 1000, Currency = Currency.ARS, UserName = "Josefina", UserDocument = "11111111" },
                    new Wallet { Id = 20, Balance = 0, Currency = Currency.ARS, UserName = "Francisco", UserDocument = "22222222" }
                );
                await context.SaveChangesAsync();
            }

            var transaction = new TransactionDto
            {
                OriginWalletId = 10,
                DestinationWalletId = 20,
                Amount = 100,
                Description = "Money loan"
            };

            // Act
            var response = await _client.PostAsJsonAsync("api/transaction", transaction);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);

            var rawJson = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<TransactionDto>(rawJson, _options);

            result.Should().NotBeNull();
            result.Amount.Should().Be(100);
            result.Description.Should().Be("Money loan");

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DataContext>();
                var origin = await context.Wallets.FindAsync(10);
                var dest = await context.Wallets.FindAsync(20);

                origin!.Balance.Should().Be(900);
                dest!.Balance.Should().Be(100);
            }
        }

        [Test]
        public async Task CreateTransaction_WhenWalletDoesNotExist_ShouldReturnBadRequest()
        {
            // Arrange
            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DataContext>();
                context.Wallets.Add(
                    new Wallet { Id = 10, Balance = 1000, Currency = Currency.ARS, UserName = "Josefina", UserDocument = "11111111" }
                );
                await context.SaveChangesAsync();
            }

            var transaction = new TransactionDto
            {
                OriginWalletId = 10,
                DestinationWalletId = 20,
                Amount = 100,
                Description = "Money loan"
            };

            // Act
            var response = await _client.PostAsJsonAsync("api/transaction", transaction);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var rawJson = await response.Content.ReadAsStringAsync();

            rawJson.Should().NotBeNull();
            rawJson.Should().Contain("Wallet with id 20 does not exist.");
        }

        [Test]
        public async Task CreateTransaction_WhenCurrenciesDoNotMatch_ShouldReturnBadRequest()
        {
            // Arrange
            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DataContext>();
                context.Wallets.AddRange(
                    new Wallet { Id = 10, Balance = 1000, Currency = Currency.ARS, UserName = "Josefina", UserDocument = "11111111" },
                    new Wallet { Id = 20, Balance = 0, Currency = Currency.EUR, UserName = "Francisco", UserDocument = "22222222" }
                );
                await context.SaveChangesAsync();
            }

            var transaction = new TransactionDto
            {
                OriginWalletId = 10,
                DestinationWalletId = 20,
                Amount = 100,
                Description = "Money loan"
            };

            // Act
            var response = await _client.PostAsJsonAsync("api/transaction", transaction);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var rawJson = await response.Content.ReadAsStringAsync();

            rawJson.Should().NotBeNull();
            rawJson.Should().Contain("Wallets' currencies don't match.");
        }

        [Test]
        public async Task CreateTransaction_WhenWalletsAreTheSame_ShouldReturnBadRequest()
        {
            // Arrange
            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DataContext>();
                context.Wallets.AddRange(
                    new Wallet { Id = 10, Balance = 1000, Currency = Currency.ARS, UserName = "Josefina", UserDocument = "11111111" }
                );
                await context.SaveChangesAsync();
            }

            var transaction = new TransactionDto
            {
                OriginWalletId = 10,
                DestinationWalletId = 10,
                Amount = 100,
                Description = "Money loan"
            };

            // Act
            var response = await _client.PostAsJsonAsync("api/transaction", transaction);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var rawJson = await response.Content.ReadAsStringAsync();

            rawJson.Should().NotBeNull();
            rawJson.Should().Contain("Origin and destination wallet can not be the same.");
        }

        [Test]
        public async Task CreateTransaction_WhenOriginHasInsufficientBalance_ShouldReturnBadRequest()
        {
            // Arrange
            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DataContext>();
                context.Wallets.AddRange(
                    new Wallet { Id = 10, Balance = 50, Currency = Currency.ARS, UserName = "Josefina", UserDocument = "11111111" },
                    new Wallet { Id = 20, Balance = 0, Currency = Currency.ARS, UserName = "Francisco", UserDocument = "22222222" }
                );
                await context.SaveChangesAsync();
            }

            var transaction = new TransactionDto
            {
                OriginWalletId = 10,
                DestinationWalletId = 20,
                Amount = 100,
                Description = "Money loan"
            };

            // Act
            var response = await _client.PostAsJsonAsync("api/transaction", transaction);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var rawJson = await response.Content.ReadAsStringAsync();

            rawJson.Should().NotBeNull();
            rawJson.Should().Contain("Wallet with id 10 does not have enough balance to make this operation.");
        }

        [TearDown]
        public void TearDown()
        {
            _client.Dispose();
            _factory.Dispose();
        }
    }
}