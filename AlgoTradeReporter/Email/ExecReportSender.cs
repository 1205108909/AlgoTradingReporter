/* ==============================================================================
 * ClassName：ExecReportSender
 * Description：
 * Author：zhaoyu
 * Created Time：2015/1/30 18:40:56
 * Revision Time：
 * Notes：
 * @version 1.0
* ==============================================================================*/


using AlgoTradeReporter.Config;
using AlgoTradeReporter.Data;
using AlgoTradeReporter.Data.ClientInfo;
using AlgoTradeReporter.StoredProc;
using AlgoTradeReporter.Util;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AlgoTradeReporter.Email
{
    class ExecReportSender : AbstractEmailSender
    {
        private static ILog logger = log4net.LogManager.GetLogger(typeof(ExecReportSender));

        public ExecReportSender() : base()
        {
        }

        public void initExecReport(RunnerParas paras_)
        {
            bool hasError = containError(paras_);
            generateSubject(paras_.parseMode(), hasError);
            generateBody(paras_);
        }

        /*
         * Init execution report config; 
         * For this report, only use config file input, dont bother with command line input.
         * 
         * */
        public void initMail(EmailConfig config_)
        {
            initMailClient(config_);

            initSender(config_.getSender(), config_.getSenderName());
            initReceiver(config_.getReceivers());
        }

        private bool containError(RunnerParas paras_)
        {
            if(paras_.getUnknownAccounts().Count != 0 ||
                paras_.getInvalidRoots().Count != 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void generateInvalidParaEmail(Mode mode_)
        {
            bool hasError = true;
            generateSubject(mode_, hasError);
            // The invalid info is added to mail body when validating paras.
        }

        private void generateSubject(Mode mode_, bool containError_)
        {
            if(containError_)
            {
                subject = "(ATTENTION)" + "AlgoTradingReporter Execution Report Generated on " + 
                    DateTimeUtil.getToday() + " - " + mode_.ToString();
            }
            else
            {
                subject = "AlgoTradingReporter Execution Report Generated on " + DateTimeUtil.getToday() + " - " + mode_.ToString();
            }
        }

        private void generateBody(RunnerParas paras_)
        {
            body = "AlgoTradingReporter Execution Report: " + Environment.NewLine;

            List<string> unidentifiedAccounts = StoredProcMgr.MANAGER.getNewClients();
            if (paras_.getUnknownAccounts().Count != 0)
            {
                body += "Unidentified Accounts, possibly new clients." + Environment.NewLine;
                foreach (string account in unidentifiedAccounts)
                {
                    body += account + Environment.NewLine;
                }
            }

            body += (Environment.NewLine + "Log Roots are : " + Environment.NewLine);
            foreach (string root in paras_.getLogRoots())
            {
                body += (root + Environment.NewLine);
            }

            body += Environment.NewLine + "Trading Days are : " + Environment.NewLine;
            foreach (string day in paras_.getTradingDays())
            {
                body += (day + Environment.NewLine);
            }

            if (paras_.getAccounts().Count != 0)
            {
                body += Environment.NewLine + "Accounts are : " + Environment.NewLine;
                foreach (string acct in paras_.getAccounts())
                {
                    body += (acct + Environment.NewLine);
                }
            }

            if (paras_.getInvalidRoots().Count != 0)
            {
                body += "Invalid (Non-Existing) Folders : \n";
                foreach (string root in paras_.getInvalidRoots())
                {
                    body += (root + Environment.NewLine);
                }
            }

            body += Environment.NewLine + "Detailed Execution Report " + Environment.NewLine;
        }

        public void addInstanceToMail(string instance_, int clientOrderNb_, int exchangeOrderNb_)
        {
            body += (instance_ + " ClientOrder " + clientOrderNb_ + " ExchangeOrder " + exchangeOrderNb_);
            body += Environment.NewLine;
        }

        public void addClientToMail(Client client_, ReportFrequency freq_)
        {
            string freq = freq_ == ReportFrequency.NONE ? "" : freq_.ToString();
            body += (freq + " ClientReport " + client_.getClientName() + "("+ client_.getAccountId() +  ") ClientOrder " + client_.getTradeCount() + " ");
            body += Environment.NewLine;
        }

        public void addDateToMail(string tradingDay_)
        {
            body += Environment.NewLine;
            body += ("TradingDay " + tradingDay_);
            body += Environment.NewLine;
        }

        public void addMessage(string msg_)
        {
            body += msg_;
            body += Environment.NewLine;
        }

        public new void send()
        {
            logger.Info("Send Ececution Report.");
            Console.Out.WriteLine("Send Ececution Report.");
            base.send();
        }
    }
}
