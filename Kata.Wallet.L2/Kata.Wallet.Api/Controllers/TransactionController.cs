using Kata.Wallet.Dtos;
using Kata.Wallet.Services.Services;
using Microsoft.AspNetCore.Mvc;

namespace Kata.Wallet.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TransactionController : ControllerBase
{

    //public readonly IWalletService _walletService;

    public TransactionController(/*IWalletService walletService*/)
    {
        //_walletService = walletService;
    }

    [HttpGet]
    public async Task<ActionResult<List<Domain.Transaction>>> GetAll()
    {
        throw new NotImplementedException();
    }

    [HttpPost]
    public async Task<ActionResult> Create([FromBody] TransactionDto transaction)
    {
        throw new NotImplementedException();
    }
}
