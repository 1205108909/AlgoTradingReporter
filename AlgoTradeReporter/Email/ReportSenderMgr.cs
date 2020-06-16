/* ==============================================================================
 * ClassName：ReportSenderMgr
 * Description：
 * Author：zhaoyu
 * Created Time：2015/3/2 14:28:06
 * Revision Time：
 * Notes：
 * @version 1.0
 * 
 * @version 1.7
 * Revision Time : 2016-12-21
 * Notes ： Removed Throw exception of sendClientReport when failure. Replace with a message.
 *          In this way, all clients are attempted and messages are saved.
* ==============================================================================*/


using AlgoTradeReporter.Config;
using AlgoTradeReporter.Data;
using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AlgoTradeReporter.Email
{
    class ReportSenderMgr
    {
        private static ILog logger = log4net.LogManager.GetLogger(typeof(ReportSenderMgr));

        public static ReportSenderMgr SENDER = new ReportSenderMgr();
        private ExecReportSender execReportSender;
        private ClientReportSender cldReportSender;
        private ManagerReportSender managerReportSender;

        private ReportSenderMgr()
        {
            this.execReportSender = new ExecReportSender();
            this.cldReportSender = new ClientReportSender();
            this.managerReportSender = new ManagerReportSender();
        }

        /// <summary>
        /// Init the email conponents. Two SENDER is needed, cause when client reported is generated, its corresponding 
        /// execution report also add an record.
        /// </summary>
        /// <param name="config_">Email Config from config file.</param>
        public void init(EmailConfig config_)
        {
            this.execReportSender.initMail(config_);
            this.cldReportSender.initMail(config_);
            this.managerReportSender.initMail(config_);
        }

        public ExecReportSender getExecReportSender()
        {
            return execReportSender;
        }

        public void sendExecutionReport()
        {
            execReportSender.send();
        }

        public string sendClientReport(Client client_, List<string> tradingDays_, string attachment_)
        {
            try
            {
                cldReportSender.prepareMail(client_, tradingDays_, attachment_);
                return cldReportSender.send();
            }
            catch (Exception e_)
            {
                string msg = "Failed to send Report for Client " + client_.getAccountId();
                logger.Error(msg);
                logger.Error(e_.StackTrace);
                execReportSender.addMessage(msg);
                //throw new Exception(msg, e_);
                return "Send mail to " + client_.getAccountId() + " has some error.";
            }
            finally
            {
                cldReportSender.clear();
            }
        }

        public void sendManagerReport(RunnerParas paras_, List<Client> clients_, string attachment_)
        {
            managerReportSender.prepareMail(paras_, clients_, attachment_);
            string to = managerReportSender.send();
            execReportSender.addMessage("Manager Report Sent" + to);
        }

        public void dispose()
        {
            execReportSender.dispose();
            cldReportSender.dispose();
            managerReportSender.dispose();
        }
    }
}
