using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot
{
    public static class Economy
    {
        public const string saveDirectory = @"/Accounts/";      // the directory where all accounts are saved into.

        /// <summary>
        /// An account that is associated with a discord account Id
        /// </summary>
        public struct Account
        {
            /// <summary>
            /// The discord account Id associated with the account
            /// </summary>
            public ulong Id { get; set; }
            /// <summary>
            /// The current balance of the account
            /// </summary>
            public double Balance { get; set;  }

            /// <summary>
            /// A list of transactions that the account has made.
            /// </summary>
            public List<Transaction> Transactions { get; set; }
            /// <summary>
            /// Is the account private? If the account is private, other people (besides an admin) will not be able to check the balance of this account.
            /// </summary>
            public bool IsPrivate { get; set; }
            /// <summary>
            /// A list of logs that have happened, such as transactions, access and more.
            /// </summary>
            public List<Log> Logs { get; set; }
            /// <summary>
            /// the time since the user has last gotten their daily currency amount through the /daily command.
            /// </summary>
            public DateTime TimeSinceLastDaily { get; set; }

            /// <summary>
            /// Returns the account balance with the correct formating (with prefixs and suffixs)
            /// </summary>
            /// <returns></returns>
            public override string ToString()
            {
                return Configuration.Config.Economy.CurrencyPrefixs + Balance.ToString() + Configuration.Config.Economy.CurrencySuffixs;
            }

        }

        /// <summary>
        /// Converts any currency to a formated version with suffixs and prefixs.
        /// </summary>
        /// <param name="currency"></param>
        /// <returns></returns>
        public static string CurrencyToFormatedString(object currency)
        {
            return Configuration.Config.Economy.CurrencyPrefixs + currency.ToString() + Configuration.Config.Economy.CurrencySuffixs;
        }

        /// <summary>
        /// A transaction of currency between a Payer and Payee
        /// </summary>
        public struct Transaction
        {
            /// <summary>
            /// Information about the transaction.
            /// </summary>
            public string Information { get; set; }
            /// <summary>
            /// The amount that was in the transaction.
            /// </summary>
            public double Amount { get; set; }
            /// <summary>
            /// Discord account Id for the person who issued the payment
            /// </summary>
            public ulong PayerDiscordAccountId { get; set; }
            /// <summary>
            /// Discord account Id for the person who receives the payment
            /// </summary>
            public ulong PayeeDiscordAccountId { get; set; }
            /// <summary>
            /// The time that the transaction occurred at
            /// </summary>
            public DateTime TimeOfTransaction { get; set; }

            /// <summary>
            /// Returns the amount with the correct formating (with prefixes and suffixes)
            /// </summary>
            /// <returns></returns>
            public override string ToString()
            {
                return Configuration.Config.Economy.CurrencyPrefixs + Amount.ToString() + Configuration.Config.Economy.CurrencySuffixs;
            }

            /// <summary>
            /// Create a new transaction with DateTime.Now as default
            /// </summary>
            /// <param name="information">information relating to the transaction</param>
            /// <param name="amount">the amount of currency involved in the transaction</param>
            /// <param name="payerId">Discord account Id for the person who issued the payment</param>
            /// <param name="payeeId">Discord account Id for the person who receives the payment</param>
            public Transaction(string information, double amount, ulong payerId, ulong payeeId)
            {
                Information = information;
                Amount = amount;
                PayerDiscordAccountId = payerId;
                PayeeDiscordAccountId = payeeId;
                TimeOfTransaction = DateTime.Now;
            }
        }

        /// <summary>
        /// A log of what has happens at a particular time.
        /// </summary>
        public struct Log
        {
            /// <summary>
            /// Details and information about the log
            /// </summary>
            public string Details { get; set; }
            /// <summary>
            /// The time that this event occurred.
            /// </summary>
            public DateTime TimeOfLog { get; set; }

            /// <summary>
            /// Create a new log with the current datetime
            /// </summary>
            /// <param name="details"></param>
            public Log (string details)
            {
                Details = details;
                TimeOfLog = DateTime.Now;
            }
        }


        /// <summary>
        /// Save an account's data to the database
        /// </summary>
        /// <param name="account"></param>
        public static void UpdateAccount(Account account)
        {
            string json = JsonConvert.SerializeObject(account);     // convert the account into json format for saving into a file.
            File.WriteAllText(Directory.GetCurrentDirectory() + saveDirectory + account.Id.ToString() + ".json", json);    // save json to a file, under their discord Id
        }

        /// <summary>
        /// Get an account from a discord Id. 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static Account GetAccountFromId(ulong id)
        {
            string accountFileDirectory = Directory.GetCurrentDirectory() + saveDirectory + id.ToString() + ".json";    // where the account file is located
            try
            {
                string json = File.ReadAllText(accountFileDirectory); // read account file and get json inside of it.
                return JsonConvert.DeserializeObject<Account>(json);    // deserialise json into an account format and return it.
            } catch
            {
                // if their is no account currently in existence than we can create a new one for them.
                if (!File.Exists(accountFileDirectory))
                {
                    Account newAccount = new Account(); // creating a new account instance
                    newAccount.Id = id;
                    newAccount.Balance = 0;
                    newAccount.IsPrivate = false;
                    newAccount.Transactions = new List<Transaction>();
                    newAccount.Logs = new List<Log>();
                    newAccount.Logs.Add(new Log()
                    {
                        Details = "Created account.",
                        TimeOfLog = DateTime.Now,
                    });
                    newAccount.TimeSinceLastDaily = DateTime.Now.AddDays(-1);   // we want the user to be able to use the daily command straight away, so we need to make it already a day 
                    UpdateAccount(newAccount);
                    return newAccount; 
                }

                return new Account();   // worst case scenario if for whatever reason the above does not work, we will just return a null account.
            }
        }

    }
}
