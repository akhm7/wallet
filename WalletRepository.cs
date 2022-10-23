using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace wallet
{
    public class TransactionLogEntry
    {
        public string WalletId { get; set; } = string.Empty;
        public DateTimeOffset PerformTime { get; set; }
        public decimal Value { get; set; }
    }

    public class Wallet
    {
        public string Id { get; private set; }
        public bool IsAuthorized { get; private set; }
        public decimal Value { get; set; }

        public Wallet(string id, bool isAuthorized)
        {
            Id = id;
            IsAuthorized = isAuthorized;
            Value = 0;
        }
    }

    public class WalletRepository
    {
        private readonly Dictionary<string, Wallet> _wallets;

        public WalletRepository(IEnumerable<Wallet> wallets)
        {
            _wallets = new();
            foreach(var wallet in wallets)
            {
                _wallets.Add(wallet.Id, wallet);
            }
        }

        public bool TryGetWalletById(string id, out Wallet output)
        {
            return _wallets.TryGetValue(id, out output!);
        }
    }

    public record DepositInfo(decimal Deposit, int DepositsCount);

    public class TransactionLogger
    {
        private readonly Dictionary<string, List<TransactionLogEntry>> _logs = new();

        public void Append(TransactionLogEntry entry)
        {
            if(!_logs.ContainsKey(entry.WalletId))
            {
                _logs.Add(entry.WalletId, new());
            }

            _logs[entry.WalletId].Add(entry);
        }

        public DepositInfo GetDepositsInfo(string walletId)
        {
            var now = DateTimeOffset.UtcNow;
            var month = now.Month;
            var year = now.Year;

            var count = 0;
            decimal deposit = 0;

            if (_logs.ContainsKey(walletId))
            {
                foreach(var transaction in _logs[walletId])
                {
                    if (transaction.PerformTime.Month == month
                        && transaction.PerformTime.Year == year)
                    {
                        count++;
                        deposit += transaction.Value;
                    }
                }
            }
            
            return new(deposit, count);
        }
    }

}