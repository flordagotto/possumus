using AutoMapper;
using Kata.Wallet.Database.Repositories;
using Kata.Wallet.Domain;
using Kata.Wallet.Dtos;
using Kata.Wallet.Services.Exceptions;
using Microsoft.Extensions.Logging;

namespace Kata.Wallet.Services.Services
{
    public interface ITransactionService
    {
        Task Create(TransactionDto transaction);
        Task<List<TransactionDto>> GetTransactionsFromWallet(int walletId);
    }

    public class TransactionService : ITransactionService
    {
        public readonly ITransactionRepository _transactionRepository;
        public readonly IWalletRepository _walletRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<TransactionService> _logger;

        public TransactionService(ITransactionRepository transactionRepository, IWalletRepository walletRepository, IUnitOfWork unitOfWork, IMapper mapper, ILogger<TransactionService> logger)
        {
            _transactionRepository = transactionRepository;
            _walletRepository = walletRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task Create(TransactionDto transactionDto)
        {
            try
            {
                var originWallet = await _walletRepository.GetById(transactionDto.OriginWalletId);
                var destinationWallet = await _walletRepository.GetById(transactionDto.DestinationWalletId);

                ValidateTransaction(transactionDto, originWallet, destinationWallet);

                var transaction = _mapper.Map<Transaction>(transactionDto);

                transaction.Id = Guid.NewGuid();
                transaction.Date = DateTime.UtcNow;
                transaction.WalletIncoming = destinationWallet!;
                transaction.WalletOutgoing = originWallet!;

                originWallet!.Balance -= transactionDto.Amount;
                destinationWallet!.Balance += transactionDto.Amount;

                await _transactionRepository.Add(transaction);

                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, $"Error creating the transaction, please try again.");
                throw;
            }
        }

        public async Task<List<TransactionDto>> GetTransactionsFromWallet(int walletId)
        {
            try
            {
                var transactions = await _transactionRepository.GetByWalletId(walletId);

                var transactionDtos = MapTransactions(transactions);

                return transactionDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, $"Error retrieving transactions from wallet {walletId}, please try again.");
                throw;
            }
        }

        private static void ValidateTransaction(TransactionDto transactionDto, Domain.Wallet? originWallet, Domain.Wallet? destinationWallet)
        {
            if (originWallet == null)
                throw new WalletDoesNotExistException($"Wallet with id {transactionDto.OriginWalletId} does not exist.");

            if (destinationWallet == null)
                throw new WalletDoesNotExistException($"Wallet with id {transactionDto.DestinationWalletId} does not exist.");

            if (originWallet.Currency != destinationWallet.Currency)
                throw new WalletsCurrenciesDoNotMatchException("Wallets' currencies don't match.");

            if (originWallet == destinationWallet)
                throw new TransactionMustBeBetweenDifferentAccountsException("Origin and destination wallet can not be the same.");

            if (originWallet.Balance < transactionDto.Amount)
                throw new InsufficientBalanceException($"Wallet with id {transactionDto.OriginWalletId} does not have enough balance to make this operation.");
        }

        private List<TransactionDto> MapTransactions(IEnumerable<Domain.Transaction> transactions)
        {
            List<TransactionDto> transactionDtos = new();

            if (transactions != null && transactions.Any())
            {
                foreach (var transaction in transactions)
                {
                    var transactionDto = _mapper.Map<TransactionDto>(transaction);
                    transactionDtos.Add(transactionDto);
                }
            }

            return transactionDtos;
        }
    }
}
