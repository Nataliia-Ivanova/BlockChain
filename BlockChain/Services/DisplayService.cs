using BlockChain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace BlockChain.Services
{
    public class DisplayService
    {
        public void ShowChain(List<Block> chain)
        {
            foreach (var block in chain)
            {
                Console.WriteLine($"Index {block.Index}");

                Console.WriteLine($"TimeStamp {block.Timestamp.ToString("o")}");
                Console.WriteLine($"Hash {block.Hash}");
                Console.WriteLine($"Nonce {block.Nonce}");
                Console.WriteLine($"MiningDuration {block.MiningDuration}");
                Console.WriteLine($"Difficulty {block.Difficulty}");
                Console.WriteLine($"PrevHash {block.PrevHash}");
                Console.WriteLine($"MerkleRoot {block.MerkleRoot}");
                if (block.Transactions != null && block.Transactions.Count > 0)
                {
                    Console.WriteLine("Transactions:");
                    foreach (var tx in block.Transactions)
                    {
                        Console.WriteLine($"  - {tx.ToRawString()}");
                    }
                }
                Console.WriteLine("\u001b[32m-------------------------------------------------------\u001b[0m");

            }
        }


        public void ShowWallet(Wallet wl)
        {
            Console.WriteLine($"Wallet Name: {wl.Name}");
            Console.WriteLine($"Wallet Address: {wl.Address}");
            Console.WriteLine($"Public Key: {Convert.ToBase64String(wl.PublicKey)}");
        }


        public void ShowMemPoll(List<Transaction> memPool)
        {
            if (memPool == null || memPool.Count == 0)
            {
                Console.WriteLine("MemPool is empty.");
                return;
            }
            Console.WriteLine("== MEMPOOL ==");
            foreach (var tx in memPool)
            {
                Console.WriteLine($"From: {tx.From} | To: {tx.To} | Amount: {tx.Amount}| Fee: {tx.Fee}  | Id: {tx.Id}");
            }
            Console.WriteLine("===================");
        }



        public void ShowStatistics(BlockchainStats stats)
        {
            if (stats == null)
            {
                Console.WriteLine("Statistic not unavailable: the chain is empty.");
                return;
            }

            Console.WriteLine("== СТАТИСТИКА ==");

            if (stats.FastestBlock != null)
                Console.WriteLine($"Fastest Block:           Index {stats.FastestBlock.Index} ({stats.FastestBlock.MiningDuration:F2} ms)");

            if (stats.SlowestBlock != null)
                Console.WriteLine($"Slowest Block:           Index {stats.SlowestBlock.Index} ({stats.SlowestBlock.MiningDuration:F2} ms)");

            if (stats.MostAttemptsBlock != null)
                Console.WriteLine($"Most Attempts Block:     Index {stats.MostAttemptsBlock.Index} ({stats.MostAttemptsBlock.Nonce} attempts)");

            Console.WriteLine($"Average Mining Duration: {stats.AverageMiningDuration:F2} ms");
            Console.WriteLine($"Average Attempts:        {stats.AverageAttempts:F2}");
            Console.WriteLine($"Min Difficulty:          {stats.MinDifficulty}");
            Console.WriteLine($"Max Difficulty:          {stats.MaxDifficulty}");
            Console.WriteLine("===================");
        }


        public void ShowSearchResult(TransactionSearchResult result)
        {
            if (result == null)
            {
                Console.WriteLine("Transaction not found.");
                return;
            }

            Console.WriteLine($"[Block #{result.BlockIndex}] From: {result.Transaction.From} | To: {result.Transaction.To} | Amount: {result.Transaction.Amount} | Id: {result.Transaction.Id}");
        }


        public void ShowSearchResults(List<TransactionSearchResult> results)
        {
            if (results == null || results.Count == 0)
            {
                Console.WriteLine("Transactions not found.");
                return;
            }

            Console.WriteLine($"\nFound transactions: {results.Count}");
            foreach (var item in results)
            {
                ShowSearchResult(item);
            }
        }


        public void PrintMerkleTree(List<Transaction> transactions)
        {

            if (transactions == null || transactions.Count == 0)
            {
                Console.WriteLine("No transactions to display in the Merkle Tree.");
                return;
            }
            var hashingService = new HashingService();
            var merkleRoot = hashingService.ComputeMerkleRoot(transactions);
            Console.WriteLine($"Merkle Root: {merkleRoot}");
            Console.WriteLine("Merkle Tree Structure:");
            List<string> currentLevel = new List<string>();
            foreach (var tx in transactions)
            {
                currentLevel.Add(hashingService.ComputeSha256(tx.ToRawString()));
            }
            int level = 0;
            while (currentLevel.Count > 1)
            {
                Console.WriteLine($"Level {level}:");
                for (int i = 0; i < currentLevel.Count; i++)
                {
                    Console.WriteLine($"{i}: {currentLevel[i]}");
                }
                
                List<string> nextLevel = new List<string>();
                for (int i = 0; i < currentLevel.Count; i += 2)
                {
                    if (i + 1 < currentLevel.Count)
                    {
                        string combinedHash = hashingService.ComputeSha256(currentLevel[i] + currentLevel[i + 1]);
                        nextLevel.Add(combinedHash);
                    }
                    else
                    {
                        nextLevel.Add(hashingService.ComputeSha256(currentLevel[i] + currentLevel[i]));
                    }
                }
                currentLevel = nextLevel;
                level++;
            }
            Console.WriteLine($"Level {level}: {currentLevel[0]}");
        }


        public void ShowMiningReport(MiningReport report)
        {
            if (report == null) return;

            Console.WriteLine("\n== MINING REPORT ==");
            Console.WriteLine($"Accepted transactions: {report.AcceptedCount}");
            Console.WriteLine($"Rejected transactions: {report.RejectedCount}");
            Console.WriteLine($"Total Fees: {report.TotalFee}");
            Console.WriteLine($"Mining Reward: {report.MiningReward}");
            Console.WriteLine($"Miner Received: {report.MinerReceived}");
            Console.WriteLine($"Remaining Mempool count: {report.RemainingMempoolCount}");
            Console.WriteLine("===================\n");
        }



    }
}
