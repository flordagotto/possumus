using Kata.Wallet.Database;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace IntegrationTests.Utils
{
    public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
    {
        private readonly string _databaseName = Guid.NewGuid().ToString();

        public DataContext DbContext { get; private set; } = null!;

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<DataContext>));

                if (descriptor != null)
                    services.Remove(descriptor);

                services.AddDbContext<DataContext>(options =>
                {
                    options.UseInMemoryDatabase(_databaseName);
                });

                var sp = services.BuildServiceProvider();

                using var scope = sp.CreateScope();
                var scopedServices = scope.ServiceProvider;
                var db = scopedServices.GetRequiredService<DataContext>();
                db.Database.EnsureCreated();
                DbContext = db;
            });
        }
    }
}
