/* ==============================================================================
 * ClassName：ConfigParser
 * Description：
 * Author：zhaoyu
 * Created Time：2018/2/8 17:24:55
 * Revision Time：
 * Notes：
 * @version 1.0
* ==============================================================================*/


using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace AlgoTradeReporter.Config
{
    class ConfigParser
    {
        private static ILog logger = log4net.LogManager.GetLogger(typeof(ConfigParser));

        public static ConfigParser CONFIG = new ConfigParser();

        private XmlDocument xml;
        private DBConfig dbConfig;
        private FileConfig fileConfig;
        private EmailConfig emailConfig;
        private RunTimeConfig runtimeConfig;

        private ConfigParser()
        {
            this.xml = new XmlDocument();
            //this.init(this.xml);
        }

        public void init(string xmlFile_)
        {
            loadXml(xmlFile_);
            initDbConfig();
            initFileConfig();
            initEmailConfig();
            initRpoConfig();
            initRuntimeConfig();
        }


        /// <summary>
        /// Load Xml config file of the input.
        /// If NULL, will load default one.
        /// </summary>
        /// <param name="xmlFile_">xml config file</param>
        private void loadXml(string xmlFile_)
        {
            if (xmlFile_ == null)
            {
                try
                {
                    this.xml.Load(@"Config\configfile.xml");
                }
                catch (Exception ex_)
                {
                    System.Console.WriteLine(@"Config\configfile.xml Not Found");
                    System.Console.WriteLine(@"try ..\Config\configfile.xml");
                    logger.Error(ex_.StackTrace);
                    logger.Error(ex_.Message);
                    logger.Error("..\\..\\Config\\configfile.xml Not Found");
                    logger.Info("try ..\\Config\\configfile.xml");
                    try
                    {
                        this.xml.Load(@"..\Config\configfile.xml");
                    }
                    catch (Exception e_)
                    {
                        logger.Error(e_.StackTrace);
                        System.Console.WriteLine(@"..\Config\configfile.xml Not Found Again, Will Exit");
                        logger.Error("..\\Config\\configfile.xml Not Found Again, Will Exit");
                        throw e_;
                    }
                }
            }
            else
            {
                this.xml.Load(xmlFile_);
            }
        }

        /// <summary>
        /// Return DataBase Config
        /// </summary>
        /// <returns>DataBase Config</returns>
        private void initDbConfig()
        {
            XmlNode fileConfig = xml.SelectSingleNode("/configuration/DBConfig/DBConnection");
            string server = fileConfig.SelectSingleNode("server").InnerText;
            string database = fileConfig.SelectSingleNode("database").InnerText;
            string user = fileConfig.SelectSingleNode("user").InnerText;
            string password = fileConfig.SelectSingleNode("password").InnerText;

            this.dbConfig = new DBConfig(server, database, user, password);
        }

        public DBConfig getDBConfig()
        {
            return this.dbConfig;
        }

        /// <summary>
        /// Return File Config
        /// </summary>
        /// <returns>Work File Config</returns>
        private void initFileConfig()
        {
            XmlNode fileConfig = xml.SelectSingleNode("/configuration/FileConfig");
            string rootFiles = fileConfig.SelectSingleNode("RootFile").InnerText;
            string orderFileName = fileConfig.SelectSingleNode("OrderFileName").InnerText;


            this.fileConfig =  new FileConfig(rootFiles, orderFileName);
        }

        public FileConfig getFileConfig()
        {
            return this.fileConfig;
        }


        /// <summary>
        /// Return Email Config
        /// </summary>
        /// <returns>Email Config</returns>
        private void initEmailConfig()
        {
            XmlNode fileConfig = xml.SelectSingleNode("/configuration/EmailConfig");
            string sender = fileConfig.SelectSingleNode("Sender").InnerText;
            string senderName = fileConfig.SelectSingleNode("SenderName").InnerText;
            string receiver = fileConfig.SelectSingleNode("Receiver").InnerText;
            string ccList = fileConfig.SelectSingleNode("CCList").InnerText;
            string managerReceivers = fileConfig.SelectSingleNode("ManagerReportReceiver").InnerText;
            string password = fileConfig.SelectSingleNode("Password").InnerText;
            string smtpServer = fileConfig.SelectSingleNode("SmtpServer").InnerText;
            int port = Int32.Parse(fileConfig.SelectSingleNode("Port").InnerText);
            bool sslEnabled = Boolean.Parse(fileConfig.SelectSingleNode("SslEnabled").InnerText);

            this.emailConfig = new EmailConfig(sender, senderName, receiver, ccList, managerReceivers, password, smtpServer, port, sslEnabled);
        }

        public EmailConfig getEmailConfig()
        {
            return this.emailConfig;
        }

        public void initRpoConfig()
        {
            XmlNode fileConfig = xml.SelectSingleNode("/configuration/RuntimeConfig");
        }

        public void initRuntimeConfig()
        {
            XmlNode RuntimeConfig = xml.SelectSingleNode("/configuration/RuntimeConfig");
            bool reportZeroQtyOrder = Boolean.Parse(RuntimeConfig.SelectSingleNode("ReportZeroQtyOrder").InnerText);
            string localFolderDir = RuntimeConfig.SelectSingleNode("LocalFileDir").InnerText;
            string attachment = RuntimeConfig.SelectSingleNode("KeepAttachment").InnerText;
            bool keepAttachement = Boolean.Parse(attachment);
            this.runtimeConfig = new RunTimeConfig(reportZeroQtyOrder, localFolderDir, keepAttachement);
        }

        public RunTimeConfig getRunTimeConfig()
        {
            return this.runtimeConfig;
        }
    }
}
