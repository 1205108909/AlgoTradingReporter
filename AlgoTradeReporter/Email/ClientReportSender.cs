/* ==============================================================================
 * ClassName：ClientReportSender
 * Description：
 * Author：zhaoyu
 * Created Time：2015/1/30 18:41:15
 * Revision Time：
 * Notes：
 * @version 1.0
* ==============================================================================*/


using AlgoTradeReporter.Config;
using AlgoTradeReporter.Data;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;

namespace AlgoTradeReporter.Email
{
    class ClientReportSender : AbstractEmailSender
    {
        private readonly char EMAIL_SPLITER = ';';

        private static ILog logger = log4net.LogManager.GetLogger(typeof(ClientReportSender));
        private string accountId;

        public ClientReportSender() : base()
        {
        }

        public void prepareMail(Client client_, List<string> tradingDays_, string attachment_)
        {
            clearReceiver();
            setAccountId(client_.getAccountId());
            initClientReport(client_, tradingDays_);
            addAttachment(attachment_);
            addReceiver(client_);
        }

        private void setAccountId(string acct_)
        {
            this.accountId = acct_;
        }

        public void initMail(EmailConfig config_)
        {
            initMailClient(config_);
            initSender(config_.getSender(), config_.getSenderName());
        }

        private void initClientReport(Client client_, List<string> days_)
        {
            generateSubject(client_, days_);
            generateBody(client_);
        }

        private void generateSubject(Client client_, List<string> days_)
        {
            subject = "算法交易报告: " + client_.getClientName() + "(" + client_.getAccountId() + ")";
            if (days_.Count > 1)
            {
                int from = Math.Min(Convert.ToInt32(days_[0]), Convert.ToInt32(days_[days_.Count - 1]));
                int to = Math.Max(Convert.ToInt32(days_[0]), Convert.ToInt32(days_[days_.Count - 1]));
                subject += ("_" + from + "_" + to);
            }
            else
            {
                subject += "_" + days_[0];
            }
        }

        private void generateBody(Client client_)
        {
            body = client_.getClientName() + "(" + client_.getAccountId() + ")" + "交易报告，请查收。";
        }

        private void addReceiver(Client client_)
        {
            List<string> receiver = new List<string>();
            string clientMail = client_.getSendToEmail();
            string[] emailAddress = clientMail.Split(EMAIL_SPLITER);

            receiver.Add(emailAddress[0]);
            base.initReceiver(receiver);
            receiver.Clear();
            if (1 != emailAddress.Length)
            {
                for (int i = 1; i < emailAddress.Length; i++)
                {
                    if(!String.IsNullOrWhiteSpace(emailAddress[i]))
                        receiver.Add(emailAddress[i]);
                }
                
            }
            if(receiver.Count != 0)
                base.initCClist(receiver);
        }

        public new string send()
        {
            logger.Info("Send Client Report " + accountId);
            Console.Out.WriteLine("Send Client Report " + accountId);
            base.send();
            return mail.To.ToString() + mail.CC.ToString();
        }

        public void clear()
        {
            mail.Attachments.Clear();
            mailQQ.Attachments.Clear();
            attachment.Dispose();
            base.mail.To.Clear();
            base.mail.CC.Clear();
            mailQQ.To = "";
            mailQQ.Cc = "";
        }
    }
}
