using BlockChain.Models;
using BlockChain.Services;
using System;
using System.Collections.Generic;
using System.Text.Json;




var displayService = new DisplayService();
var testBlockChain = new BlockChainService(1, 3000, 10);
var transactionService = new TransactionService();
var walletService = new WalletService();
var balanceService = new BalanceService();


var aliceWallet = walletService.CreateWallet("Alice");
var bobWallet = walletService.CreateWallet("Bob");
var charlieWallet = walletService.CreateWallet("Charlie");
var daveWallet = walletService.CreateWallet("Dave");

int port = int.Parse(args[0]);

var nodes = args.Skip(1).Select(x =>
{
    var p = x.Split(":");
    return new Node(p[0], int.Parse(p[1]));

}).ToList();


var networkService = new NetworkService(port, nodes);
networkService.Start(async (type, message) =>
{
    Console.WriteLine($"Received message of type {type}: {message}");

    switch (type)
    {
        case "CHAIN":
            var chain = JsonSerializer.Deserialize<List<Block>>(message);
            if (chain != null)
            {
                var isreplace = testBlockChain.TryReplaceChain(chain);
                Console.WriteLine($"Chain replacement {(isreplace ? "successful" : "failed")}");
            }
            break;
        case "GET_CHAIN":
            await networkService.BroadCastAsync("CHAIN", testBlockChain.Chain);
            break;
        case "Transaction":
            var transaction = JsonSerializer.Deserialize<Transaction>(message);
            if (transaction != null)
            {
                Console.WriteLine($"Received transaction from {transaction.From} to {transaction.To} for amount {transaction.Amount}");
                testBlockChain.AddTransaction(transaction);
            }
            break;
        case "Block":
            var block = JsonSerializer.Deserialize<Block>(message);
            if (block != null)
            {
                Console.WriteLine($"Received block with index {block.Index} and hash {block.Hash}");
                testBlockChain.TryAddReceivedBlock(block);
            }
            break;
        default:
            Console.WriteLine($"Unknown message type: {type}");
            break;
    }
});



List<Transaction> pendingTransactions = new List<Transaction>();
List<Wallet> wallets = new List<Wallet>();



//var aliceWallet = walletService.CreateWallet("Alice");
//var bobWallet = walletService.CreateWallet("Bob");
//var charlieWallet = walletService.CreateWallet("Charlie");
//var daveWallet = walletService.CreateWallet("Dave");

//displayService.ShowWallet(aliceWallet);
//displayService.ShowWallet(bobWallet);
//displayService.ShowWallet(charlieWallet);
//displayService.ShowWallet(daveWallet);



testBlockChain.MinePendingTransaction(aliceWallet.Address);
//testBlockChain.MinePendingTransaction(aliceWallet.Address);
//testBlockChain.MinePendingTransaction(aliceWallet.Address);
//testBlockChain.MinePendingTransaction(aliceWallet.Address);


var tx1 = transactionService.CreateTransaction(aliceWallet, bobWallet.Address, 10.0m, 2);
var tx2 = transactionService.CreateTransaction(aliceWallet, charlieWallet.Address, 5.0m, 7);
//var tx3 = transactionService.CreateTransaction(aliceWallet, daveWallet.Address, 25.0m, 15);
//var tx4 = transactionService.CreateTransaction(aliceWallet, daveWallet.Address, 15.0m, 25);
//var tx5 = transactionService.CreateTransaction(aliceWallet, bobWallet.Address, 15.0m, 40);
//var tx6 = transactionService.CreateTransaction(aliceWallet, charlieWallet.Address, 15.0m, 50);
//var tx7 = transactionService.CreateTransaction(aliceWallet, bobWallet.Address, 15.0m, 60);

testBlockChain.AddTransaction(tx1);
testBlockChain.AddTransaction(tx2);
//testBlockChain.AddTransaction(tx3);
//testBlockChain.AddTransaction(tx4);
//testBlockChain.AddTransaction(tx5);
//testBlockChain.AddTransaction(tx6);
//testBlockChain.AddTransaction(tx7);


while (true)
{    
    Console.WriteLine("\n\u001b[32m--- Main Menu ---\u001b[0m");
    Console.WriteLine("\u001b[32m1.\u001b[0m Mine Block");
    Console.WriteLine("\u001b[32m2.\u001b[0m Display Blockchain");
    Console.WriteLine("\u001b[32m3.\u001b[0m Validate Blockchain");
    Console.WriteLine("\u001b[32m4.\u001b[0m Blockchain Statistic");

    //Console.WriteLine("\u001b[32m5.\u001b[0m Search Transactions");
    Console.WriteLine("\u001b[32m5.\u001b[0m Show MemPool");

    Console.WriteLine("\u001b[32m6.\u001b[0m Add new wallet");
    Console.WriteLine("\u001b[32m7.\u001b[0m Show all wallets");
    Console.WriteLine("\u001b[32m8.\u001b[0m Show wallets balances");
    Console.WriteLine("\u001b[32m9.\u001b[0m Add transaction");
    Console.WriteLine("\u001b[32m10.\u001b[0m Exit");
    Console.WriteLine("\u001b[32m11.\u001b[0m Sync");
    Console.WriteLine("\u001b[32m12.\u001b[0m Test Mempool Validation Errors");
    Console.Write("\nChoice: ");

    var choice = Console.ReadLine();


    switch (choice)
    {
        case "1":
            var newBlock = testBlockChain.MinePendingTransaction(aliceWallet.Address);

            displayService.ShowMiningReport(testBlockChain.LastMiningReport);

            networkService.BroadCastAsync("Block", newBlock);
            break;

            break;
        case "2":
            Console.WriteLine("\u001b[32m-------------------------------------------------------\u001b[0m");
            displayService.ShowChain(testBlockChain.Chain);

            //displayService.PrintMerkleTree(testBlockChain.Chain[testBlockChain.Chain.Count - 1].Transactions);

            break;
        case "3":
            bool isValid = testBlockChain.IsValid(testBlockChain.Chain);
            Console.WriteLine($"Blockchain is {(isValid ? "valid" : "invalid")}");
            break;
        case "4":
            var stats = testBlockChain.GetStatistics();
            displayService.ShowStatistics(stats);
            break;
        case "5":
            //HandleSearchMenu();
            //break;
            displayService.ShowMemPoll(testBlockChain.MemPool);
            //case "6":
            //    AddWalletInteractive();
            break;
        case "7":
            if (wallets.Count == 0)
            {
                Console.WriteLine("\nNo wallets available.");
            }
            else
            {
                for (int i = 0; i < wallets.Count; i++)
                {
                    Console.WriteLine($"\n\u001b[33m  -----  Wallet {i + 1}  -----\u001b[0m");
                    displayService.ShowWallet(wallets[i]);
                }
            }
            break;


        case "8":

            Console.WriteLine($"Alice Balance {balanceService.GetBalance(aliceWallet.Address, testBlockChain.Chain)}");
            Console.WriteLine($"Bob Balance {balanceService.GetBalance(bobWallet.Address, testBlockChain.Chain)}");
            Console.WriteLine($"Charlie Balance {balanceService.GetBalance(charlieWallet.Address, testBlockChain.Chain)}");
            Console.WriteLine($"Dave Balance {balanceService.GetBalance(daveWallet.Address, testBlockChain.Chain)}");
            break;

            //if (wallets.Count == 0)
            //{
            //    Console.WriteLine("\nNo wallets available.");
            //}
            //else
            //{
            //    Console.WriteLine("\n--- Wallet Balances ---");
            //    foreach (var wallet in wallets)
            //    {
            //        decimal balance = balanceService.GetBalance(wallet.Address, testBlockChain.Chain);
            //        Console.WriteLine($"Wallet: {wallet.Name} ({wallet.Address}) - Balance: {balance}");
            //    }
            //}
            //break;

        case "9":

            //Console.WriteLine($"Alice Balance {balanceService.GetBalance(aliceWallet.Address, testBlockChain.Chain)}");
            //Console.WriteLine($"Bob Balance {balanceService.GetBalance(bobWallet.Address, testBlockChain.Chain)}");
            //Console.WriteLine($"Charlie Balance {balanceService.GetBalance(charlieWallet.Address, testBlockChain.Chain)}");
            //Console.WriteLine($"Dave Balance {balanceService.GetBalance(daveWallet.Address, testBlockChain.Chain)}");
            //break;

            var tx10 = transactionService.CreateTransaction(aliceWallet, bobWallet.Address, 10.0m, 5);
            testBlockChain.AddTransaction(tx10);
            var tx11 = transactionService.CreateTransaction(aliceWallet, bobWallet.Address, 10.0m, 1);
            testBlockChain.AddTransaction(tx11);
            var tx12 = transactionService.CreateTransaction(aliceWallet, bobWallet.Address, 10.0m, 5);
            testBlockChain.AddTransaction(tx12);
            var tx13 = transactionService.CreateTransaction(aliceWallet, bobWallet.Address, 10.0m, 1);
            testBlockChain.AddTransaction(tx13);
            var tx14 = transactionService.CreateTransaction(aliceWallet, bobWallet.Address, 10.0m, 1);
            testBlockChain.AddTransaction(tx14);
            var tx15 = transactionService.CreateTransaction(aliceWallet, bobWallet.Address, 10.0m, 0.5m);
            testBlockChain.AddTransaction(tx15);

            break;
        case "10":
            return;
        case "11":
            await networkService.BroadCastAsync("GET_CHAIN", "");
            break;
        case "12":
            Console.WriteLine("\n--- Testing Mempool Validations ---");

            //Дублікат
            Console.WriteLine("\n[Test 1] Trying to add duplicate transaction:");
            var duplicateTx = transactionService.CreateTransaction(aliceWallet, bobWallet.Address, 1.0m, 2);
            testBlockChain.AddTransaction(duplicateTx);
            testBlockChain.AddTransaction(duplicateTx);

            //Недостатній баланс
            Console.WriteLine("\n[Test 2] Trying to spend more than balance:");
            try
            {
                var poorTx = transactionService.CreateTransaction(bobWallet, aliceWallet.Address, 999999m, 5);
                testBlockChain.AddTransaction(poorTx);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Caught creation error: {ex.Message}");
            }

            //Expired
            Console.WriteLine("\n[Test 3] Trying to add expired transaction (> 5 mins):");
            var expiredTx = transactionService.CreateTransaction(aliceWallet, bobWallet.Address, 1.0m, 2);
            expiredTx.Timestamp = DateTime.UtcNow.AddMinutes(-6);
            testBlockChain.AddTransaction(expiredTx);

            break;
        default:
            Console.WriteLine("Invalid choice. Please try again.");
            break;
    }
}


//void HandleAddBlockMenu()
//{
//    bool inSubMenu = true;

//    while (inSubMenu)
//    {
//        Console.WriteLine("\n\u001b[33m  --- Adding transaction ---\u001b[0m");
//        Console.WriteLine("\u001b[33m    1.\u001b[0m Add transaction");
//        Console.WriteLine("\u001b[33m    2.\u001b[0m Show pending transactions");
//        Console.WriteLine("\u001b[33m    3.\u001b[0m Exit and add block");
//        Console.Write("  Choice: ");

//        var subChoice = Console.ReadLine();

//        switch (subChoice)
//        {
//            case "1":
//                AddTransactionInteractive();
//                break;

//            case "2":
//                ShowPendingTransactions();
//                break;

//            case "3":
//                if (pendingTransactions.Count == 0)
//                {
//                    Console.WriteLine("Transactions list is empty. Block is NOT created!");
//                }
//                else
//                {
//                    if (wallets.Count == 0)
//                    {
//                        Console.WriteLine("Error: No wallets available to receive mining reward!");
//                        break;
//                    }

//                    Console.WriteLine("\nSelect MINER wallet (who will receive the block reward):");
//                    var minerWallet = SelectWalletFromList(wallets);
//                    if (minerWallet == null) break;
//                    testBlockChain.AddBlock(new List<Transaction>(pendingTransactions), minerWallet.Address);

//                    Console.WriteLine($"\nBlock mined successfully! {pendingTransactions.Count} transactions included.");
//                    Console.WriteLine($"Reward successfully sent to miner: {minerWallet.Name} ({minerWallet.Address})");

//                    pendingTransactions.Clear();
//                }
//                inSubMenu = false;
//                break;



//            //if (pendingTransactions.Count == 0)
//            //{
//            //    Console.WriteLine("Transactions list is empty. Block is NOT created!");
//            //}
//            //else
//            //{
//            //    testBlockChain.AddBlock(new List<Transaction>(pendingTransactions), aliceWallet.Address);
//            //    Console.WriteLine($"\nBlock added successfully! {pendingTransactions.Count} transactions!");

//            //    pendingTransactions.Clear();
//            //}
//            //inSubMenu = false;
//            //break;





//            default:
//                Console.WriteLine("Invalid choice. Please try again.");
//                break;
//        }
//    }
//}

//void AddWalletInteractive()
//{
//    string name;
//    do
//    {
//        Console.Write("Enter Name: ");
//        name = Console.ReadLine();

//        if (string.IsNullOrWhiteSpace(name))
//        {
//            Console.WriteLine("Error: Name cannot be empty. Please try again.");
//        }
//    }
//    while (string.IsNullOrWhiteSpace(name));

//    Wallet wallet = walletService.CreateWallet(name);
//    wallets.Add(wallet);
//    Console.WriteLine("\nWallet created successfully");

//    Console.Write("Do you want to mine a starting block to give this wallet initial balance? (\u001b[33my/n\u001b[0m): ");
//    var answer = Console.ReadLine()?.Trim().ToLower();
//    if (answer == "y")
//    {
//        testBlockChain.AddBlock(new List<Transaction>(), wallet.Address);
//        Console.WriteLine($"\nStarting block mined! Initial reward sent to {wallet.Name}.");
//    }


//}

//void AddTransactionInteractive()
//{
//    Console.Write("Enter From: ");
//    string from = Console.ReadLine();

//    Console.Write("Enter To: ");
//    string to = Console.ReadLine();

//    Console.Write("Enter Amount: ");
//    if (!decimal.TryParse(Console.ReadLine(), out decimal amount))
//    {
//        Console.WriteLine("Error: incorrect amount (should be a number)!");
//        return;
//    }

//    try
//    {
//        var tx = transactionService.CreateTransaction(from, to, amount);
//        pendingTransactions.Add(tx);
//        Console.WriteLine("Transaction added successfully!");
//    }
//    catch (InvalidOperationException ex)
//    {
//        Console.WriteLine($"Error creating transaction: {ex.Message}");
//    }
//    catch (Exception ex)
//    {
//        Console.WriteLine($"Unexpected error: {ex.Message}");
//    }
//}


void AddTransactionInteractive()
{
    if (wallets == null || wallets.Count < 2)
    {
        Console.WriteLine("Error: Need at least 2 wallets to create a transaction!");
        return;
    }

    Console.WriteLine("\nSelect \u001b[94mSENDER\u001b[0m (From):");
    var senderWallet = SelectWalletFromList(wallets);
    if (senderWallet == null) return;

    Console.WriteLine("\nSelect \u001b[94mRECIPIENT\u001b[0m (To):");
    var recipientWallet = SelectWalletFromList(wallets);
    if (recipientWallet == null) return;

    if (senderWallet == recipientWallet)
    {
        Console.WriteLine("Error: Sender and recipient cannot be the same wallet!");
        return;
    }

    Console.Write("Enter Amount: ");
    if (!decimal.TryParse(Console.ReadLine(), out decimal amount))
    {
        Console.WriteLine("Error: incorrect amount (should be a number)!");
        return;
    }

    Console.Write("Enter Fee: ");
    if (!decimal.TryParse(Console.ReadLine(), out decimal fee))
    {
        Console.WriteLine("Error: incorrect fee (should be a number)!");
        return;
    }

    try
    {
        var tx = transactionService.CreateTransaction(
            senderWallet,
            recipientWallet.Address,
            amount,
            fee
        );

        pendingTransactions.Add(tx);
        Console.WriteLine("Transaction added successfully!");
    }
    catch (InvalidOperationException ex)
    {
        Console.WriteLine($"Error creating transaction: {ex.Message}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Unexpected error: {ex.Message}");
    }
}

Wallet SelectWalletFromList(List<Wallet> walletList)
{
    for (int i = 0; i < walletList.Count; i++)
    {
        Console.WriteLine($"\u001b[94m{i + 1}.\u001b[0m  {walletList[i].Name} ({walletList[i].Address})");
    }

    while (true)
    {
        Console.Write("Choice: ");
        if (int.TryParse(Console.ReadLine(), out int index) && index >= 1 && index <= walletList.Count)
        {
            Console.WriteLine($"Selected wallet: {walletList[index - 1].Name} ({walletList[index - 1].Address})");
            return walletList[index - 1];
        }

        Console.WriteLine("Invalid choice. Please enter a valid number from the list.");
    }
}




void ShowPendingTransactions()
{
    if (pendingTransactions.Count == 0)
    {
        Console.WriteLine("Pending transactions list is empty.");
        return;
    }

    Console.WriteLine("\n--- Created transactions (PENDING) ---");
    foreach (var tx in pendingTransactions)
    {
        Console.WriteLine(tx.ToRawString());
    }
}


void HandleSearchMenu()
{
    Console.WriteLine("\n--- SEARCH MENU ---");
    Console.WriteLine("1. Search transaction by Id");
    Console.WriteLine("2. Search transaction by User");
    Console.WriteLine("3. Search transaction More than...");
    Console.WriteLine("4. Search Largest transaction");
    Console.Write("Choice: ");

    var searchChoice = Console.ReadLine();

    switch (searchChoice)
    {
        case "1":
            Console.Write("Enter transaction Id: ");
            string id = Console.ReadLine();
            var resultById = transactionService.GetById(testBlockChain.Chain, id);
            displayService.ShowSearchResult(resultById);
            break;

        case "2":
            Console.Write("Enter User name: ");
            string username = Console.ReadLine();
            var resultsByUser = transactionService.GetByUser(testBlockChain.Chain, username);
            displayService.ShowSearchResults(resultsByUser);
            break;

        case "3":
            Console.Write("Enter Amount: ");
            if (decimal.TryParse(Console.ReadLine(), out decimal minAmount))
            {
                var resultsByAmount = transactionService.GetByMinAmount(testBlockChain.Chain, minAmount);
                displayService.ShowSearchResults(resultsByAmount);
            }
            else
            {
                Console.WriteLine("Error: incorrect amount (should be a number)!");
            }
            break;

        case "4":
            var largestTx = transactionService.GetLargestTransaction(testBlockChain.Chain);
            Console.WriteLine("\nLargest transaction:");
            displayService.ShowSearchResult(largestTx);
            break;

        default:
            Console.WriteLine("Invalid choice. Please try again.");
            break;
    }
}




