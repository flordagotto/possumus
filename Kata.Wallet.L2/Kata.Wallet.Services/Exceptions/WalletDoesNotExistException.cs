namespace Kata.Wallet.Services.Exceptions
{
    public class WalletDoesNotExistException : BadRequestException
    {
        public WalletDoesNotExistException()
        {
        }

        public WalletDoesNotExistException(string message)
            : base(message)
        {
        }

        public WalletDoesNotExistException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
