/* ==============================================================================
 * ClassName：OrderParser
 * Description：Get dirs of all engine logs and parse into orders using engine dlls.
 * Author：zhaoyu
 * Created Time：2014/12/30 15:11:58
 * Revision Time：
 * Notes：
 * @version 1.0
* ==============================================================================*/


using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using AlgoTradeReporter.Data.Trades;
using AlgoTradeReporter.Data.Commons;
using log4net;
//using AlgoTrading.Util;

namespace AlgoTradeReporter.FileUtil
{
    class OrderParser
    {
        private static ILog logger = log4net.LogManager.GetLogger(typeof(OrderParser));

        private const char FOLDER_SPLITER = '\\';
        private const char ACCOUNT_SPLITER = '_';
        private const char SYMBOL_EXCHANGE_SPLIT = '.';
        private const int SYMBOL_INDEX = 0;
        private const int EXCHANGE_INDEX = 1;
        private const int ACCOUNT_INDEX = 0;

        private IFormatter formatter;

        public OrderParser()
        {
            this.formatter = new BinaryFormatter();
        }

        /// <summary>
        /// Recover an order given its file dir.
        /// </summary>
        /// <param name="file_">File dir.</param>
        /// <returns>OrderHandler</returns>
        private AlgoTrading.Util.OrderHandler recoverAnOrder(FileInfo file_)
        {
            Object obj = null;
            try
            {
                using (Stream stream = File.Open(file_.FullName, FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    obj = formatter.Deserialize(stream);
                }
            }
            catch (IOException e_)
            {
                Console.WriteLine("Exception in recovering states --- " + e_.Message);
                logger.Error("Exception in recovering states --- " + e_.Message);
                logger.Error(e_.StackTrace);
                return null;
            }
            catch (Exception se_)
            {
                Console.WriteLine(se_.StackTrace);
                Console.WriteLine("Exception in recovering states ---" + se_.Message);
                logger.Error("Exception in recovering states --- " + se_.Message);
                logger.Error(se_.StackTrace);
                return null;
            }
            return (AlgoTrading.Util.OrderHandler)obj;
        }

        /// <summary>
        /// Use the orderHandler to build an order.
        /// </summary>
        /// <param name="orderHandler_">OrderHandler</param>
        /// <returns>Recovered order</returns>
        private Order recoverFromOrderHandler(AlgoTrading.Util.OrderHandler orderHandler_)
        {        
            DateTime tradingDay = getTradingDay(orderHandler_.getClientOrder().effectiveTime);
            return new Order(orderHandler_, tradingDay);
        }

        /// <summary>
        /// Loop over the order files and recover all orders.
        /// </summary>
        /// <param name="files_"></param>
        /// <returns>Orders recovered.</returns>
        public List<Order> recoverClientOrders(List<FileInfo> files_)
        {
            List<Order> orders = new List<Order>();
            foreach (FileInfo file in files_)
            {
                AlgoTrading.Util.OrderHandler orderHandler = recoverAnOrder(file);
                if (orderHandler == null)
                {
                    continue;
                }
                orders.Add(recoverFromOrderHandler(orderHandler));
            }
            // TODO put compute makert variables here
            return orders;
        }

        /// <summary>
        /// Get Trading of the order effective time.
        /// </summary>
        /// <param name="effectiveTime_"></param>
        /// <returns>TradingDay</returns>
        // TODO Put it in the dateUtil class.
        private DateTime getTradingDay(DateTime effectiveTime_)
        {
            return effectiveTime_.Date;
        }
    }
}
