using BlockChain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using Transaction = BlockChain.Models.Transaction;

namespace BlockChain.Services
{
    public class BalanceService
    {
        public decimal GetBalance(string address, List<Block> chain)
        {
            decimal balance = 0;
            foreach (var block in chain)
            {
                foreach (var tx in block.Transactions)
                {
                    if (tx.From == address)
                    {
                        balance -= tx.Amount + tx.Fee;
                    }
                    else if (tx.To == address)
                    {
                        balance += tx.Amount;
                    }                    
                }
            }
            return balance;
        }


        public bool HasSufficientBalance(Transaction transaction, List<Block> chain, List<Transaction> transactionPool)
        {
            decimal balance = GetBalance(transaction.From, chain);
            foreach (var tx in transactionPool)
            {
                if (tx.From == transaction.From)
                {
                    balance -= tx.Amount + tx.Fee;
                }
                if (tx.To == transaction.To)
                {
                    balance += tx.Amount;
                }
            }
            return balance >= transaction.Amount;
        }


        public decimal GetAvailableBalance(string address, List<Block> chain, List<Transaction> transactionPool)
        {
            decimal balance = GetBalance(address, chain);
            foreach (var tx in transactionPool)
            {
                if (tx.From == address)
                {
                    balance -= tx.Amount;
                }
                else if (tx.To == address)
                {
                    balance += tx.Amount;
                }
            }
            return balance;
        }




    }
}
