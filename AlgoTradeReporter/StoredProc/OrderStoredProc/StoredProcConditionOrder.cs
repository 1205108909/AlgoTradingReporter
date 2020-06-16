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
using AlgoTradeReporter.Util;
using AlgoTradeReporter.Data;
using AlgoTradeReporter.Config;

namespace AlgoTradeReporter.StoredProc
{
    class StoredProcConditionOrder : AbstractOrderStoredProc<Order>
    {
        private static ILog logger = log4net.LogManager.GetLogger(typeof(StoredProcConditionOrder));

         private static int BATCH_COUNT = 500;
        private static string INS_UPD_STORED_PROC = "spu_InsUpdConditionOrder";
        //private static string READ_ORDER_STORED_PROC = "spu_GetClientOrderByAccDate";
        
        private string currentInstance;

        public StoredProcConditionOrder() : base()
        {
            batchCount = BATCH_COUNT;
            
            insUpdStoredProcName = INS_UPD_STORED_PROC;
            //readOrderStoreProcName = READ_ORDER_STORED_PROC;
        }

        public void setCurrentInstance(string instance_)
        {
            currentInstance = instance_;
        }


        public override void processOrder(Order order_)
        {
            if (order_.getClientOrder().useConditionControl || order_.getClientOrder().customizedRiskParams)
            {
                createSqlCommand(order_, logger);
            }            
        }

 
        public override SqlParameter[] buildParameters(Order order_)
        {            
            SqlParameter[] paras = new SqlParameter[19];
            AlgoTrading.Util.ClientOrder clientOrder = order_.getClientOrder();

            paras[0] = new SqlParameter("@orderId", order_.getOrderHandler().clOrdId);
            //todo:
            paras[1] = new SqlParameter("@EnableCondition", Convert.ToInt32(clientOrder.useConditionControl));
            paras[2] = new SqlParameter("@ReferenceIndex", clientOrder.referenceIndex);
            paras[3] = new SqlParameter("@TradingCondition", clientOrder.tradingCondition);
            paras[4] = new SqlParameter("@IsAbsolutePx", Convert.ToInt32(clientOrder.useAbsolutePx));
            paras[5] = new SqlParameter("@RefPrice", clientOrder.refPrice);
            paras[6] = new SqlParameter("@PxType", clientOrder.pxType);
            paras[7] = new SqlParameter("@RelativePriceLimitOffset", clientOrder.relativePriceLimitOffset);
            paras[8] = new SqlParameter("@EligibleToTrading", Convert.ToInt32(clientOrder.eligibleToTrading));
            paras[9] = new SqlParameter("@AutoPauseResume", Convert.ToInt32(clientOrder.autoPauseResume));
            paras[10] = new SqlParameter("@BuyOnUpLimit", Convert.ToInt32(clientOrder.buyOnUpLimit));
            paras[11] = new SqlParameter("@SellOnUpLimit", Convert.ToInt32(clientOrder.sellOnUpLimit));
            paras[12] = new SqlParameter("@BuyOnDownLimit", Convert.ToInt32(clientOrder.buyOnDownLimit));
            paras[13] = new SqlParameter("@SellOnDownLimit", Convert.ToInt32(clientOrder.sellOnDownLimit));
            paras[14] = new SqlParameter("@CustomizedMinute", (int)clientOrder.customizedRollingIntervalInSecs);
            paras[15] = new SqlParameter("@CustomizedPctOfPx", clientOrder.customizedPctOfPx);
            paras[16] = new SqlParameter("@CancelRate", clientOrder.cancelRate);
            paras[17] = new SqlParameter("@lastUpdDt", DateTime.Now);
            paras[18] = new SqlParameter("@lastUpdId", lastUpdId);
            orderCount++;
            
            logSqlParas(paras, logger);
            return paras;
        }

        public override void parseQueryResult(Client client_, SqlDataReader reader_) { }

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
            
        }
    }
}
