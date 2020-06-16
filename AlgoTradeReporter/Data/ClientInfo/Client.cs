/* ==============================================================================
 * ClassName：TradeTab
 * Description：
 * Author：zhaoyu
 * Created Time：2018/2/1 10:24:03
 * Revision Time：
 * Notes：
 * @version 1.0
 * 
 * @version 1.7
 * Revision Time: 2016-12-12
 * Notes : Client Merge order is not supported anymore. Remove relevent method and variables.
 *          The reason is because that it is more reasonable to send to client containing each of her order's detail.
 *          
 *          Add Client abbr item.
* ==============================================================================*/

using AlgoTradeReporter.Data.ClientInfo;
using AlgoTradeReporter.Data.Commons;
using AlgoTradeReporter.Data.Trades;
using AlgoTradeReporter.StoredProc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgoTradeReporter.Data
{
    class Client
    {
        private string accountId;
        private string clientName;
        private string email;
        private string repsentEmail;
        private bool sendToClient;
        //private bool mergeOrders;
        private ReportFrequency frequency;
        private bool isValid;
        private List<string> ccList;
        private List<string> tradingDays;
        private List<SavedClientOrder> trades;

        private string clientAbbr;

        /// <summary>
        /// Construct a client, which is read from database
        /// </summary>
        /// <param name="accountId_">Client AccountId</param>
        /// <param name="clientName_">client name, say the company name</param>
        /// <param name="email_">Client Email</param>
        /// <param name="repsentEmail_">Representative Email, one guy of our own who covers this client</param>
        /// <param name="sendToClient_">If true, send report to client; if false, send to representative</param>
        /// <param name="mergeOrders_">If true, same stock, same day should be merged as one order; false otherwise</param>
        /// <param name="frequency_">Report frequency</param>
        /// <param name="isValid_">If this client is valid</param>
        public Client(string accountId_, string clientName_, string email_, 
            string repsentEmail_, bool sendToClient_,
            ReportFrequency frequency_, bool isValid_, string clientAbbr_)
        {
            accountId = accountId_;
            clientName = clientName_;
            email = email_;
            repsentEmail = repsentEmail_;
            sendToClient = sendToClient_;
            //mergeOrders = mergeOrders_;
            frequency = frequency_;

            isValid = isValid_;

            this.clientAbbr = clientAbbr_;
            tradingDays = new List<string>();
            ccList = new List<string>();
            trades = new List<SavedClientOrder>();
        }


        /// <summary>
        /// TradingDays
        /// </summary>
        /// <param name="tradingDays_">Input tradingDays</param>
        public void setTradingDays(List<string> tradingDays_)
        {
            tradingDays.AddRange(tradingDays_);
        }
        
        public void initClientReport()
        {
            string reportStartDate = tradingDays[0];
            string reportEndDate = tradingDays[tradingDays.Count - 1];
        }

        public string getAccountId()
        {
            return accountId;
        }
        public string getClientName()
        {
            return clientName;
        }

        public void setEmail(string email)
        {
            this.email = email;
        }

        public void setRepresentEmail(string email)
        {
            this.repsentEmail = email;
        }

        public bool getIsValid()
        {
            return isValid;
        }
        public List<string> getTradingDay()
        {
            return tradingDays;
        }
        public ReportFrequency getReportFrequency()
        {
            return frequency;
        }
        public string getClientAbbr()
        {
            return this.clientAbbr;
        }

        /// <summary>
        /// Get all trades of the client for the input tradingDays
        /// </summary>
        /// <returns>trades</returns>
        public List<SavedClientOrder> getClientTrades()
        {
            return trades;
        }

        public List<SavedClientOrder> getRepoTrades()
        {
            List<SavedClientOrder> repoTrades = new List<SavedClientOrder>();
            foreach (SavedClientOrder trade in trades)
            {
                //if (StoredProcMgr.MANAGER.isRepo(trade.getSymbol()))
                if (trade.GetSecurityType().Equals(AlgoTrading.Data.SecurityType.RPO))
                {
                    repoTrades.Add(trade);
                }
            }
            return repoTrades;
        }

        public List<SavedClientOrder> getEqtTrades()
        {
            List<SavedClientOrder> eqtTrades = new List<SavedClientOrder>();
            foreach (SavedClientOrder trade in trades)
            {
                if(trade.GetSecurityType().Equals(AlgoTrading.Data.SecurityType.EQA) 
                    || trade.GetSecurityType().Equals(AlgoTrading.Data.SecurityType.FDC)
                    || trade.GetSecurityType().Equals(AlgoTrading.Data.SecurityType.FDO))
                //if (StoredProcMgr.MANAGER.isEqt(trade.getSymbol()))
                //if (!StoredProcMgr.MANAGER.isRepo(trade.getSymbol()))
                {
                    eqtTrades.Add(trade);
                }
            }
            return eqtTrades;

        }

        public List<SavedClientOrder> getFutureTrades()
        {
            List<SavedClientOrder> futTrades = new List<SavedClientOrder>();
            foreach (SavedClientOrder trade in trades)
            {
                //if (StoredProcMgr.MANAGER.isFuture(trade.getSymbol()))
                //if (!StoredProcMgr.MANAGER.isRepo(trade.getSymbol()))
                if (trade.GetSecurityType().Equals(AlgoTrading.Data.SecurityType.FTR))
                {
                    futTrades.Add(trade);
                }
            }
            return futTrades;
        }

        public List<SavedClientOrder> getBDCTrades()
        {
            List<SavedClientOrder> bdcTrades = new List<SavedClientOrder>();
            foreach (SavedClientOrder trade in trades)
            {
                //if (StoredProcMgr.MANAGER.isFuture(trade.getSymbol()))
                //if (!StoredProcMgr.MANAGER.isRepo(trade.getSymbol()))
                if (trade.GetSecurityType().Equals(AlgoTrading.Data.SecurityType.BDC))
                {
                    bdcTrades.Add(trade);
                }
            }
            return bdcTrades;
        }

        public void clearTrades()
        {
            trades.Clear();
        }

        /// <summary>
        /// Save this client's trades for the input tradingDays
        /// </summary>
        /// <param name="clientTrades_">client trades from Database</param>
        public void setClientTrades(List<SavedClientOrder> clientTrades_)
        {
            trades.Clear();
            trades = clientTrades_;
        }

        public void addAnExchangeOrder(int seq_, SavedExchangeOrder exchangeOrder_)
        {
            trades[seq_].addOneExchangeorder(exchangeOrder_);
        }
        public void addAnExchangeOrder(int seq_)
        {
            trades[seq_].incExchangeOrderCount();
        }

        public void addClientOrder(SavedClientOrder order_)
        {
            this.trades.Add(order_);
        }

        // Removed for merge order not supported.
        //public bool shouldMergeOrders()
        //{
        //    return mergeOrders;
        //}
        //public void setMergeOrder(bool mergeOrders_)
        //{
        //    mergeOrders = mergeOrders_;
        //}

        /// <summary>
        /// 
        /// </summary>
        /// <returns>One trade of more, is valid</returns>
        public bool hasValidTrade()
        {
            if (trades.Count == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public int getTradeCount()
        {
            return trades.Count;
        }
        /// <summary>
        /// Get who to send the client email
        /// </summary>
        /// <returns>Client email if set to sendToClient; repsentEmail otherwise</returns>
        public string getSendToEmail()
        {
            return sendToClient ? email : repsentEmail;
        }

        /// <summary>
        /// Dose this client have a report today, based on hers ReportFrequency and what day is today
        /// </summary>
        /// <param name="properity_">DateProperty</param>
        /// <returns>True if she has a report today</returns>
        public bool hasOrdinaryReport(DateProperty properity_)
        {
            if (!isValid)
            {
                return false;
            }

            if (frequency == ReportFrequency.NONE)
            {
                return false;
            }

            if (frequency == ReportFrequency.DAILY)
            {
                return true;
            }
            if (frequency == ReportFrequency.WEEKLY)
            {
                if (DateProperty.Last_Of_Week == properity_
                    || DateProperty.Last_Of_Week_Month == properity_)
                {
                    return true;
                }
            }

            if (frequency == ReportFrequency.MONTHLY)
            {
                if (DateProperty.Last_Of_Month == properity_
                    || DateProperty.Last_Of_Week_Month == properity_)
                {
                    return true;
                }
            }

            return false;
        }
        
        /// <summary>
        /// If Client set to WEEKLY report, he doesnot need a week report as those set to Daily Report
        /// </summary>
        /// <returns></returns>
        public bool needWkSummary()
        {
            return frequency == ReportFrequency.DAILY;
        }

        /// <summary>
        /// Get CCs.
        /// </summary>
        /// <returns>CC List</returns>
        public List<string> getCcList()
        {
            return ccList;
        }
    }
}
