using AutoMapper;
using Kata.Wallet.Database.Repositories;
using Kata.Wallet.Domain;
using Kata.Wallet.Dtos;
using Kata.Wallet.Services.Exceptions;
using Microsoft.Extensions.Logging;
using System.Reflection.Metadata.Ecma335;

namespace Kata.Wallet.Services.Services
{
    public interface ITransactionService
    {
        Task Create(TransactionDto transaction);
    }

    public class TransactionService : ITransactionService
    {
        public readonly ITransactionRepository _transactionRepository;
        public readonly IWalletRepository _walletRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<TransactionService> _logger;

        public TransactionService(ITransactionRepository transactionRepository, IWalletRepository walletRepository, IMapper mapper, ILogger<TransactionService> logger)
        {
            _transactionRepository = transactionRepository;
            _walletRepository = walletRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task Create(TransactionDto transactionDto)
        {
            try
            {
                await ValidateTransaction(transactionDto);

                var transaction = _mapper.Map<Transaction>(transactionDto);

                await _transactionRepository.Add(transaction);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, $"Error creating the transaction, please try again.");
                throw;
            }
        }

        private async Task ValidateTransaction(TransactionDto transactionDto)
        {
            var originWallet = await _walletRepository.GetById(transactionDto.OriginWalletId);
            var destinationWallet = await _walletRepository.GetById(transactionDto.DestinationWalletId);

            if (originWallet == null)
                throw new WalletDoesNotExistException($"Wallet with id {transactionDto.OriginWalletId} does not exist.");

            if (destinationWallet == null)
                throw new WalletDoesNotExistException($"Wallet with id {transactionDto.DestinationWalletId} does not exist.");

            if (originWallet.Currency != destinationWallet.Currency)
                throw new WalletsCurrenciesDoNotMatchException("Wallets' currencies don't match.");

            if (originWallet.Balance < transactionDto.Amount)
                throw new InsufficientBalanceException($"Wallet with id {transactionDto.OriginWalletId} does not have enough balance to make this operation.");
        }
    }
}
