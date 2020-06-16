/* ==============================================================================
 * ClassName：StoredProcTranscations
 * Description：
 * Author：zhaoyu
 * Created Time：2015/1/7 16:38:54
 * Revision Time：
 * Notes：
 * @version 1.0
 * 
 * @version 1.6
 * 2016-10-24
 * Modified parseQueryResult(). If configed to include orders with zeros qty, these orders are added.
 * Else these orders are skipped.
* ==============================================================================*/


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using AlgoTradeReporter.Data.Trades;
using System.Data.SqlClient;
using System.Data;
using log4net;
using AlgoTrading.Engine.Util;
using AlgoTrading.Data;
using AlgoTradeReporter.Util;
using AlgoTradeReporter.Data;
using AlgoTradeReporter.Config;

namespace AlgoTradeReporter.StoredProc
{
    class StoredProcClientOrder : AbstractOrderStoredProc<Order>
    {
        private static ILog logger = log4net.LogManager.GetLogger(typeof(StoredProcClientOrder));

        private const string SHANGHAI = "SS";
        private const string SHENZHEN = "SZ";
        private const string CFFEX = "CFFEX";
        private const string HK = "HK";

        private static int BATCH_COUNT = 500;
        private static string INS_UPD_STORED_PROC = "spu_InsUpdClientOrder";
        private static string READ_ORDER_STORED_PROC = "spu_GetClientOrderByAccDate";
        private Dictionary<string, int> engineList;
        private string currentInstance;

        public StoredProcClientOrder() : base()
        {
            batchCount = BATCH_COUNT;
            engineList = new Dictionary<string, int>();
            insUpdStoredProcName = INS_UPD_STORED_PROC;
            readOrderStoreProcName = READ_ORDER_STORED_PROC;
        }

        public void setCurrentInstance(string instance_)
        {
            currentInstance = instance_;
        }

        public void setEngineList(Dictionary<string, int> engines_)
        {
            engineList = engines_;
        }

        public override void processOrder(Order order_)
        {
            createSqlCommand(order_, logger);
        }

        private int getEngineIndex(string engineName_)
        {
            if (engineList.ContainsKey(engineName_))
                return engineList[engineName_];
            else
                return -1;
        }

        public override SqlParameter[] buildParameters(Order order_)
        {
            SqlParameter[] paras = new SqlParameter[30];
            AlgoTrading.Util.ClientOrder clientOrder = order_.getClientOrder();

            paras[0] = new SqlParameter("@accountId", order_.getAccountId());
            paras[1] = new SqlParameter("@orderId", order_.getOrderHandler().clOrdId);
            paras[2] = new SqlParameter("@instance", getEngineIndex(currentInstance));
            paras[3] = new SqlParameter("@symbol", clientOrder.mdSymbol);
            paras[4] = new SqlParameter("@tradingDay", order_.getTradingDay());
            paras[5] = new SqlParameter("@exDestination", getDestinationValue(clientOrder.exDestination));
            paras[6] = new SqlParameter("@orderStatus", (int)clientOrder.orderStatus);
            paras[7] = new SqlParameter("@orderState", (int)clientOrder.orderState);
            paras[8] = new SqlParameter("@side", (int)clientOrder.side);
            paras[9] = new SqlParameter("@type", (int)clientOrder.type);
            paras[10] = new SqlParameter("@price", clientOrder.price);
            paras[11] = new SqlParameter("@avgPrice", clientOrder.avgPrice);
            paras[12] = new SqlParameter("@slipageInBps", order_.getOrderSlipage());
            paras[13] = new SqlParameter("@algo", (int)clientOrder.orderAlgo);
            paras[14] = new SqlParameter("@effectiveTime", base.getValidTime(clientOrder.effectiveTime));
            paras[15] = new SqlParameter("@expireTime", base.getValidTime(clientOrder.expireTime));
            paras[16] = new SqlParameter("@cumQty", clientOrder.cumQty);
            paras[17] = new SqlParameter("@leavesQty", clientOrder.leavesQty);
            paras[18] = new SqlParameter("@orderQty", clientOrder.orderQty);
            paras[19] = new SqlParameter("@iVWP", order_.getIVWP());
            paras[20] = new SqlParameter("@iVWPVS", order_.getIVWPVS());
            paras[21] = new SqlParameter("@actualPov", order_.getActualPov());
            paras[22] = new SqlParameter("@clientId", clientOrder.clientID);
            paras[23] = new SqlParameter("@pctAdv20", order_.getPctAdv20());
            paras[24] = new SqlParameter("@adv20", order_.getAdv20());
            paras[25] = new SqlParameter("@securityType", (int)clientOrder.securityType);

            // If not set in log, deserialized result is 0; need to set to -1, which means UNKNOWN in engine dll.
            int marginType = (int)clientOrder.marginType != 0 ? (int)clientOrder.marginType : -1;
            paras[26] = new SqlParameter("@marginType", marginType);
            paras[27] = new SqlParameter("@participationRate", clientOrder.participationRate);
            paras[28] = new SqlParameter("@lastUpdDt", DateTime.Now);
            paras[29] = new SqlParameter("@lastUpdId", lastUpdId);

            orderCount++;
            logSqlParas(paras, logger);
            return paras;
        }

        private int getDestinationValue(string destination_)
        {
            if (destination_.Equals(SHANGHAI))
                return 0;
            else if (destination_.Equals(SHENZHEN))
                return 1;
            else if (destination_.Equals(CFFEX))
                return 2;
            else if (destination_.Equals(HK))
                return 3;
            else
                return -1;
        }

        public override void parseQueryResult(Client client_, SqlDataReader reader_)
        {
            while (reader_.Read())
            {
                string acct = reader_["accountId"].ToString();
                string orderId = reader_["orderId"].ToString();
                string symbol = reader_["symbol"].ToString();
                string stockName = reader_["stockName"].ToString();
                MarginType marginType = (MarginType)Int32.Parse(reader_["marginType"].ToString());
                decimal participation = MathUtil.round((decimal)reader_["participationRate"]);
                OrderAlgo algo = (OrderAlgo)Int32.Parse(reader_["algo"].ToString());
                OrderSide side = (OrderSide)Int32.Parse(reader_["side"].ToString());
                decimal avgPrice = MathUtil.round((decimal)reader_["avgPrice"]);
                decimal slipageInBps = MathUtil.round((decimal)reader_["slipageInBps"]);
                decimal cumQty = (decimal)reader_["cumQty"];
                string tradingDay = reader_["tradingDay"].ToString();
                string effectiveTime = reader_["effectiveTime"].ToString();
                string expireTime = reader_["expireTime"].ToString();
                int sliceCount = Int32.Parse(reader_["SliceCount"].ToString());
                int cancelCount = Int32.Parse(reader_["cancelCount"].ToString());
                int sentQty = Int32.Parse(reader_["totalSentQty"].ToString());
                int filledQty = Int32.Parse(reader_["totalFilledQty"].ToString());
                int filledCount = Int32.Parse(reader_["filledCount"].ToString());
                SecurityType securityType = (SecurityType)Int32.Parse(reader_["securityType"].ToString());
                SavedClientOrder order = new SavedClientOrder(acct, orderId, symbol, stockName, marginType, participation,
                    algo, side, avgPrice, slipageInBps, cumQty, tradingDay, effectiveTime, expireTime,
                    sliceCount, cancelCount, filledCount, sentQty, filledQty, securityType);
                
                // If configed to include orders with zero cumQty
                if (ConfigParser.CONFIG.getRunTimeConfig().reportZeroQtyOrders())
                {
                    client_.addClientOrder(order);
                }
                else if (order.getCumQty() > 0)
                {
                    // Only include orders with positive cumQty
                    client_.addClientOrder(order);
                }
            }
        }

        public void updateTmpTradingDay(List<string> tradingDays_, SqlConnection conn_)
        {
            string sql = "truncate table #tmpTradingDay";
            SqlCommand cmd = new SqlCommand(sql, conn_);
            cmd.ExecuteNonQuery();
            cmd.Dispose();

            foreach (string day in tradingDays_)
            {
                sql = "insert into #tmpTradingDay values ('" + day + "')";
                cmd = new SqlCommand(sql, conn_);
                cmd.ExecuteNonQuery();
                cmd.Dispose();
            }
        }

        public override void updateTmpTable(Client client_, SqlConnection conn_, int start_, int count_)
        {
            string sql = "truncate table ";
            SqlCommand cmd = new SqlCommand(sql + "#tmpAccountId", conn_);
            cmd.ExecuteNonQuery();
            cmd.Dispose();

            sql = "insert into #tmpAccountId values ('" + client_.getAccountId() + "', 1)";
            cmd = new SqlCommand(sql, conn_);
            cmd.ExecuteNonQuery();
            cmd.Dispose();
        }

        public override void readFromDataBase(Client client_, SqlConnection conn_)
        {
            updateTmpTable(client_, conn_, 0, 0);
            query(client_, conn_);
        }
    }
}
