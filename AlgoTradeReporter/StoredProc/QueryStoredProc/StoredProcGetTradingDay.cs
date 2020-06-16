/* ==============================================================================
 * ClassName：StoredProcGetTradingDay
 * Description：
 * Author：zhaoyu
 * Created Time：2015/1/7 16:34:51
 * Revision Time：
 * Notes：
 * @version 1.0
* ==============================================================================*/


using AlgoTradeReporter.StoredProc.QueryStoredProc;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace AlgoTradeReporter.StoredProc
{
    class StoredProcGetTradingDay : AbstractQueryStoredProc
    {
        private const string STORED_PROC_NAME = "spu_GetTradeDate";

        private List<string> tradingDays;
        public StoredProcGetTradingDay()
        {
            this.tradingDays = new List<string>();
            this.storedProcName = STORED_PROC_NAME;
        }

        protected override void parseResult(SqlDataReader reader_)
        {
            while (reader_.Read())
            {
                tradingDays.Add(reader_["tradingDay"].ToString());
            }
        }


        public List<string> getTradingDays()
        {
            return this.tradingDays;
        }

        public bool isTradingDay(string date_)
        {
            return tradingDays.Contains(date_);
        }

        public bool isTradingDay(List<string> dates_)
        {
            foreach (string day in dates_)
            {
                if (!isTradingDay(day))
                    return false;
            }
            return true;
        }
    }
}
