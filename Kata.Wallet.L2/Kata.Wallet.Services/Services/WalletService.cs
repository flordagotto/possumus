﻿using AutoMapper;
using Kata.Wallet.Database.Repositories;
using Kata.Wallet.Domain;
using Kata.Wallet.Dtos;
using Kata.Wallet.Services.Exceptions;
using Microsoft.Extensions.Logging;

namespace Kata.Wallet.Services.Services
{
    public interface IWalletService
    {
        Task<WalletDto> Create(WalletDto wallet);
        Task<List<WalletDto>> GetAll(string? document, Currency? currency);
        Task<WalletDto> GetById(int id);
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

        public async Task<WalletDto> Create(WalletDto walletDto)
        {
            try
            {
                await CheckIfWalletAlreadyExists(walletDto);

                var wallet = _mapper.Map<Domain.Wallet>(walletDto);

                await _walletRepository.Add(wallet);

                var result = _mapper.Map<WalletDto>(wallet);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, $"Error creating a wallet for user {walletDto.UserName}.");
                throw;
            }
        }

        public async Task<List<WalletDto>> GetAll(string? document, Currency? currency)
        {
            try
            {
                var wallets = await _walletRepository.GetAll(document, currency);

                var walletDtos = MapWallets(wallets);

                return walletDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, "Error retrieving wallets, please try again.");
                throw;
            }
        }

        public async Task<WalletDto> GetById(int id)
        {
            try
            {
                var wallet = await _walletRepository.GetById(id);

                return _mapper.Map<WalletDto>(wallet); ;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, "Error retrieving wallet, please try again.");
                throw;
            }
        }

        private async Task CheckIfWalletAlreadyExists(WalletDto walletDto)
        {
            var wallet = await _walletRepository.GetById(walletDto.Id);

            if (wallet != null)
                throw new WalletAlreadyExistsException($"The wallet with id {wallet.Id} already exists");
        }

        private List<WalletDto> MapWallets(IEnumerable<Domain.Wallet> wallets)
        {
            List<WalletDto> walletDtos = new();

            if (wallets != null && wallets.Any())
            {
                foreach (var wallet in wallets)
                {
                    var walletDto = _mapper.Map<WalletDto>(wallet);
                    walletDtos.Add(walletDto);
                }
            }
            
            return walletDtos;
        }
    }
}
