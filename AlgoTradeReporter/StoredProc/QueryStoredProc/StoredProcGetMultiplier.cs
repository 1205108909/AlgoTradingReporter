using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace AlgoTradeReporter.StoredProc.QueryStoredProc
{
    class StoredProcGetMultiplier : AbstractQueryStoredProc
    {
        private const string STORED_PROC_NAME = "spu_GetMultiplier";

        private Dictionary<string, int> multipliers;

        public StoredProcGetMultiplier()
        {
            this.multipliers = new Dictionary<string, int>();
            this.storedProcName = STORED_PROC_NAME;
        }

        public int getMultiplier(string symbol)
        {
            if (this.multipliers.ContainsKey(symbol))
            {
                return multipliers[symbol];
            }
            else
            {
                return -1;
            }
        }

        public bool isRepo(string symbol)
        {
            return this.multipliers.ContainsKey(symbol);
        }

        protected override void parseResult(SqlDataReader reader_)
        {
            while (reader_.Read())
            {
                string symbol = reader_["symbol"].ToString();
                int multiplier = Int32.Parse(reader_["multiplier"].ToString());
                this.multipliers.Add(symbol, multiplier);
            }
        }


    }
}
