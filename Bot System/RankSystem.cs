using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot
{
    public static class RankSystem
    {
        /// <summary>
        /// An account that is associated with a discord account Id
        /// </summary>
        public struct Rank
        {
            public ulong RoleId { get; set; }   // role id in the discord server
            public double Cost { get; set; }    // cost of the rank in currency

            public string Description { get; set; }     // description of what this rank is 

            /// <summary>
            /// Returns the cost with the correct formating (with prefixs and suffixs)
            /// </summary>
            /// <returns></returns>
            public override string ToString()
            {
                return Configuration.Config.Economy.CurrencyPrefixs + Cost.ToString() + Configuration.Config.Economy.CurrencySuffixs;
            }
        }
    }
}
