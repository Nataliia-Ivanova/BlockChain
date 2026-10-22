using BlockChain.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlockChain.Services
{
    public class MiningService
    {
        //Сервіс для майнінгу блоку. Майнінг - це процес знаходження правильного хешу для блоку, який відповідає певним умовам (наприклад, починається з певної кількості нулів).
        //Це забезпечує безпеку та складність процесу додавання нових блоків до блокчейну.
        //2 references
        private readonly HashingService _hashingService;
        //0 references
        public MiningService()
        {
            _hashingService = new HashingService();
        }

        //0 references
        public void MineBlock(Block block, int difficulty)
        {
            // Майнінг блоку: знаходження правильного хешу, який починається з певної кількості нулів (визначеної параметром difficulty).
            string target = new string('0', difficulty); //"0000..."

            var sw = Stopwatch.StartNew(); 

            while (true)
            {
                block.Hash = _hashingService.ComputeHash(block); // Обчислення хешу блоку з поточним значенням Nonce.
                if (block.Hash.Substring(0, difficulty) == target) // Перевірка, чи починається хеш з потрібної кількості нулів. Якщо так, майнінг завершено.
                {
                    block.MiningDuration = sw.Elapsed.TotalMilliseconds; // Встановлюємо час майнінгу блоку.
                    break;
                }

                block.Nonce++;

                if (block.Nonce % 100000 == 0) // Виводимо повідомлення кожні 100000 спроб для відстеження прогресу.
                {
                    Console.WriteLine($"Mining in progress... Current Nonce: {block.Nonce}, Current Hash: {block.Hash}");
                }
            }
        }
    }

}



