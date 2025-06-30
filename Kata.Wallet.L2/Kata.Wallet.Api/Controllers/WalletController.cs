using Kata.Wallet.Domain;
using Kata.Wallet.Dtos;
using Kata.Wallet.Services.Services;
using Microsoft.AspNetCore.Mvc;

namespace Kata.Wallet.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WalletController : ControllerBase
{
    public readonly IWalletService _walletService;

    public WalletController(IWalletService walletService)
    {
        _walletService = walletService;
    }

    [HttpGet]
    public async Task<ActionResult<List<WalletDto>>> GetAll([FromQuery] string? document, Currency? currency)
    {
        return Ok(await _walletService.GetAll(document, currency));
    }

    [HttpPost]
    public async Task<ActionResult> Create([FromBody] WalletDto wallet)
    {
        var createdWallet = await _walletService.Create(wallet);
        return CreatedAtAction(nameof(Create), new { id = createdWallet.Id }, createdWallet);
    }

    [HttpGet("id")]
    public async Task<ActionResult<WalletDto>> GetById([FromQuery] int id)
    {
        return Ok(await _walletService.GetById(id));
    }
}
