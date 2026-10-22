using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlockChain.Models
{
    public class TransactionSearchResult
    {
        public int BlockIndex { get; set; }
        public Transaction Transaction { get; set; }

        public TransactionSearchResult(int blockIndex, Transaction transaction)
        {
            BlockIndex = blockIndex;
            Transaction = transaction;
        }
    }
}
