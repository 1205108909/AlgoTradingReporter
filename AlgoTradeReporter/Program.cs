using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

using CommandLine;
using CommandLine.Text;
using CommandLine.Parsing;
using CommandLine.Infrastructure;

using AlgoTrading.Data;
using AlgoTrading.Util;
using AlgoTrading.Builders;
using AlgoTrading.MathMisc;
using AlgoTrading.Admins;
using AlgoTrading;

using System.Data;
using System.Configuration;
using AlgoTradeReporter.FileUtil;
using AlgoTradeReporter.Data.Trades;
using AlgoTradeReporter.Config;
using log4net;
using AlgoTradeReporter.StoredProc;
using AlgoTradeReporter.Email;
using AlgoTradeReporter.Data.ClientInfo;
using AlgoTradeReporter.Data;
using System.Threading;

namespace AlgoTradeReporter
{
    class Program
    {
        private static ILog logger = log4net.LogManager.GetLogger(typeof(Program));

        static void Main(string[] args)
        {
            //string[] ARGS = { "-d", "20190326", "-a", "e:/account_test.csv" }; 
            //string[] ARGS = { "-d", "20191021"  };
            //string[] ARGS = { "-d", "20190208", "-a", "e:/account_test.csv" };
            string[] ARGS = { "-d", "20190718:20191028", "-a", "e:/account_test.csv", "-m", "CLIENT_REPORT" };
            args = ARGS;
            if (args.Contains("-h"))
            {
                Options options = new Options();
                string help = options.GetUsage();
                Console.Out.WriteLine(help);
                logger.Info("Help info printed, will exit");
                return;
            }
            else
            {
                try
                {
                    config(args);
                    ReportController controller = new ReportController();
                    controller.init(args);
                    controller.run();
                }
                catch (Exception e_)
                {
                    logger.Error(e_.Message);
                    logger.Error(e_.StackTrace);
                    Console.WriteLine(e_.StackTrace);
                }
                finally
                {
                    // Release Email SENDER
                    logger.Info("Release Email Resources.");
                    ReportSenderMgr.SENDER.dispose();
                    // Release DataBase connections.
                    logger.Info("Release DataBase Connection.");
                    StoredProcMgr.MANAGER.closeConn();
                    Thread.Sleep(10000);
                }
            }
        }

        private static void config(string[] args_)
        {
            logger.Info("Input Args ");
            string input = null;
            foreach (string para in args_)
            {
                input = input + para + " ";
            }
            logger.Info(input);
            logger.Info(DateTime.Now.ToString());

            Console.Out.WriteLine(DateTime.Now.ToString());
            Options options = new Options();
            string xmlFile = null;
            if (CommandLine.Parser.Default.ParseArguments(args_, options))
            {
                xmlFile = options.ConfigFile;
            }
            ConfigParser.CONFIG.init(xmlFile);
            StoredProcMgr.MANAGER.init(ConfigParser.CONFIG.getDBConfig());
            ReportSenderMgr.SENDER.init(ConfigParser.CONFIG.getEmailConfig());
        }
    }
}
