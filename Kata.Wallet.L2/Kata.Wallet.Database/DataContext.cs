using Kata.Wallet.Database.Repositories;
using Kata.Wallet.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Kata.Wallet.Database;

public class DataContext : DbContext, IUnitOfWork
{
    protected readonly IConfiguration Configuration;

    public DataContext(DbContextOptions<DataContext> options)
        : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseInMemoryDatabase("WalletDb");
        }
    }

    public DbSet<Domain.Wallet> Wallets { get; set; }
    public DbSet<Transaction> Transactions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Transaction>().HasKey(x => x.Id);
        modelBuilder.Entity<Transaction>().Property(x => x.Amount).HasPrecision(16, 2).IsRequired();
        modelBuilder.Entity<Transaction>().HasOne(x => x.WalletIncoming).WithMany(x => x.IncomingTransactions);
        modelBuilder.Entity<Transaction>().HasOne(x => x.WalletOutgoing).WithMany(x => x.OutgoingTransactions);
        modelBuilder.Entity<Domain.Wallet>().HasKey(x => x.Id);
        modelBuilder.Entity<Domain.Wallet>().Property(x => x.Balance).HasPrecision(16, 2).IsRequired();
        modelBuilder.Entity<Domain.Wallet>().Property(x => x.Currency).IsRequired();
        modelBuilder.Entity<Domain.Wallet>().HasMany(x => x.IncomingTransactions).WithOne(x => x.WalletIncoming);
        modelBuilder.Entity<Domain.Wallet>().HasMany(x => x.OutgoingTransactions).WithOne(x => x.WalletOutgoing);
    }

    public static class DbContextSeed
    {
        public static async Task SeedAsync(DataContext context)
        {
            context.Wallets.AddRange(
                new Domain.Wallet
                {
                    Id = 1,
                    Balance = 500,
                    Currency = Currency.ARS,
                    UserName = "Florencia Dagotto",
                    UserDocument = "123456789"
                },
                new Domain.Wallet
                {
                    Id = 2,
                    Balance = 300,
                    Currency = Currency.ARS,
                    UserName = "Juan Medina",
                    UserDocument = "987654321"
                },
                new Domain.Wallet
                {
                    Id = 3,
                    Balance = 300,
                    Currency = Currency.EUR,
                    UserName = "Milagros Bustos",
                    UserDocument = "9876543"
                },
                new Domain.Wallet
                {
                    Id = 4,
                    Balance = 0,
                    Currency = Currency.ARS,
                    UserName = "Alejandro Lopez",
                    UserDocument = "1234567"
                }
            );

            await context.SaveChangesAsync();
        }
    }

}
