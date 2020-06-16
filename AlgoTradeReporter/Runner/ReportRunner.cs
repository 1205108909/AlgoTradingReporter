using AlgoTradeReporter.Data;
using AlgoTradeReporter.Data.ClientInfo;
using AlgoTradeReporter.Data.Commons;
using AlgoTradeReporter.Data.Trades;
using AlgoTradeReporter.Email;
using AlgoTradeReporter.FileUtil;
using AlgoTradeReporter.FileUtil.ExcelHelper.Revised;
using AlgoTradeReporter.StoredProc;
using AlgoTradeReporter.Util;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AlgoTradeReporter
{
    class ReportRunner
    {
        private static ILog logger = log4net.LogManager.GetLogger(typeof(ReportRunner));
        private const ReportFrequency WEEK_REPORT_FREQ = ReportFrequency.WEEKLY;

        private FileMgr fileMgr;
        private RunnerParas paras;
        private ClientExcel excelHelper;
        private ManagerExcel mgrExcelHelper;
        private List<Client> clients;

        public ReportRunner()
        {
            excelHelper = new ClientExcel();
            mgrExcelHelper = new ManagerExcel();
            paras = new RunnerParas();
            fileMgr = new FileMgr();
            clients = new List<Client>();
        }

        public void init(RunnerParas paras_)
        {
            paras = paras_;
            fileMgr.init(paras.getLogRoots(), paras.getFileConfig().getOrderFileName());
            loadClients();
        }

        private void loadClients()
        {
            if (paras.getAccounts().Count == 0)
            {
                clients.AddRange(StoredProcMgr.MANAGER.getClients());
            }
            else
            {
                foreach (string account in paras.getAccounts())
                {
                    Client client = StoredProcMgr.MANAGER.getClientByAcct(account);
                    if (client == null)
                    {
                        continue;
                    }
                    clients.Add(client);
                }
            }

            if (0 != paras.getToList().Count)
            {
                string to = "";
                List<string> receivers = paras.getToList();
                foreach(string receiver in receivers)
                {
                    to = to + receiver + ";";
                }
                foreach(Client client in clients)
                {
                    client.setEmail(to);
                    client.setRepresentEmail(to);
                }
            }
        }

        /// <summary>
        /// Write Client Order and Exchange Slices to database.
        /// </summary>
        public void writeOrderToDb()
        {
            foreach (string day in paras.getTradingDays())
            {
                fileMgr.setTradingDay(day);
                fileMgr.parseClientOrderFileAddress();
                ReportSenderMgr.SENDER.getExecReportSender().addDateToMail(day);

                foreach (string instanceId in fileMgr.getIntanceIds())
                {
                    Console.Out.Write("Handling Instance " + instanceId + " ");
                    logger.Info("Handling Instance " + instanceId + " of date " + day);
                    List<Order> instanceOrders = fileMgr.getInstanceOrders(instanceId);
                    foreach (Order order in instanceOrders)
                    {
                        order.computeMarketVariables();
                        order.validateOrderParas();
                    }

                    UpdatedCount updatedCount = StoredProcMgr.MANAGER.insUpdTrades(instanceOrders, instanceId);
                    ReportSenderMgr.SENDER.getExecReportSender().addInstanceToMail(
                        instanceId, updatedCount.getClientOrderCount(), updatedCount.getExchangeOrderCount());
                    Console.Out.WriteLine(updatedCount.getClientOrderCount() + " " + updatedCount.getExchangeOrderCount());
                }
                fileMgr.clear();
                Console.Out.WriteLine();
            }
        }

        /// <summary>
        /// Send Daily/Weekly Report
        /// </summary>
        public void sendScheduledReport()
        {
            // loop over all clients, for ordinary report, none, daily, weekly, monthly.
            string targetDay = paras.getTradingDays().ElementAt(paras.getTradingDays().Count - 1);
            DateProperty dateProperity = DateTimeUtil.getDateProperty(targetDay, StoredProcMgr.MANAGER.getTradingDays());

            foreach (Client client in StoredProcMgr.MANAGER.getClients())
            {
                if (!client.hasOrdinaryReport(dateProperity))
                {
                    continue;
                }
                prepareSendReport(client, targetDay, client.getReportFrequency());
            }

            // Send an summary report when this day is the end of a week.
            if (DateProperty.Last_Of_Week == dateProperity || DateProperty.Last_Of_Week_Month == dateProperity)
            {
                String msg = "End of Week, Generating Weekly report for clients.";
                Console.Out.WriteLine(msg);
                logger.Info(msg);

                foreach (Client client in StoredProcMgr.MANAGER.getClients())
                {
                    if (!client.needWkSummary())
                    {
                        continue;
                    }
                    prepareSendReport(client, targetDay, WEEK_REPORT_FREQ);
                }
            }
        }

        /// <summary>
        /// Handle the case when only client is need for some
        /// specific clients.
        /// </summary>
        public void sendSpecifiedClientReport()
        {
            StoredProcMgr.MANAGER.updateTmpTradingDay(paras.getTradingDays());

            foreach (Client client in clients)
            {
                if (null != client)
                {
                    prepareClientTrades(client);
                    //bool hasTrades = excelHelper.hasTrade(client);
                    bool hasTrades = client.hasValidTrade();
                    if (!hasTrades)
                    {
                        string msg = client.getAccountId() + " has no trade for the input days";
                        ReportSenderMgr.SENDER.getExecReportSender().addMessage(msg);
                        continue;
                    }

                    doReport(client, paras.getTradingDays(), ReportFrequency.NONE);
                }
                else
                {
                    string msg = client.getAccountId() + " matched no account from DataBase, will skip";
                    ReportSenderMgr.SENDER.getExecReportSender().addMessage(msg);
                    logger.Error(msg);
                    continue;
                }
            }
        }


        public void sendManagerReport()
        {
            StoredProcMgr.MANAGER.updateTmpTradingDay(paras.getTradingDays());
            mgrExcelHelper.before(paras.getTradingDays());

            foreach (Client client in clients)
            {
                prepareClientTrades(client);
                if (client.hasValidTrade())
                {
                    mgrExcelHelper.createWorkSheet(client);
                }
                //client.setMergeOrder(false);                
            }
            string fileDir = mgrExcelHelper.generateSummaryTab(paras.getTradingDays());
            fileMgr.recordAnExcel(fileDir);
            fileMgr.recordAnExcel(fileDir + ".rar");

            ReportSenderMgr.SENDER.sendManagerReport(paras, clients, fileDir);
        }


        /// <summary>
        /// Prepare and send report
        /// </summary>
        /// <param name="client_">The client</param>
        /// <param name="date_">tradingDay</param>
        /// <param name="freq_">Report frequency</param>
        private void prepareSendReport(Client client_, string date_, ReportFrequency freq_)
        {
            List<string> dateSpan = prepareAndUpdateDate(client_, date_, freq_);
            bool hasTrades = prepareClientTrades(client_);
            logger.Info(client_.getClientName() + " of date " + date_ + " has trades :" + hasTrades);
            if (hasTrades)
            {
                doReport(client_, dateSpan, freq_);
            }
        }

        /// <summary>
        /// Load Client trades from DataBase. TradingDays are set to a #tmpTradingDay table before this function is called.
        /// For every run, all clients trades are of the same tradingDay(s), so set up once is enough.
        /// </summary>
        /// <param name="client_"></param>
        /// <returns>If this client has valid trades of the specific day(s)</returns>
        private bool prepareClientTrades(Client client_)
        {
            //client_.setClientTrades(
            StoredProcMgr.MANAGER.loadAllOrders(client_);
            if (!client_.hasValidTrade())
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Generate and send client report
        /// </summary>
        /// <param name="client_">Client info, containing trades.</param>
        /// <param name="dateSpan_">tradingDays.</param>
        /// <returns>to whom the report is sent. Will log it and write this info to email.</returns>
        private void doReport(Client client_, List<string> dateSpan_, ReportFrequency frequency_)
        {
            excelHelper.before(client_, dateSpan_);
            string fileDir = excelHelper.createWorkSheet(client_);
            fileMgr.recordAnExcel(fileDir);
            string toList = ReportSenderMgr.SENDER.sendClientReport(client_, dateSpan_, fileDir);

            ReportSenderMgr.SENDER.getExecReportSender().addClientToMail(client_, frequency_);
            ReportSenderMgr.SENDER.getExecReportSender().addMessage(toList);
        }

        /// <summary>
        /// Get the interested tradingDay. If today is Friday, and freq is Weekly, the whole week days is returned.
        /// Or, if freq is Daily, only today is returned.
        /// </summary>
        /// <param name="client_"></param>
        /// <param name="date_"></param>
        /// <param name="freq_"></param>
        /// <returns>The interested days.</returns>
        private List<string> prepareAndUpdateDate(Client client_, string date_, ReportFrequency freq_)
        {
            List<string> dateSpan = DateTimeUtil.getDateSpan(date_, freq_, StoredProcMgr.MANAGER.getTradingDays());
            StoredProcMgr.MANAGER.updateTmpTradingDay(freq_, dateSpan);
            return dateSpan;
        }

        public void afterWork()
        {
            if (!paras.keepExcelReport())
            {
                fileMgr.deleteFile();
            }
        }
    }
}
