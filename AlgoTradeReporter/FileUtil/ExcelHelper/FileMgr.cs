/* ==============================================================================
 * ClassName：ClientOrderMgr
 * Description：
 * Author：JiangTao Li
 * Created Time：2014/12/30 15:17:19
 * Revision Time：
 * Notes：
 * @version 1.0
* ==============================================================================*/


using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using AlgoTradeReporter.Data.Trades;

namespace AlgoTradeReporter.FileUtil
{
    class FileMgr
    {
        private const char FOLDER_SPLITER = '\\';

        private List<string> tradingDays;
        private string orderFolderName;
        private OrderLoader loader;
        private OrderParser parser;

       /*
        * Map of instanceIds and their corrsponding orderFiles;
        * */
        private Dictionary<string, List<FileInfo>> orderFiles;

        private List<string> excelFiles;

        public FileMgr()
        {
            this.parser = new OrderParser();
            this.loader = new OrderLoader();
            this.orderFiles = new Dictionary<string, List<FileInfo>>();
            this.tradingDays = new List<string>();
            this.excelFiles = new List<string>();
        }

        public void init(List<string> logRoots_, string orderFolderName_)
        {            
            orderFolderName = orderFolderName_;
            loader.setEngineFolders(logRoots_);
        }

        public void setTradingDay(string tradingDay_)
        {
            this.tradingDays.Add(tradingDay_);
        }

        public void clear()
        {
            tradingDays.Clear();
            orderFiles.Clear();
        }

        /// <summary>
        /// Get all instance Ids, say 10.101.237.141-9999
        /// </summary>
        /// <returns>Instance Ids</returns>
        public Dictionary<string, List<FileInfo>>.KeyCollection getIntanceIds()
        {
            return this.orderFiles.Keys;
        }

        /// <summary>
        /// First Get Instance 'tradingDay' folder, then load its 'order' folder.
        /// </summary>
        public void parseClientOrderFileAddress()
        {
            Console.Out.WriteLine("Analyzing Client Order Address for Date " + tradingDays[0]);
            orderFiles = loader.getEngineOrders(tradingDays, orderFolderName);

            int clientOrderCount = 0;
            foreach (var item in this.orderFiles)
            {
                clientOrderCount += item.Value.Count;
            }
            Console.Out.WriteLine("{0} engines found, {1} client orders", orderFiles.Count, clientOrderCount);
        }

        /// <summary>
        /// Get all trades of an instance
        /// </summary>
        /// <param name="instanceId_">Instance Id</param>
        /// <returns>Orders</returns>
        public List<Order> getInstanceOrders(string instanceId_)
        {
            return parser.recoverClientOrders(orderFiles[instanceId_]);
        }

        public void deleteFile()
        {
            foreach(string excel in excelFiles)
            {
                File.Delete(excel);
            }
        }

        public void recordAnExcel(string fileDir_)
        {
            excelFiles.Add(fileDir_);
        }
    }
}
