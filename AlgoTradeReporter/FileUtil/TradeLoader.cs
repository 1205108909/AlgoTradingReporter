using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;



namespace AlgoTradeReporter.FileUtil
{
    class OrderLoader
    {
        private static ILog logger = log4net.LogManager.GetLogger(typeof(OrderLoader));

        private const string sPattern = "\\d{8}$";
        private const char FOLDER_SPRERATOR = '\\';

        private List<string> engineFolders;

        public OrderLoader()
        {
            this.engineFolders = new List<string>();
        }

        /// <summary>
        /// Set engine log base folders. Say 10.101.237.141/logs/Engine
        /// </summary>
        /// <param name="engineFolders_"></param>
        public void setEngineFolders(List<string> engineFolders_)
        {
            this.engineFolders = engineFolders_;
        }

        /// <summary>
        /// Get instance-orderfile map of the input days. One instance is capable of holding multiple orders.
        /// </summary>
        /// <param name="tradingDays_"></param>
        /// <param name="orderFolderName_">The folder name contains orders, usually 'order'</param>
        /// <returns>Instance-OrderFile map</returns>
        public Dictionary<string, List<FileInfo>> getEngineOrders(List<String> tradingDays_, string orderFolderName_)
        {
            Dictionary<string, List<FileInfo>> orderFiles = new Dictionary<string, List<FileInfo>>();
            foreach (String dir in engineFolders)
            {
                string instance = getInstanceFromEngineFolder(dir);
                List<FileInfo> files = getOrdersFromInstance(dir, tradingDays_, orderFolderName_);
                if (files != null)
                    orderFiles.Add(instance, files);
            }
            return orderFiles;
        }

        /// <summary>
        /// The order root file is traversed to get all trades files.
        /// </summary>
        /// <param name="root_">One Server log, say 10.101.237.141/logs/Engine</param>
        /// <param name="tradingDays_">Specified Days</param>
        /// <param name="orderFolderName_">'order', the very last folder contains orders.</param>
        /// <returns></returns>
        private List<FileInfo> getOrdersFromInstance(string root_, 
            List<String> tradingDays_, string orderFolderName_)
        {
            List<FileInfo> tradeFiles = new List<FileInfo>();
            Stack<string> dirs = new Stack<string>();
            if (!System.IO.Directory.Exists(root_))
            {
                return null;
            }
            dirs.Push(root_);

            while (dirs.Count > 0)
            {
                string currentDir = dirs.Pop();
                string[] subDirs;
                try
                {
                    subDirs = System.IO.Directory.GetDirectories(currentDir);
                }
                catch (UnauthorizedAccessException e)
                {
                    Console.WriteLine(e.Message);
                    logger.Error(e.StackTrace);
                    continue;
                }
                catch (System.IO.DirectoryNotFoundException e)
                {
                    Console.WriteLine(e.Message);
                    logger.Error(e.StackTrace);
                    continue;
                }

                string[] files = null;
                try
                {
                    files = System.IO.Directory.GetFiles(currentDir);
                }
                catch (UnauthorizedAccessException e)
                {

                    Console.WriteLine(e.Message);
                    logger.Error(e.StackTrace);
                    continue;
                }
                catch (System.IO.DirectoryNotFoundException e)
                {
                    Console.WriteLine(e.Message);
                    logger.Error(e.StackTrace);
                    continue;
                }

                tradeFiles.AddRange(getTradeFileDirs(files, currentDir, orderFolderName_));

                // Push the subdirectories onto the stack for traversal.
                // This could also be done before handing the files.
                foreach (string str in subDirs)
                {
                    if (!isFolderSkipped(str, tradingDays_))
                        dirs.Push(str);
                }
            }
            return tradeFiles;
        }
        
        private List<FileInfo> getTradeFileDirs(string[] files_,
            String currentDir_, String orderFolderName_)
        {
            List<FileInfo> tradeFiles = new List<FileInfo>();
            foreach (string file in files_)
            {
                try
                {
                    // Perform whatever action is required in your scenario.
                    System.IO.FileInfo fi = new System.IO.FileInfo(file);
                    if (currentDir_.EndsWith(orderFolderName_))
                    {
                        tradeFiles.Add(new FileInfo(currentDir_ + FOLDER_SPRERATOR + fi.Name));
                    }
                }
                catch (System.IO.FileNotFoundException e)
                {
                    Console.WriteLine(e.Message);
                    logger.Error(e.StackTrace);
                    continue;
                }
            }
            return tradeFiles;
        }


        /*
         * This subfolder is skipped when it's a date folder but the date
         * is not expected.
         */
        private bool isFolderSkipped(string str_, List<string> tradingDays_)
        {
            string dateStr = parseDate(str_);
            if (tradingDays_.Contains(dateStr))
            {
                if (isFolderEndWithDate(str_) || str_.Contains("EngineState"))
                {
                    return false;
                }
            }
            return true;
            //if (isFolderEndWithDate(str_))
            //{
            //    string dateStr = parseDate(str_);
            //    if (tradingDays_.Contains(dateStr))
            //        return false;
            //    else
            //        return true;
            //}
            //if (str_.EndsWith("EngineState"))
            //{
            //    return false;
            //}
            //return true;
        }

        /// <summary>
        /// If the file ends tradingDay
        /// </summary>
        /// <param name="str_"></param>
        /// <returns></returns>
        private bool isFolderEndWithDate(string str_)
        {
            if (System.Text.RegularExpressions.Regex.IsMatch(str_, sPattern))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private string parseDate(string fileStr_)
        {
            System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex("\\d{8}");
            System.Text.RegularExpressions.MatchCollection mc = regex.Matches(fileStr_);
            return mc[mc.Count-1].Value;
        }

        private string getInstanceFromEngineFolder(string engineFolder_)
        {
            string[] folders = engineFolder_.Split(FOLDER_SPRERATOR);
            return folders[folders.Length - 1];
        }
    }
}
