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


        [TearDown]
        public void TearDown()
        {
            // Cerrar el cliente HTTP
            _client.Dispose();

            // Liberar factory
            _factory.Dispose();
        }

    }
}