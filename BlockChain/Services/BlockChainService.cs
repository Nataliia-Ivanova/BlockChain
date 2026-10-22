using BlockChain.Models;

namespace BlockChain.Services
{
    internal class BlockChainService
    {
        public List<Block> Chain { get; set; }
        public List<Transaction> MemPool { get; set; }
        public int Difficulty { get; set; } = 2;

        private readonly double _targetBlockTime = 3000; // 3 seconds

        private readonly int _adjustmentInterval = 10; // Adjust difficulty every 10 blocks

        private int _maxTransactionPerBlock = 5; // Maximum number of transactions per block

        private decimal _minerReward = 50;

        private int _halvingInterval = 3;
        public MiningReport LastMiningReport { get; private set; }


        public readonly MiningService miningService;

        private readonly HashingService _hashingService;

        private readonly TransactionService _transactionService;



        public BlockChainService(int difficulty, double targetBlockTime, int adjustmentInterval)
        {
            this.Difficulty = difficulty;
            this._targetBlockTime = targetBlockTime;
            this._adjustmentInterval = adjustmentInterval;
            _hashingService = new HashingService();
            Chain = new List<Block>();
            miningService = new MiningService();
            _transactionService = new TransactionService();

            MemPool = new List<Transaction>();

            AddGenesisBlock();
        }

        private void AddGenesisBlock()
        {
            var merkleRoot = _hashingService.ComputeMerkleRoot(new List<Transaction>());
            var genesis = new Block { Transactions = new List<Transaction>(), Index = 0, Timestamp = new DateTime(1900, 1, 1), PrevHash = "0", MerkleRoot = merkleRoot };
            miningService.MineBlock(genesis, Difficulty);
            Chain.Add(genesis);
        }

       
        public Block MinePendingTransaction(string minerAddress)
        {

            var transactions = SelectTransactionsForBlock(_maxTransactionPerBlock, out int rejectedCount);
            var totalFee = transactions.Sum(t => t.Fee);
            decimal miningReward = GetCurrentMinerReward();
            decimal minerReceived = miningReward + totalFee;

            Block lastBlock = Chain[^1];

            var coinbase = _transactionService.CreateCoinbaseTransaction(minerAddress, miningReward);
            var rewardTransaction = _transactionService.CreateRewardTransaction(minerAddress, totalFee);


            var newBlock = new Block()
            {
                Index = lastBlock.Index + 1,
                Transactions = transactions,
                PrevHash = lastBlock.Hash,
                Difficulty = Difficulty,
            };


            newBlock.Transactions.Insert(0, coinbase);
            newBlock.Transactions.Insert(1, rewardTransaction);
            var merkleRoot = _hashingService.ComputeMerkleRoot(newBlock.Transactions);
            newBlock.MerkleRoot = merkleRoot;

            miningService.MineBlock(newBlock, Difficulty);
            Chain.Add(newBlock);



            if (newBlock.Index % _adjustmentInterval == 0)
            {
                AdjustDifficulty();
            }

            LastMiningReport = new MiningReport
            {
                AcceptedCount = transactions.Count,
                RejectedCount = rejectedCount,
                TotalFee = totalFee,
                MiningReward = miningReward,
                MinerReceived = minerReceived,
                RemainingMempoolCount = MemPool.Count
            };

            return newBlock;
        }

        //public List<Transaction> SelectTransactionsForBlock(int maxCount)
        //{
        //    var selectedTransactions = MemPool.Where(t => t.Fee >= 1).OrderByDescending(t => t.Fee).ThenBy(t => t.Timestamp).Take(maxCount).ToList();
        //    //Console.WriteLine($"Selected {selectedTransactions.Count} transactions for the next block:");
        //    //foreach (var tx in selectedTransactions)
        //    //{
        //    //    Console.WriteLine($"From: {tx.From}, To: {tx.To}, Amount: {tx.Amount}, Fee: {tx.Fee}");
        //    //}

        //    foreach (var tx in selectedTransactions)
        //    {
        //        MemPool.Remove(tx);
        //    }
        //    return selectedTransactions;
        //}



        public List<Transaction> SelectTransactionsForBlock(int maxCount, out int rejectedCount)
        {
            rejectedCount = 0;

            int expiredCount = MemPool.RemoveAll(t => (DateTime.UtcNow - t.Timestamp).TotalMinutes > 5);
            rejectedCount += expiredCount;

            var candidates = MemPool
                .OrderByDescending(t => t.Fee)
                .ThenBy(t => t.Timestamp)
                .ToList();

            var validTransactions = new List<Transaction>();

            foreach (var tx in candidates)
            {
                if (validTransactions.Count >= maxCount)
                {
                    break;
                }

                if (IsTransactionInChain(tx.Id))
                {
                    MemPool.Remove(tx);
                    rejectedCount++;
                    continue;
                }

                var (isTxValid, _) = _transactionService.ValidateTransaction(tx);
                if (!isTxValid)
                {
                    MemPool.Remove(tx);
                    rejectedCount++;
                    continue;
                }

                var tempPool = MemPool.Where(t => t != tx && !validTransactions.Contains(t)).ToList();
                var (isBalanceValid, _) = _transactionService.ValidateTransactionWithBalance(tx, Chain, tempPool);
                if (!isBalanceValid)
                {
                    MemPool.Remove(tx);
                    rejectedCount++;
                    continue;
                }

                validTransactions.Add(tx);
                MemPool.Remove(tx);
            }

            return validTransactions;
        }


        private bool IsTransactionInChain(string transactionId)
        {
            return Chain.Any(b => b.Transactions != null && b.Transactions.Any(t => t.Id == transactionId));
        }


        //public bool AddTransaction(Transaction transaction)
        //{
        //    var (isValid, message) = _transactionService.ValidateTransactionWithBalance(transaction, Chain, MemPool);
        //    if (!isValid)
        //    {
        //        Console.WriteLine($"Invalid transaction: {message}");
        //        return false;
        //    }
        //    MemPool.Add(transaction);
        //    return true;
        //}

        public bool AddTransaction(Transaction transaction)
        {
            if (transaction == null)
                return false;

            if (MemPool.Count >= 20)
            {
                Console.WriteLine("Invalid transaction: Mempool full");
                return false;
            }

            if (MemPool.Any(t => t.Id == transaction.Id) || IsTransactionInChain(transaction.Id))
            {
                Console.WriteLine("Invalid transaction: Duplicate");
                return false;
            }

            if ((DateTime.UtcNow - transaction.Timestamp).TotalMinutes > 5)
            {
                Console.WriteLine("Invalid transaction: Expired");
                return false;
            }

            var (isValid, message) = _transactionService.ValidateTransactionWithBalance(transaction, Chain, MemPool);
            if (!isValid)
            {
                Console.WriteLine($"Invalid transaction: {message}");
                return false;
            }

            MemPool.Add(transaction);
            return true;
        }


        private void AdjustDifficulty()
        {
            var recentBlocks = Chain.Where(b => b.Index > 0).TakeLast(_adjustmentInterval).ToList();

            if (recentBlocks.Count == 0)
                return;

            var avtTime = recentBlocks.Average(b => b.MiningDuration);

            if (avtTime < _targetBlockTime)
            {
                Difficulty++;
                Console.WriteLine($"Difficulty increased to {Difficulty}");
            }
            else if (avtTime > _targetBlockTime)
            {
                Difficulty = Math.Max(1, Difficulty - 1);
                Console.WriteLine($"Difficulty decreased to {Difficulty}");
            }

        }


        //private void AdjustDifficulty()
        //{
        //    var recentBlocks = Chain.Where(b => b.Index > 0).TakeLast(_adjustmentInterval).ToList();

        //    if (recentBlocks.Count == 0)
        //        return;

        //    var avtTime = recentBlocks.Average(b => b.MiningDuration);

        //    if (avtTime < _targetBlockTime)
        //    {
        //        Difficulty++;
        //        Console.WriteLine($"Difficulty increased to {Difficulty}");
        //    }
        //    else if (avtTime > _targetBlockTime)
        //    {
        //        Difficulty = Math.Max(1, Difficulty - 1);
        //        Console.WriteLine($"Difficulty decreased to {Difficulty}");
        //    }

        //}



        public bool IsValid(List<Block> chain)
        {
            for (int i = 1; i < chain.Count; i++)
            {
                var currentBlock = chain[i];
                var prevBlock = chain[i - 1];

                string currentHash = _hashingService.ComputeHash(currentBlock);

                if (currentBlock.Hash != currentHash)
                    return false;

                if (prevBlock.Hash != currentBlock.PrevHash)
                    return false;

                var target = new string('0', currentBlock.Difficulty);

                if (!currentBlock.Hash.StartsWith(target))
                    return false;
                if (currentBlock.MerkleRoot != _hashingService.ComputeMerkleRoot(currentBlock.Transactions))
                    return false;
            }

            return true;
        }



        private decimal GetCurrentMinerReward()
        {
            int halvings = Chain.Count / _halvingInterval;
            return _minerReward / (decimal)Math.Pow(2, halvings);
        }



        public BlockchainStats GetStatistics()
        {

            if (Chain == null || Chain.Count == 0)
                return null;

            var blocks = Chain.Where(b => b.Index > 0).ToList();

            if (!blocks.Any())
                return null;

            return new BlockchainStats
            {
                FastestBlock = blocks.OrderBy(b => b.MiningDuration).FirstOrDefault(),
                SlowestBlock = blocks.OrderByDescending(b => b.MiningDuration).FirstOrDefault(),
                MostAttemptsBlock = blocks.OrderByDescending(b => b.Nonce).FirstOrDefault(),
                AverageMiningDuration = blocks.Average(b => b.MiningDuration),
                AverageAttempts = blocks.Average(b => b.Nonce),
                MinDifficulty = blocks.Min(b => b.Difficulty),
                MaxDifficulty = blocks.Max(b => b.Difficulty)
            };
        }


        public bool TryAddReceivedBlock(Block block)
        {
            var lastBlock = Chain[^1];
            if (block.PrevHash != lastBlock.Hash)
            {
                Console.WriteLine("Invalid block: Previous hash does not match.");
                return false;
            }
            if (block.Index != lastBlock.Index + 1)
            {
                Console.WriteLine("Invalid block: Index is not sequential.");
                return false;
            }
            if (block.Hash != _hashingService.ComputeHash(block))
            {
                Console.WriteLine("Invalid block: Hash is incorrect.");
                return false;
            }
            if (block.MerkleRoot != _hashingService.ComputeMerkleRoot(block.Transactions))
            {
                Console.WriteLine("Invalid block: Merkle root is incorrect.");
                return false;
            }
            if (!block.Hash.StartsWith(new string('0', block.Difficulty)))
            {
                Console.WriteLine("Invalid block: Hash does not meet difficulty requirement.");
                return false;
            }

            var transactionId = block.Transactions.Where(t => !t.IsCoinbase).Select(t => t.Id).ToList();
            MemPool.RemoveAll(t => transactionId.Contains(t.Id));

            Chain.Add(block);
            return true;
        }


        public bool TryReplaceChain(List<Block> newChain)
        {
            if (newChain.Count <= Chain.Count)
            {
                return false;
            }

            if (newChain[0].Hash != Chain[0].Hash)
            {
                return false;
            }

            if (!IsValid(newChain))
            {
                return false;
            }
            Chain = newChain;

            var transactionRemove = Chain.SelectMany(tr => tr.Transactions).Select(t => t.Id).ToHashSet();

            MemPool.RemoveAll(x => transactionRemove.Contains(x.Id));
            return true;
        }
    }
}
