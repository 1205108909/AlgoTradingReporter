/* ==============================================================================
 * ClassName：RunnerParas
 * Description：
 * Author：zhaoyu
 * Created Time：2015/3/2 11:16:42
 * Revision Time：
 * Notes：
 * @version 1.0
* ==============================================================================*/


using AlgoTradeReporter.Config;
using AlgoTradeReporter.StoredProc;
using AlgoTradeReporter.Util;
using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AlgoTradeReporter
{
    class RunnerParas
    {
        private static ILog logger = log4net.LogManager.GetLogger(typeof(RunnerParas));
        private const char DATE_SEPERATOR = ':';
        private const char INPUT_SEPERATOR = ';';
        private const string EMPTY_STRING = "";
        private const int FROM_DATE_INDEX = 0;
        private const int TO_DATE_INDEX = 1;

        private FileConfig fileConfig;
        private RunTimeConfig runtimeConfig;
        

        private List<string> tradingDays;
        private List<string> logRoots;
        private List<string> invalidRoots;
        private List<string> accounts;
        private List<string> toList;
        private List<string> managerReceivers;

        private List<string> unKnownAccounts;

        private Mode mode;
        private bool keepExcel;

        /// <summary>
        /// Parameters of this run. Default mode is regular.
        /// </summary>
        public RunnerParas()
        {
            this.tradingDays = new List<string>();
            this.logRoots = new List<string>();
            this.invalidRoots = new List<string>();
            this.accounts = new List<string>();
            this.toList = new List<string>();
            this.mode = Mode.REGULAR;
            this.managerReceivers = new List<string>();
            this.unKnownAccounts = new List<string>();
        }

        public List<string> getTradingDays()
        {
            return this.tradingDays;
        }

        public List<string> getLogRoots()
        {
            return this.logRoots;
        }

        public List<string> getInvalidRoots()
        {
            return this.invalidRoots;
        }

        public List<string> getAccounts()
        {
            return this.accounts;
        }

        public List<string> getToList()
        {
            return this.toList;
        }

        public FileConfig getFileConfig()
        {
            return this.fileConfig;
        }

        public Mode parseMode()
        {
            return this.mode;
        }

        public List<string> getUnknownAccounts()
        {
            return this.unKnownAccounts;
        }

        public bool keepExcelReport()
        {
            return this.keepExcel;
        }

        /// <summary>
        /// Set runtime parameters from input args.
        /// </summary>
        /// <param name="args_">Command line input.</param>
        public void parseArgs(string[] args_)
        {
            this.fileConfig = ConfigParser.CONFIG.getFileConfig();
            this.runtimeConfig = ConfigParser.CONFIG.getRunTimeConfig();
            this.keepExcel = this.runtimeConfig.isKeepAttach();
            this.setRuntimePara(args_);

            this.logRuntimeParas(this.logRoots, "Engine Log Dirs");
            this.logRuntimeParas(this.tradingDays, "TradingDays");
        }

        /// <summary>
        /// Set up runtime paras.
        /// </summary>
        /// <param name="args_">Command line input.</param>
        private void setRuntimePara(string[] args_)
        {
            Options options = new Options();
            if (CommandLine.Parser.Default.ParseArguments(args_, options))
            {
                string traidingDayFile = options.DateFile;
                string tradingDay = options.TradingDay;
                string accountFile = options.AccountFile;
                string modeStr = options.RunMode;
                string recipient = options.ToList;
                bool logRootFromFile = options.LogFromConfig;

                if (traidingDayFile != null)
                {
                    this.tradingDays = this.readFromFile(traidingDayFile);
                }
                else if (null != tradingDay)
                {
                    if (tradingDay.Contains(DATE_SEPERATOR))
                    {
                        string fromDate = tradingDay.Split(DATE_SEPERATOR)[FROM_DATE_INDEX];
                        string toDate = tradingDay.Split(DATE_SEPERATOR)[TO_DATE_INDEX];

                        List<string> allTradingDays = StoredProcMgr.MANAGER.getTradingDays();
                        int fromIndex = allTradingDays.LastIndexOf(fromDate);
                        int endIndex = allTradingDays.LastIndexOf(toDate);
                        int count = endIndex - fromIndex + 1;
                        
                        this.tradingDays.AddRange(allTradingDays.GetRange(fromIndex, count));
                    }
                    else
                    {
                        tradingDays.Add(tradingDay); //Used as a single input date.
                    }
                }
                else
                {
                    tradingDays.Add(DateTimeUtil.getToday());
                }

                if (accountFile != null)
                {
                    this.accounts = this.readFromFile(accountFile);
                }

                if (recipient != null)
                {
                    this.toList = this.readFromFile(recipient);
                }

                this.mode = this.parseMode(modeStr);
                this.initRoots(logRootFromFile);
            }
            this.sortTradingDays();
            this.printParameters();
            this.unKnownAccounts = StoredProcMgr.MANAGER.getNewClients();
        }

        /// <summary>
        /// Make sure the tradingDays are in ascending order. Hardly needed. Just in case.
        /// </summary>
        private void sortTradingDays()
        {
            List<int> tempDate = this.tradingDays.Select(x => int.Parse(x)).ToList();
            tempDate.Sort();
            this.tradingDays.Clear();
            this.tradingDays = tempDate.Select(x => x.ToString()).ToList();
        }

        /// <summary>
        /// Is the engine logs from config file or should load them from Database.
        /// Fucking wired when looked at this code.
        /// </summary>
        /// <param name="isLogFromConfig_"></param>
        /// <returns></returns>
        // TODO so wired. Remove this function.
        private bool getIsLogRootFromFile(bool isLogFromConfig_)
        {
            if (!isLogFromConfig_)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Set up engine logs dirs from config file or database.
        /// </summary>
        /// <param name="isLogFromConfig_">If true, read log dirs from config file.</param>
        private void initRoots(bool isLogFromConfig_)
        {
            if (this.mode == Mode.REGULAR || this.mode == Mode.SAVER)
            {
                bool isLogFromConfig = this.getIsLogRootFromFile(isLogFromConfig_);
                this.setValidEngineFolderNames(isLogFromConfig);
            }
            else
            {
                return;
            }
        }

        /// <summary>
        /// Read from file, used to read tradingDay, accounts, and toList from file.
        /// </summary>
        /// <param name="file_">Input file.</param>
        /// <returns>Lins of the input file.</returns>
        private List<string> readFromFile(string file_)
        {
            FileStream fs = new FileStream(file_, FileMode.Open, FileAccess.Read);
            StreamReader reader = new StreamReader(fs);
            reader.BaseStream.Seek(0, SeekOrigin.Begin);
            List<string> content = new List<string>();
            string str = reader.ReadLine();
            while (str != null)
            {
                if (str.Length != 0)
                {
                    content.Add(str);
                }
                str = reader.ReadLine();
            }
            reader.Close();
            fs.Close();
            return content;
        }

        private void printParameters()
        {
            Console.Out.WriteLine("Input dates are ");
            int index = 0;
            foreach (string day in tradingDays)
            {
                Console.Out.Write(day + " ");
                if (++index % 8 == 0)
                {
                    Console.Out.WriteLine();
                }
            }
                
            Console.Out.WriteLine();
            if (this.mode == Mode.SAVER || this.mode == Mode.REGULAR)
            {
                Console.Out.WriteLine("Trade log roots dirs are ");
                foreach (string rootDir in this.logRoots)
                {
                    Console.Out.Write(rootDir + Environment.NewLine);
                }
            }
            Console.Out.WriteLine();
        }

        /// <summary>
        /// Set up engine log dirs.
        /// </summary>
        /// <param name="isRootFromConfig_">If log dirs are read from config.</param>
        private void setValidEngineFolderNames(bool isRootFromConfig_)
        {
            if (isRootFromConfig_)
            {
                logger.Info("Log Roots from Config");
                foreach (string rootDir in this.fileConfig.getRootDirs())
                {
                    string[] dirs = System.IO.Directory.GetDirectories(rootDir);
                    foreach (string dir in dirs)
                    {
                        if (System.IO.Directory.Exists(dir))
                        {
                            this.logRoots.Add(dir);
                        }
                        else
                        {
                            this.invalidRoots.Add(dir);
                        }
                    }
                }
            }
            else
            {
                logger.Info("Log Roots from DataBase");
                List<string> engineDirs = StoredProcMgr.MANAGER.getEngineDirs();

                foreach (string dir in engineDirs)
                {
                    if (System.IO.Directory.Exists(dir))
                    {
                        this.logRoots.Add(dir);
                    }
                    else
                    {
                        this.invalidRoots.Add(dir);
                    }
                }

            }
        }

        /// <summary>
        /// Log runtime paras.
        /// </summary>
        /// <param name="paras_">List of paras, say tradingDays, engine list</param>
        /// <param name="name_">Properity of the input</param>
        private void logRuntimeParas(List<string> paras_, string name_)
        {
            logger.Info(name_);
            StringBuilder sb = new StringBuilder();
            int index = 0;
            foreach (string para in paras_)
            {
                sb.Append(para + ";");
                if (++index % 10 == 0)
                {
                    sb.Append(Environment.NewLine);
                }
            }
            logger.Info(sb.ToString());
            sb.Clear();
        }

        private Mode parseMode(string mode_)
        {
            if (null == mode_)
            {
                return Mode.REGULAR;
            }
            return (Mode)System.Enum.Parse(typeof(Mode), mode_.ToUpper());
        }
    }
}
