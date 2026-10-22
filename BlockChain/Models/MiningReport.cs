using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlockChain.Models
{
    public class MiningReport
    {
        public int AcceptedCount { get; set; }
        public int RejectedCount { get; set; }
        public decimal TotalFee { get; set; }
        public decimal MiningReward { get; set; }
        public decimal MinerReceived { get; set; }
        public int RemainingMempoolCount { get; set; }
    }
}
