using AutoMapper;
using Kata.Wallet.Database.Repositories;
using Kata.Wallet.Dtos;
using Kata.Wallet.Services.Exceptions;
using Microsoft.Extensions.Logging;

namespace Kata.Wallet.Services.Services
{
    public interface IWalletService
    {
        Task Create(WalletDto wallet);
    }

    public class WalletService : IWalletService
    {
        public readonly IWalletRepository _walletRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<WalletService> _logger;

        public WalletService(IWalletRepository walletRepository, IMapper mapper, ILogger<WalletService> logger)
        {
            _walletRepository = walletRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task Create(WalletDto walletDto)
        {
            try
            {
                await CheckIfWalletAlreadyExists(walletDto);

                var wallet = _mapper.Map<Domain.Wallet>(walletDto);

                await _walletRepository.Add(wallet);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, $"Error creating a wallet for user {walletDto.UserName}.");
                throw;
            }
        }

        private async Task CheckIfWalletAlreadyExists(WalletDto walletDto)
        {
            var wallet = await _walletRepository.GetById(walletDto.Id);

            if (wallet != null)
                throw new WalletAlreadyExistsException();
        }
    }
}
