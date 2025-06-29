namespace Kata.Wallet.Services.Exceptions
{
    public class WalletAlreadyExistsException : BadRequestException
    {
        public WalletAlreadyExistsException()
        {
        }

        public WalletAlreadyExistsException(string message)
            : base(message)
        {
        }

        public WalletAlreadyExistsException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
