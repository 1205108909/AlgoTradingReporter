/* ==============================================================================
 * ClassName：AbstractStoredProc
 * Description：
 * Author：zhaoyu
 * Created Time：2015/1/29 10:47:28
 * Revision Time：
 * Notes：
 * @version 1.0
* ==============================================================================*/


using AlgoTradeReporter.Data;
using AlgoTradeReporter.Data.Trades;
using log4net;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace AlgoTradeReporter.StoredProc
{
    abstract class AbstractOrderStoredProc <T>
    {
        private static ILog logger = log4net.LogManager.GetLogger(typeof(AbstractOrderStoredProc <T>));
        protected static string lastUpdId = "AlgoService";
        protected int batchCount;
        protected string insUpdStoredProcName;
        protected string readOrderStoreProcName;
        protected SqlCommand cmd;
        private int transCount;
        protected SqlTransaction transaction;

        protected int orderCount;

        public AbstractOrderStoredProc()
        {
            this.orderCount = 0;
        }

        public abstract SqlParameter[] buildParameters(T order_);
        public abstract void processOrder(Order order_);
        public abstract void parseQueryResult(Client client_, SqlDataReader reader_);
        public abstract void updateTmpTable(Client client_, SqlConnection conn_, int start_, int count_);
        public abstract void readFromDataBase(Client client_, SqlConnection conn_);

        protected void logSqlParas(SqlParameter[] paras_, ILog logger_)
        {
            StringBuilder sb = new StringBuilder();
            foreach (SqlParameter para in paras_)
            {                
                if (para != null && para.Value != null)
                    sb.Append(para.Value.ToString() + ";");
            }
            logger_.Info(sb.ToString());
            sb.Clear();
        }

        protected void createSqlCommand(T order_, ILog logger_)
        {
            cmd.Parameters.AddRange(buildParameters(order_));
            cmd.ExecuteNonQuery();
            cmd.Parameters.Clear();
            logger_.Info("sql executed and cleared");
            increaseTransactionCount();
        }

        public int writeToDateBase(SqlConnection conn_, List<Order> orders_)
        {
            orderCount = 0;
            transCount = 0;
            cmd = new SqlCommand(insUpdStoredProcName, conn_);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Connection = conn_;

            beginTransaction(conn_);

            foreach (Order order in orders_)
            {
                processOrder(order);
                if (transCount > batchCount)
                {
                    commit(conn_);
                }
            }
            commit(conn_);

            transaction.Dispose();
            cmd.Dispose();
            return orderCount;
        }

        /*
        public void readFromDataBase(Client client_, SqlConnection conn_)
        {
            SqlCommand cmd = null;
            SqlDataReader reader = null;

            int offset = 0;
            int step = 100;

            updateTmpTable(client_, conn_);


        }
        */

        protected void query(Client client_, SqlConnection conn_)
        {
            SqlCommand queryCmd = new SqlCommand(readOrderStoreProcName, conn_);
            queryCmd.CommandType = CommandType.StoredProcedure;
            queryCmd.ExecuteNonQuery();
            queryCmd.CommandTimeout = 1000;
            SqlDataReader reader = queryCmd.ExecuteReader();

            parseQueryResult(client_, reader);

            queryCmd.Dispose();
            reader.Close();
        }

        public DateTime getValidTime(DateTime original_)
        {
            if (original_.Year < 1900)
            {
                return new DateTime(1900, 1, 1);
            }
            return original_;
        }

        private void increaseTransactionCount()
        {
            transCount++;
        }
        protected void beginTransaction(SqlConnection conn_)
        {
            transaction = conn_.BeginTransaction(insUpdStoredProcName);
            cmd.Transaction = transaction;
        }

        protected void commit(SqlConnection conn_)
        {
            try
            {
                transaction.Commit();
                logger.Info("Transaction Committed");
            }
            catch (Exception ex)
            {
                logger.Error("Transaction Commit Failed");
                logger.Error(ex.StackTrace);
                if (transaction != null)
                {
                    transaction.Rollback();
                }
                Console.WriteLine(ex.Message);
                throw new Exception("Transaction Commit Failed" + ex.Message);
            }
            transCount = 0;
            transaction.Dispose();
            beginTransaction(conn_);
        }
    }
}
