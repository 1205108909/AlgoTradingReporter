/* ==============================================================================
 * ClassName：ManagerExcel
 * Description：
 * Author：zhaoyu
 * Created Time：2018/2/1 11:21:14
 * Revision Time：
 * Notes：
 * @version 1.0
 * 
 * @version 1.7
 * Revision Time : 2016-12-12
 * Notes : Add tow tabs for manager if needed. One for Equity and one for Repo.
 *          Remove public new bool hasTrade(Client client_), which is nothing but calls client.hasValidTrade()
* ==============================================================================*/


using AlgoTradeReporter.Data;
using AlgoTradeReporter.Data.Trades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AlgoTradeReporter.FileUtil.ExcelHelper.Revised
{
    class ManagerExcel : ClientExcel
    {
        private ManagerTab marEqtTab;
        private ManagerTab mgrRepoTab;
        private ManagerTab mgrFutTab;
        private ManagerTab mgrBDCTab;
        private List<ClientTradeSummary> tradeStatistics;
        private List<ClientTradeSummary> repoTradeStatistics;
        private List<ClientTradeSummary> FutureTradeStatistics;
        private List<ClientTradeSummary> BDCTradeStatistics;

        public ManagerExcel() : base()
        {
            this.marEqtTab = new ManagerTab();
            this.mgrRepoTab = new ManagerTab();
            this.mgrFutTab = new ManagerTab();
            this.mgrBDCTab = new ManagerTab();
            this.tradeStatistics = new List<ClientTradeSummary>();
            this.repoTradeStatistics = new List<ClientTradeSummary>();
            this.FutureTradeStatistics = new List<ClientTradeSummary>();
            this.BDCTradeStatistics = new List<ClientTradeSummary>();
        }

        public void before(List<string> tradingDays_)
        {
            setFileName(null, tradingDays_);
            createFile();
        }

        public new void createWorkSheet(Client client_)
        {
            List<SavedClientOrder> eqtTrades = client_.getEqtTrades();
            List<SavedClientOrder> repoTrades = client_.getRepoTrades();
            List<SavedClientOrder> futTrades = client_.getFutureTrades();
            List<SavedClientOrder> bdcTrades = client_.getBDCTrades();

            if (eqtTrades.Count > 0)
            {
                String tabName = "Equity_" + client_.getClientName();
                ClientTradeSummary summary = tradeTab.createTradeTab(eqtTrades, client_, wk, tabName);
                this.tradeStatistics.Add(summary);
            }
            if (repoTrades.Count > 0)
            {
                String tabName = "Repo_" + client_.getClientName();
                ClientTradeSummary summary = tradeTab.createTradeTab(repoTrades, client_, wk, tabName);
                this.repoTradeStatistics.Add(summary);
            }
            if (futTrades.Count > 0)
            {
                String tabName = "Future_" + client_.getClientName();
                ClientTradeSummary summary = tradeTab.createTradeTab(futTrades, client_, wk, tabName);
                this.FutureTradeStatistics.Add(summary);
            }
            if (bdcTrades.Count > 0)
            {
                String tabName = "BDC_" + client_.getClientName();
                ClientTradeSummary summary = tradeTab.createTradeTab(bdcTrades, client_, wk, tabName);
                this.BDCTradeStatistics.Add(summary);
            }
        }



        public string generateSummaryTab(List<string> tradingDays_)
        {
            if (repoTradeStatistics.Count > 0)
            {
                marEqtTab.createManagerTab(repoTradeStatistics, tradingDays_, wk, "REPO");
            }

            if (tradeStatistics.Count > 0)
            {
                mgrRepoTab.createManagerTab(tradeStatistics, tradingDays_, wk, "EQUITY");
            }

            if(FutureTradeStatistics.Count > 0)
            {
                mgrFutTab.createManagerTab(FutureTradeStatistics, tradingDays_, wk, "FUTURE");
            }

            if (BDCTradeStatistics.Count > 0)
            {
                mgrBDCTab.createManagerTab(BDCTradeStatistics, tradingDays_, wk, "BDC");
            }

            writeToXls();
            return fileDir;
        }
    }
}
