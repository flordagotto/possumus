using FluentValidation;
using Kata.Wallet.Dtos;

namespace Kata.Wallet.Api.Validations
{
    public class TransactionValidation : AbstractValidator<TransactionDto>
    {
        public TransactionValidation()
        {
            RuleFor(x => x.Id)
                .Empty().WithMessage("The transaction id is automatically set");

            RuleFor(x => x.Date)
                .Empty().WithMessage("The transaction date is automatically set");

            RuleFor(x => x.Amount)
                .GreaterThan(0).WithMessage("The transfer amount must be greater than 0.");

            RuleFor(x => x.OriginWalletId)
                .NotEmpty().WithMessage("The origin wallet account must have a value.");

            RuleFor(x => x.DestinationWalletId)
                .NotEmpty().WithMessage("The destination wallet account must have a value.");
        }
    }
}
