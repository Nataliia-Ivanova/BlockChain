using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlockChain.Models
{
    public class BlockchainStats
    {
        public Block FastestBlock { get; set; }
        public Block SlowestBlock { get; set; }
        public Block MostAttemptsBlock { get; set; }
        public double AverageMiningDuration { get; set; }
        public double AverageAttempts { get; set; }
        public int MinDifficulty { get; set; }
        public int MaxDifficulty { get; set; }
    }
}
