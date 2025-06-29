namespace Kata.Wallet.Services.Exceptions
{
    public class TransactionMustBeBetweenDifferentAccountsException : BadRequestException
    {
        public TransactionMustBeBetweenDifferentAccountsException()
        {
        }

        public TransactionMustBeBetweenDifferentAccountsException(string message)
            : base(message)
        {
        }

        public TransactionMustBeBetweenDifferentAccountsException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
