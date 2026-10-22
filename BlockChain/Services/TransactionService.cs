using System;
using System.Collections.Generic;
using System.Linq;
using BlockChain.Models;

namespace BlockChain.Services
{
    public class TransactionService
    {
        private readonly WalletService _walletService = new WalletService();
        private readonly BalanceService _balanceService = new BalanceService();

        public Transaction CreateTransaction(Wallet sender, string to, decimal amount, decimal fee)
        {
            var tx = new Transaction(sender.Address, to, amount, fee, sender.PublicKey);
            tx.Signature = sender.SignTransaction(tx);
            var (isValid, message) = ValidateTransaction(tx);
            if (!isValid)
            {
                throw new InvalidOperationException($"Invalid transaction: {message}");
            }

            return tx;
        }

        public Transaction CreateCoinbaseTransaction(string minerAddress, decimal reward)
        {
            var coinbaseTx = new Transaction("COINBASE", minerAddress, reward, 0, null)
            {
                IsCoinbase = true
            };
            return coinbaseTx;
        }


        public Transaction CreateRewardTransaction(string minerAddress, decimal reward)
        {
            var rewardTx = new Transaction("RewardFee", minerAddress, reward, 0, null)
            {
                IsCoinbase = true
            };
            return rewardTx;
        }



        public (bool isValid, string message) ValidateTransaction(Transaction transaction)
        {
            if (transaction == null)
            {
                return (false, "Transaction is null.");
            }
            if (string.IsNullOrWhiteSpace(transaction.From))
            {
                return (false, "Sender address is required.");
            }
            if (string.IsNullOrWhiteSpace(transaction.To))
            {
                return (false, "Recipient address is required.");
            }
            if (transaction.Amount <= 0)
            {
                return (false, "Transaction amount must be greater than zero.");
            }
            if (transaction.Fee <= 0)
            {
                return (false, "Transaction fee must be greater than zero.");
            }
            bool signIsValid = _walletService.VerifyTransactionSignature(transaction, transaction.PublicKey);
            if (!signIsValid)
            {
                return (false, "Invalid transaction signature.");
            }
            return (true, "Transaction is valid.");
        }


        public (bool isValid, string message) ValidateTransactionWithBalance(Transaction transaction, List<Block> chain, List<Transaction> transactionPool)
        {
            if (transactionPool.Contains(transaction))
            {
                transactionPool.Remove(transaction);
            }
            var (isValid, message) = ValidateTransaction(transaction);
            if (!isValid)
            {
                return (false, message);
            }
            bool hasSufficientBalance = _balanceService.HasSufficientBalance(transaction, chain, transactionPool);
            if (!hasSufficientBalance)
            {
                return (false, "Insufficient balance for the transaction.");
            }
            return (true, "Transaction is valid and has sufficient balance.");
        }










        private IEnumerable<TransactionSearchResult> GetAllSearchableTransactions(List<Block> chain)
        {
            if (chain == null) return Enumerable.Empty<TransactionSearchResult>();

            return chain.Where(b => b.Transactions != null)
                        .SelectMany(b => b.Transactions.Select(t => new TransactionSearchResult(b.Index, t)));
        }

        public TransactionSearchResult GetById(List<Block> chain, string id)
        {
            return GetAllSearchableTransactions(chain)
                .FirstOrDefault(res => string.Equals(res.Transaction.Id, id, StringComparison.OrdinalIgnoreCase));
        }

        public List<TransactionSearchResult> GetByUser(List<Block> chain, string username)
        {
            return GetAllSearchableTransactions(chain)
                .Where(res => string.Equals(res.Transaction.From, username, StringComparison.OrdinalIgnoreCase) ||
                              string.Equals(res.Transaction.To, username, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        public List<TransactionSearchResult> GetByMinAmount(List<Block> chain, decimal minAmount)
        {
            return GetAllSearchableTransactions(chain)
                .Where(res => res.Transaction.Amount > minAmount)
                .ToList();
        }

        public TransactionSearchResult GetLargestTransaction(List<Block> chain)
        {
            return GetAllSearchableTransactions(chain)
                .OrderByDescending(res => res.Transaction.Amount)
                .FirstOrDefault();
        }
    }
}












//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using BlockChain.Models;

//namespace BlockChain.Services
//{
//    public class TransactionService
//    {

//        public Transaction CreateTransaction(string from, string to, decimal amount)
//        {
//            var tx = new Transaction(from, to, amount);
//            var (isValid, message) = ValidateTransaction(tx);
//            if (!isValid)
//            {
//                throw new InvalidOperationException($"Invalid transaction: {message}");
//            }

//            return tx;
//        }

//        //Валідація транзакції
//        public (bool isValid, string message) ValidateTransaction(Transaction transaction)
//        {
//            if (transaction == null)
//            {
//                return (false, "Transaction is null.");
//            }
//            if (string.IsNullOrWhiteSpace(transaction.From))
//            {
//                return (false, "Sender address is required.");
//            }
//            if (string.IsNullOrWhiteSpace(transaction.To))
//            {
//                return (false, "Recipient address is required.");
//            }
//            if (transaction.Amount <= 0)
//            {
//                return (false, "Transaction amount must be greater than zero.");
//            }
//            // Additional validation logic can be added here (e.g., checking balances, signatures, etc.)
//            return (true, "Transaction is valid.");
//        }















//public Transaction CreateTransaction(string from, string to, decimal amount)
//{
//    var tx = new Transaction(from, to, amount);
//    var (isValid, message) = ValidateTransaction(tx);
//    if (!isValid)
//    {
//        throw new InvalidOperationException($"Invalid transaction: {message}");
//    }

//    return tx;
//}

//public (bool isValid, string message) ValidateTransaction(Transaction transaction)
//{
//    if (transaction == null)
//    {
//        return (false, "Transaction is null");
//    }
//    if (string.IsNullOrWhiteSpace(transaction.From))
//    {
//        return (false, "Sender address is required.");
//    }
//    if (string.IsNullOrWhiteSpace(transaction.To))
//    {
//        return (false, "Recipient address is required.");
//    }
//    if (transaction.Amount <= 0)
//    {
//        return (false, "Transaction amount must be greater than zero.");
//    }
//    return (true, "Transaction is valid.");
//}
//    }
//}
