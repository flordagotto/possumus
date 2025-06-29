namespace Kata.Wallet.Services.Exceptions
{
    internal class WalletsCurrenciesDoNotMatchException : BadRequestException
    {
        public WalletsCurrenciesDoNotMatchException()
        {
        }

        public WalletsCurrenciesDoNotMatchException(string message)
            : base(message)
        {
        }

        public WalletsCurrenciesDoNotMatchException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
