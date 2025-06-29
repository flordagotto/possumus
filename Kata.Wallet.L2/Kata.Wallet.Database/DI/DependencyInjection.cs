using Kata.Wallet.Database.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Kata.Wallet.Database.DI
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
        {
            services.AddScoped<IWalletRepository, WalletRepository>();

            return services;
        }
    }
}