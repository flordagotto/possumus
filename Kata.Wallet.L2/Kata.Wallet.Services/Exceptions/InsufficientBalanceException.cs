namespace Kata.Wallet.Services.Exceptions
{
    internal class InsufficientBalanceException : BadRequestException
    {
        public InsufficientBalanceException()
        {
        }

        public InsufficientBalanceException(string message)
            : base(message)
        {
        }

        public InsufficientBalanceException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
