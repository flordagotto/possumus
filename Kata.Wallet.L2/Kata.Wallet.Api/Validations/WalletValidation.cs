using FluentValidation;
using Kata.Wallet.Dtos;

namespace Kata.Wallet.Api.Validations
{
    public class WalletValidation : AbstractValidator<WalletDto>
    {
        public WalletValidation()
        {
            RuleFor(x => x.Balance)
                .GreaterThanOrEqualTo(0).WithMessage("The initial balance can not be negative.");

            RuleFor(x => x.Currency)
                .IsInEnum()
                .WithMessage("The currency is invalid.");
        }
    }
}
