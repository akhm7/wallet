using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using wallet.Controllers;
using Xunit;

namespace wallet
{
    public class WalletTest
    {
        private WalletRepository _walletRepository = new WalletRepository(new Wallet[]{
            new Wallet("1", true),
            new Wallet("2", false)
        });
        private readonly ILogger<WalletController> _logger = new NullLogger<WalletController>();
        private TransactionLogger _transactionLogger = new TransactionLogger();

        private string authUserId = "1";
        private string notAuthUserId = "2";

        [Fact]
        public void CheckDepositLimitTest()
        {
            var controller = new WalletController(_logger, _walletRepository, _transactionLogger);
            var input = new WalletController.DepositInput{value=1000};
            var maxInputForAuth = new WalletController.DepositInput{value=100000};
            var maxInputForNotAuth = new WalletController.DepositInput{value=100000};

            controller.Deposit(authUserId, maxInputForAuth);
            controller.Deposit(notAuthUserId, maxInputForNotAuth);

            Assert.IsType<BadRequestResult>(controller.Deposit(authUserId, input));
            Assert.IsType<BadRequestResult>(controller.Deposit(notAuthUserId, input));
        }
    }
}