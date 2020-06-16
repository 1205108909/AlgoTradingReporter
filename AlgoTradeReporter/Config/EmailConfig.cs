using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgoTradeReporter.Config
{
    class EmailConfig
    {
        private static char RECEIVER_SPLITER = ';';

        private string sender;
        private string senderName;
        private string password;
        private string smtpServer;
        private int smtpPort;
        private bool isSslEnabled;
        private List<string> recipient;
        private List<string> ccList;
        private List<String> managerReportReceivers;


        /// <summary>
        /// Construct EmailConfig
        /// </summary>
        /// <param name="sender_">Sender email address</param>
        /// <param name="senderName_">Sender Name appears in the mail</param>
        /// <param name="receiver_"></param>
        /// <param name="ccList_"></param>
        /// <param name="password_"></param>
        /// <param name="smtpServer_"></param>
        /// <param name="smtpPort_"></param>
        /// <param name="isSslEnabled_"></param>
        public EmailConfig(string sender_, string senderName_, string receiver_, string ccList_, string managerReceivers_, string password_,
            string smtpServer_, int smtpPort_, bool isSslEnabled_)
        {
            this.sender = sender_;
            this.senderName = senderName_;
            this.password = password_;
            this.smtpServer = smtpServer_;
            this.smtpPort = smtpPort_;
            this.isSslEnabled = isSslEnabled_;

            this.recipient = this.parseStringList(receiver_);
            this.ccList = this.parseStringList(ccList_);
            this.managerReportReceivers = this.parseStringList(managerReceivers_);
        }

        /// <summary>
        /// Parse Receivers
        /// </summary>
        /// <param name="input_">TO List or CC List, seperated by ';'</param>
        /// <returns></returns>
        private List<string> parseStringList(string input_)
        {
            string[] values = input_.Split(RECEIVER_SPLITER);
            List<string> valueList = new List<string>();
            foreach (string str in values)
            {
                if (string.IsNullOrWhiteSpace(str))
                    continue;
                valueList.Add(str);
            }
            return valueList;
        }

        public List<string> getReceivers()
        {
            return this.recipient;
        }

        public List<string> getManagerReceivers()
        {
            return this.managerReportReceivers;
        }


        public List<string> getCcList()
        {
            return this.ccList;
        }

        public int getPort()
        {
            return this.smtpPort;
        }

        public string getEmailServer()
        {
            return this.smtpServer;
        }

        public string getSender()
        {
            return this.sender;
        }

        public string getSenderName()
        {
            return this.senderName;
        }

        public string getPassword()
        {
            return this.password;
        }

        public bool getIsSSL()
        {
            return this.isSslEnabled;
        }
    }
}
