using Microsoft.AspNetCore.Mvc;
namespace wallet.Controllers;

[ApiController]
[Route("[controller]")]
public class WalletController : ControllerBase
{
    private readonly ILogger<WalletController> _logger;
    private WalletRepository _walletRepository;
    private TransactionLogger _transactionLogger;
    public class DepositInput
    {
        public decimal value { get; set; }
    }

    public WalletController(ILogger<WalletController> logger, WalletRepository walletRepository, TransactionLogger transactionLogger)
    {
        _walletRepository = walletRepository;
        _transactionLogger = transactionLogger;
        _logger = logger;
    }

    [HttpPost("[action]")]
    public object Exists([FromHeader(Name="X-UserId")] string userId)
    {
        return new { Exists = _walletRepository.TryGetWalletById(userId, out var _) };
    }

    [HttpPost("[action]")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public object Deposit([FromHeader(Name="X-UserId")] string userId, [FromBody] DepositInput input)
    {
        // Start of work
        if(!_walletRepository.TryGetWalletById(userId, out var wallet))
        {
            return BadRequest();
        }

        var maxBalance = wallet.IsAuthorized ? 100000 : 10000;
        if (wallet.Value + input.value > maxBalance || !(input.value.CompareTo(decimal.Zero) > 0))
        {
            return BadRequest();
        }

        wallet.Value += input.value;

        _transactionLogger.Append(new TransactionLogEntry
        {
            WalletId = userId,
            PerformTime = DateTimeOffset.Now,
            Value = input.value
        });
        // End of work
        return Ok();
    }

    [HttpPost("[action]")]
    public object DepositHistory([FromHeader(Name="X-UserId")] string userId)
    {
        if(!_walletRepository.TryGetWalletById(userId, out var wallet))
        {
            return BadRequest();
        }
        var info = _transactionLogger.GetDepositsInfo(userId);
        return info;
    }

    [HttpPost("[action]")]
    public object Balance([FromHeader(Name="X-UserId")] string userId)
    {
        if(!_walletRepository.TryGetWalletById(userId, out var wallet))
        {
            return BadRequest();
        }

        return new {Balance = wallet.Value};
    }
}
