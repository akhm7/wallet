using Microsoft.AspNetCore.Mvc;
namespace wallet.Controllers;

[ApiController]
[Route("[controller]")]
public class WalletController : ControllerBase
{
    private readonly ILogger<WalletController> _logger;
    private WalletRepository _walletRepository;
    private TransactionLogger _transactionLogger;
    public class ReplenishmentInput
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
        _logger.LogInformation("Checking exists of user id {0}", userId);
        return new { Exists = _walletRepository.TryGetWalletById(userId, out var _) };
    }

    [HttpPost("[action]")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public object Replenishment([FromHeader(Name="X-UserId")] string userId, [FromBody] ReplenishmentInput input)
    {
        _logger.LogInformation("Replenishment of balance for {0} user id {1}", input.value, userId);
        // Start of work
        if(!_walletRepository.TryGetWalletById(userId, out var wallet))
        {
            return BadRequest();
        }

        var maxBalance = wallet.IsAuthorized ? 100000 : 10000;
        if (wallet.Value + input.value > maxBalance || !(input.value.CompareTo(decimal.Zero) > 0))
        {
            _logger.LogDebug(
                "Balance is {0}. Error maximum balance ({1}) or replenishment amount ({2}).",
                wallet.Value,
                maxBalance,
                input.value
            );
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
    public object ReplenishmentHistory([FromHeader(Name="X-UserId")] string userId)
    {
        _logger.LogInformation("Replenishment history of user id {0}.", userId);
        if(!_walletRepository.TryGetWalletById(userId, out var wallet))
        {
            _logger.LogDebug("User id is {0} not found.", userId);
            return BadRequest();
        }
        var info = _transactionLogger.GetReplenishmentsInfo(userId);
        return info;
    }

    [HttpPost("[action]")]
    public object Balance([FromHeader(Name="X-UserId")] string userId)
    {
        _logger.LogInformation("Balance of user id {0}.", userId);
        if(!_walletRepository.TryGetWalletById(userId, out var wallet))
        {
            _logger.LogDebug("User id is {0} not found.", userId);
            return BadRequest();
        }

        return new {Balance = wallet.Value};
    }
}
