using System;
using System.Collections.Generic;
using System.Text;

namespace BlockChain.Models
{
    public class Block
    {
        public int Index { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        //public string Data { get; set; }

        public List<Transaction> Transactions { get; set; }

        public string Hash { get; set; }

        public string PrevHash { get; set; }
        public long Nonce { get; set; } = 0;
        public int Difficulty { get; set; }
        public double MiningDuration { get; set; }

        public string MerkleRoot { get; set; } = "";
    }
}
