using AutoMapper;
using Kata.Wallet.Database.Repositories;
using Kata.Wallet.Domain;
using Kata.Wallet.Dtos;
using Kata.Wallet.Services.Exceptions;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

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

        public async Task<List<WalletDto>> GetAll(WalletFiltersDto walletFiltersDto)
        {
            try
            {
                var wallets = await _walletRepository.GetAll(walletFiltersDto.UserDocument, walletFiltersDto.Currency);

                var walletDtos = MapWallets(wallets);

                return walletDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, $"Error retrieving wallets, please try again.");
                throw;
            }
        }

        private async Task CheckIfWalletAlreadyExists(WalletDto walletDto)
        {
            var wallet = await _walletRepository.GetById(walletDto.Id);

            if (wallet != null)
                throw new WalletAlreadyExistsException($"The wallet with id {wallet.Id} already exists");
        }

        private List<WalletDto> MapWallets(List<Domain.Wallet> wallets)
        {
            List < WalletDto > walletDtos = new List < WalletDto >();

            foreach (var wallet in wallets)
            {
                var walletDto = _mapper.Map<WalletDto>(wallet);
                walletDtos.Add(walletDto);
            }

            return walletDtos;
        }
    }
}
