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

            RuleFor(x => x.UserDocument)
                .NotEmpty()
                .MinimumLength(7)
                .MaximumLength(9)
                .Matches("^[0-9]+$")
                .WithMessage("Document should be a valid document and have between 7 and 9 digits");

            RuleFor(x => x.UserName)
                .NotEmpty()
                .MaximumLength(50)
                .Matches(@"^[a-zA-Z\s]+$")
                .WithMessage("Name should be a valid name and have a maximum of 50 characters");
        }
    }
}
