using Kata.Wallet.Services.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Kata.Wallet.Services.DI
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<IWalletService, WalletService>();
            services.AddScoped<ITransactionService, TransactionService>();

            return services;
        }
    }
}