using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;


namespace Kata.Wallet.Services.Exceptions
{
    public class BadRequestExceptionFilter : IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            if (context.Exception is BadRequestException badRequestEx)
            {
                context.Result = new BadRequestObjectResult(new { error = badRequestEx.Message });
                context.ExceptionHandled = true;
            }
        }
    }
}
