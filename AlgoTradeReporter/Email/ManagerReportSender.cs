/* ==============================================================================
 * ClassName：ManagerReportSender
 * Description：
 * Author：zhaoyu
 * Created Time：2018/2/8 10:19:03
 * Revision Time：
 * Notes：
 * @version 1.0
* ==============================================================================*/


using AlgoTradeReporter.Config;
using AlgoTradeReporter.Data;
using AlgoTradeReporter.Util;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AlgoTradeReporter.Email
{
    class ManagerReportSender : AbstractEmailSender
    {
        private static ILog logger = log4net.LogManager.GetLogger(typeof(ManagerReportSender));

        public ManagerReportSender() : base()
        {
        }

        public void initMail(EmailConfig config_)
        {
            initMailClient(config_);

            initSender(config_.getSender(), config_.getSenderName());
            initReceiver(config_.getManagerReceivers());
            initCClist(config_.getCcList());
        }

        public void prepareMail(RunnerParas paras_, List<Client> clients_, string attachment_)
        {
            // Replace to-list if has 'to' input; otherwise use config file input.
            reinitReceiver(paras_.getToList());
            generateMail(paras_.getTradingDays(), clients_);
            addRarAttachment(attachment_);
        }

        private void reinitReceiver(List<string> toList_)
        {
            int receiverCount = toList_.Count;
            if(receiverCount != 0)
            {
                initReceiver(toList_.GetRange(0, 1));
                if(receiverCount != 1)
                {
                    initCClist(toList_.GetRange(1, receiverCount - 1));
                }
            }
        }

        private void generateMail(List<string> tradingDays_, List<Client> clients_)
        {
            generateSubject();
            generateBody(tradingDays_, clients_);
        }

        private void generateSubject()
        {
            subject = "ManagerReport Generated on " + DateTimeUtil.getToday();
        }

        private void generateBody(List<string> tradingDays_, List<Client> clients_)
        {
            string tmpFirstDay = tradingDays_[0];
            string tmpLastDay = tradingDays_[tradingDays_.Count - 1];

            body = "Aggregated Clients Trade Report : " + Environment.NewLine;
            if(tmpFirstDay.Equals(tmpLastDay))
            {
                body += ("TradingDay " + tmpFirstDay + Environment.NewLine);
            }
            else
            {
                body += ("TradingDays " + tmpFirstDay + " : " + tmpLastDay + Environment.NewLine);
            }

            for (int i = 0; i < clients_.Count; i++)
            {
                body += (clients_[i].getAccountId() + " " + clients_[i].getClientName() + Environment.NewLine);
            }
            body += Environment.NewLine;
        }

        public new string send()
        {
            logger.Info("Send Manager Report.");
            Console.Out.WriteLine("Send Manager Report.");
            base.send();
            return mail.To.ToString() + mail.CC.ToString();
        }
    }
}
