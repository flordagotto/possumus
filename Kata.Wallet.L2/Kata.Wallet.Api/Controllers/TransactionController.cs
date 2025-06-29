using Kata.Wallet.Dtos;
using Kata.Wallet.Services.Services;
using Microsoft.AspNetCore.Mvc;

namespace Kata.Wallet.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TransactionController : ControllerBase
{

    public readonly ITransactionService _transactionService;

    public TransactionController(ITransactionService transactionService)
    {
        _transactionService = transactionService;
    }

    [HttpGet]
    public async Task<ActionResult<List<Domain.Transaction>>> GetAll([FromQuery] int walletId)
    {
        return Ok(await _transactionService.GetTransactionsFromWallet(walletId));
    }

    [HttpPost]
    public async Task<ActionResult> Create([FromBody] TransactionDto transaction)
    {
        await _transactionService.Create(transaction);
        return Ok();
    }
}
