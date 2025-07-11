using FluentValidation;
using FluentValidation.AspNetCore;
using Kata.Wallet.Database;
using Kata.Wallet.Database.DI;
using Kata.Wallet.Services.DI;
using Kata.Wallet.Services.Exceptions;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using static Kata.Wallet.Database.DataContext;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("IntegrationTests")]

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<DataContext>(options =>
    options.UseInMemoryDatabase("WalletDb"));


builder.Services.AddInfrastructureServices();
builder.Services.AddApplicationServices();

builder.Services.AddControllers(options =>
{
    options.Filters.Add<BadRequestExceptionFilter>();
})
.AddJsonOptions(x =>
{
    // serialize enums as strings in api responses 
    x.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});


builder.Services.AddValidatorsFromAssemblyContaining<Program>();    
builder.Services.AddFluentValidationAutoValidation();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<DataContext>();
    await DbContextSeed.SeedAsync(context);
}

app.Run();
