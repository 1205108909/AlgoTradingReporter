/* ==============================================================================
 * ClassName：ClientExcel
 * Description：
 * Author：zhaoyu
 * Created Time：2018/2/1 11:09:00
 * Revision Time：
 * Notes：
 * @version 1.0
 * 
 * @version 1.7
 * Revision Time : 2016-12-12
 * Notes : Created two tabs for each client if needed. One of Equity, the other of RPO.
 *          Removed hasValidTrade method, which is equvilant to Client.hasValidTrades()
 *         Fixed Weekly report excel filename error, so that filename is like ***_from_to.xls
 *         Cleared after() method, which returns fileDir but not used anymore.
 *          
 *          Use client abbreviation when generating report file name, because the version of .Net Mail Client
 *          has a limit of attachement name length up to 24 chars.
 * 
 * TODO Futures contracts might be needed.
* ==============================================================================*/


using AlgoTradeReporter.Config;
using AlgoTradeReporter.Data;
using AlgoTradeReporter.Data.Trades;
using AlgoTradeReporter.Util;
using NPOI.HSSF.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AlgoTradeReporter.FileUtil.ExcelHelper.Revised
{
    class ClientExcel
    {
        protected string fileDir;
        protected FileStream fs;
        protected HSSFWorkbook wk;
        protected string dailyFolder;
        protected TradeTab tradeTab;
        protected TradeTab repoTab;
        protected TradeTab futTab;
        protected TradeTab bdcTab;

        public ClientExcel()
        {
            string date = DateTimeUtil.getToday2();
            this.dailyFolder = ConfigParser.CONFIG.getRunTimeConfig().getExcelFolder() + date + "\\";
            this.tradeTab = new TradeTab();
            this.repoTab = new TradeTab();
            this.futTab = new TradeTab();
            this.bdcTab = new TradeTab();
        }

        public void before(Client client_, List<string> tradingDays_)
        {
            setFileName(client_.getClientAbbr(), tradingDays_);
            createFile();
        }

        public string createWorkSheet(Client client_)
        {
            List<SavedClientOrder> eqtTrades = client_.getEqtTrades();
            List<SavedClientOrder> repoTrades = client_.getRepoTrades();
            List<SavedClientOrder> futTrades = client_.getFutureTrades();
            List<SavedClientOrder> bdcTrades = client_.getBDCTrades();
            if (eqtTrades.Count > 0)
            {
                tradeTab.createTradeTab(eqtTrades, client_, wk, "EQUITY");
            }
            if (repoTrades.Count > 0)
            {
                repoTab.createTradeTab(repoTrades, client_, wk, "REPO");
            }
            if (futTrades.Count > 0)
            {
                futTab.createTradeTab(futTrades, client_, wk, "FUTURE");
            }
            if (bdcTrades.Count > 0)
            {
                bdcTab.createTradeTab(bdcTrades, client_, wk, "BDC");
            }
            writeToXls();
            return this.fileDir;
        }

        public string after()
        {
            return null;
        }

        protected void setFileName(string clientName_, List<string> tradingDays_)
        {
            if (tradingDays_.Count == 1)
            {
                string date = tradingDays_[0];
                if(clientName_ != null)
                {
                    this.fileDir = dailyFolder + clientName_ + "_" + date + ".xls";
                }
                else
                {
                    this.fileDir = dailyFolder + "Report_" + date + ".xls";
                }
            }
            else
            {
                
                int from = Math.Min(Convert.ToInt32(tradingDays_[0]), Convert.ToInt32(tradingDays_[tradingDays_.Count - 1]));
                int to = Math.Max(Convert.ToInt32(tradingDays_[0]), Convert.ToInt32(tradingDays_[tradingDays_.Count - 1]));

                // Remove year
                string fromDate = Convert.ToString(from).Substring(4);
                string endDate = Convert.ToString(to).Substring(4);
                

                if(clientName_ != null)
                {
                    this.fileDir = dailyFolder + clientName_ + "_" + fromDate + "_" + endDate + ".xls";
                }
                else
                {
                    this.fileDir = dailyFolder + "Report_" + fromDate + "_" + endDate + ".xls";
                }
            }
        }

        protected void createFile()
        {
            if (File.Exists(@fileDir))
            {
                File.Delete(@fileDir);
            }

            using (FileStream tmp = File.OpenWrite(@fileDir))
            {
                wk = new HSSFWorkbook();
                wk.Write(tmp);
                wk = null;
            }
            fs = new FileStream(@fileDir, FileMode.Open, FileAccess.ReadWrite);
            wk = new HSSFWorkbook(fs);
        }

        protected void writeToXls()
        {
            using (FileStream writeFs = new FileStream(@fileDir, FileMode.Open, FileAccess.ReadWrite))
            {
                wk.Write(writeFs);   //向打开的这个xls文件中写入mySheet表并保存。
                wk = null;
            }
            fs.Close();
        }
    }
}
