/* ==============================================================================
 * ClassName：StoredProcExchangeOrder
 * Description：
 * Author：zhaoyu
 * Created Time：2015/1/28 9:41:31
 * Revision Time：
 * Notes：
 * @version 1.0
* ==============================================================================*/


using AlgoTradeReporter.Data;
using AlgoTradeReporter.Data.Trades;
using AlgoTradeReporter.Util;
using AlgoTrading.Engine.AlgoDecision;
using AlgoTrading.Engine.Orders;
using AlgoTrading.Engine.Util;
using log4net;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace AlgoTradeReporter.StoredProc
{
    class StoredProcExchangeOrder : AbstractOrderStoredProc<ExchangeSlice>
    {
        private static ILog logger = log4net.LogManager.GetLogger(typeof(StoredProcExchangeOrder));

        private static int BATCH_COUNT = 2000;
        private static string INS_UPD_STORED_PROC = "spu_InsUpdExchangeOrder";
        private static string READ_ORDER_STORED_PROC = "spu_GetSlicesByOrderId";

        private string currentAccountId;

        public StoredProcExchangeOrder() : base()
        {
            batchCount = BATCH_COUNT;
            insUpdStoredProcName = INS_UPD_STORED_PROC;
            readOrderStoreProcName = READ_ORDER_STORED_PROC;
        }

        public override void processOrder(Order order_)
        {
            currentAccountId = order_.getAccountId();
            foreach (ExchangeSlice slice_ in order_.getOrderHandler().exchangeOrders)
            {
                createSqlCommand(slice_, logger);
            }
            currentAccountId = null;
        }

        public override SqlParameter[] buildParameters(ExchangeSlice slice_)
        {
            SqlParameter[] paras = new SqlParameter[16];

            paras[0] = new SqlParameter("@orderId", slice_.rootClOrdId);
            paras[1] = new SqlParameter("@sliceId", slice_.clOrdId);
            paras[2] = new SqlParameter("@price", slice_.price);
            paras[3] = new SqlParameter("@qty", slice_.orderQty);
            paras[4] = new SqlParameter("@cumQty", slice_.cumQty);
            paras[5] = new SqlParameter("@leavesQty", slice_.leavesQty);
            paras[6] = new SqlParameter("@orderStatus", (int)slice_.orderStatus);
            paras[7] = new SqlParameter("@type", (int)slice_.type);
            paras[8] = new SqlParameter("@sliceAvgPrice", slice_.avgPrice);
            paras[9] = new SqlParameter("@decisionType", (int)slice_.decisionType);
            paras[10] = new SqlParameter("@category", (int)slice_.category);
            paras[11] = new SqlParameter("@effectiveTime", base.getValidTime(slice_.timeInQueue));
            paras[12] = new SqlParameter("@expireTime", base.getValidTime(slice_.closedTime));
            paras[13] = new SqlParameter("@text", slice_.reason);
            paras[14] = new SqlParameter("@lastUpdDt", DateTime.Now);
            paras[15] = new SqlParameter("@lastUpdId", lastUpdId);

            orderCount++;
            logSqlParas(paras, logger);
            
            return paras;
        }

        public override void parseQueryResult(Client client_, SqlDataReader reader_)
        {
            while (reader_.Read())
            {
                int seq = Int32.Parse(reader_["seqNb"].ToString());
                client_.addAnExchangeOrder(seq);

                // May be of further use.
/*
                string orderId = reader_["orderId"].ToString();
                string sliceId = reader_["sliceId"].ToString();
                decimal price = (decimal)reader_["price"];
                decimal quantity = (decimal)reader_["qty"];
                decimal cumQty = (decimal)reader_["cumQty"];
                decimal leavesQty = (decimal)reader_["leavesQty"];
                int status = Int32.Parse(reader_["orderStatus"].ToString());
                int type = Int32.Parse(reader_["type"].ToString());
                decimal sliceAvgPrice = (decimal)reader_["sliceAvgPrice"];
                int decision = Int32.Parse(reader_["decisionType"].ToString());
                int category = Int32.Parse(reader_["category"].ToString()); 
                string effectiveTime = reader_["effectiveTime"].ToString();
                string expireTime = reader_["expireTime"].ToString();

                int seq = Int32.Parse(reader_["seqNb"].ToString()); 
                client_.addAnExchangeOrder(seq,
                    new SavedExchangeOrder(orderId, sliceId, price, quantity, cumQty,
                    leavesQty, status, type, sliceAvgPrice, decision, category, effectiveTime, expireTime)
                    );
 * */
            }
        }

        public override void updateTmpTable(Client client_, SqlConnection conn_, int start_, int count_)
        {
            string sql = "truncate table ";
            SqlCommand clearCmd = new SqlCommand(sql + "#tmpOrderId", conn_);
            clearCmd.ExecuteNonQuery();
            clearCmd.Dispose();

            List<SavedClientOrder> orders = client_.getClientTrades().GetRange(start_, count_);

            cmd = new SqlCommand();
            cmd.Connection = conn_;
            string accountId = client_.getAccountId();

            beginTransaction(conn_);
            sql = "Insert into #tmpOrderId values ('";
            for (int i = 0; i < orders.Count; i++)
            {
                cmd.CommandText = sql + orders[i].getOrderId() + "', '" + i + "')";
                cmd.ExecuteNonQuery();
                logger.Info("Query exchange order of " + accountId + " " + orders[i].getOrderId());
            }
            commit(conn_);
            transaction.Dispose();
            cmd.Dispose();
        }

        public override void readFromDataBase(Client client_, SqlConnection conn_)
        {
            int start = 0;
            int count = 0;
            int step = 100;
            int exchangeOrderCount = client_.getClientTrades().Count;

            if (exchangeOrderCount == 0)
            {
                return;
            }

            while(true)
            {
                count = Math.Min(step, exchangeOrderCount - start);
                updateTmpTable(client_, conn_, start, count);
                query(client_, conn_);

                start += step;
                if(start > exchangeOrderCount)
                {
                    break;
                }
            }
        }
    }
}
