using BlockChain.Models;
using System;
using System.Collections.Generic;
using System.Runtime.Intrinsics.Arm;
using System.Security.Cryptography;
using System.Text;

namespace BlockChain.Services
{
    public class HashingService
    {
        public string ComputeHash(Block block)
        {
            //var transactionsRaw = string.Join("", block.Transactions.ConvertAll(t => $"{t.ToRawString()}"));
            //var transactionHash = ComputeSha256(transactionsRaw);

            var transactionHash = ComputeMerkleRoot(block.Transactions);
            //block.MerkleRoot = transactionHash;

            string rawData = $"{block.Index}{transactionHash}{block.Timestamp.ToString("o")}{block.PrevHash}{block.Nonce}{block.Difficulty}";
            return ComputeSha256(rawData);
        }

        public string ComputeSha256(string input)
        {
            // var inputBytes - це byte[] - кожен елемент є цілим числом від 0 до 255
            // Encoding.UTF8.GetBytes(input) - кодує кожен символ у байти за стандартом UTF-8 (Hi! -> [72, 105, 33])

            var inputBytes = Encoding.UTF8.GetBytes(input);
            var hashBytes = SHA256.HashData(inputBytes);

            return Convert.ToHexString(hashBytes).ToLower();
        }

        public string ComputeMerkleRoot(List<Transaction> transactions)
        {
            if (transactions.Count == 0)
                return string.Empty;

            List<string> merkleLeaves = new List<string>();
            foreach (var tx in transactions)
            {
                merkleLeaves.Add(ComputeSha256(tx.ToRawString()));
            }

            while (merkleLeaves.Count > 1)
            {
                List<string> newLevel = new List<string>();
                for (int i = 0; i < merkleLeaves.Count; i += 2)
                {
                    if (i + 1 < merkleLeaves.Count)
                    {
                        string combinedHash = ComputeSha256(merkleLeaves[i] + merkleLeaves[i + 1]);
                        newLevel.Add(combinedHash);
                    }
                    else
                    {
                        newLevel.Add(ComputeSha256(merkleLeaves[i] + merkleLeaves[i]));
                    }
                }
                merkleLeaves = newLevel;
            }
            return merkleLeaves[0];
        }


    }
}
