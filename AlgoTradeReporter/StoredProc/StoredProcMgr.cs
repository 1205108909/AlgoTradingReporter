/* ==============================================================================
 * ClassName：StoredProcController
 * Description：
 * Author：zhaoyu
 * Created Time：2015/1/7 15:56:39
 * Revision Time：
 * Notes：
 * @version 1.0
 * 
 * @revision 1.7
 * Revision Time: 2016-12-12
 * Notes : Add StoredProcGetMultiplier for instrument multipliers.
* ==============================================================================*/


using AlgoTradeReporter.Config;
using AlgoTradeReporter.Data;
using AlgoTradeReporter.Data.ClientInfo;
using AlgoTradeReporter.Data.Trades;
using AlgoTradeReporter.StoredProc.QueryStoredProc;
using AlgoTradeReporter.Util;
using log4net;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace AlgoTradeReporter.StoredProc
{
    class StoredProcMgr
    {
        private static ILog logger = log4net.LogManager.GetLogger(typeof(StoredProcMgr));
        private const string ACCT_PREFIX = "0000";

        public static StoredProcMgr MANAGER = new StoredProcMgr();

        private SqlConnection connection;
        private StoredProcClientOrder clientOrderProc;
        private StoredProcExchangeOrder exchangeOrderProc;
        //add by zhaoyu @ 20181228
        private StoredProcConditionOrder conditionOrderProc;
        private StoredProcGetTradingDay tradingDayProc;
        private StoredProcGetEngineList engineListProc;
        private StoredProcGetClients clientsProc;
        private StoredProcGetNewClient newClientProc;
        private StoredProcGetMultiplier multiplierProc;

        private ReportFrequency lastUpdateFreq;

        private StoredProcMgr()
        {
            this.tradingDayProc = new StoredProcGetTradingDay();
            this.clientOrderProc = new StoredProcClientOrder();
            this.exchangeOrderProc = new StoredProcExchangeOrder();
            this.conditionOrderProc = new StoredProcConditionOrder();
            this.engineListProc = new StoredProcGetEngineList();
            this.clientsProc = new StoredProcGetClients();
            this.newClientProc = new StoredProcGetNewClient();
            this.multiplierProc = new StoredProcGetMultiplier();

            this.lastUpdateFreq = ReportFrequency.NONE;
        }

        public void init(DBConfig config_)
        {
            openConn(config_);
            tradingDayProc.getFromDataBase(connection);
            engineListProc.getFromDataBase(connection);

            multiplierProc.getFromDataBase(connection);
            clientOrderProc.setEngineList(engineListProc.getEngineInstanceSeq());
            clientsProc.getFromDataBase(connection);
            newClientProc.getFromDataBase(connection);
            
        }

        public List<string> getTradingDays()
        {
            return tradingDayProc.getTradingDays();
        }

        public List<string> getEngineDirs()
        {
            return engineListProc.getEngineDirs();
        }

        public List<Client> getClients()
        {
            return clientsProc.getClients();
        }

        /// <summary>
        /// Match account read from database, with runtime input account.
        /// The logic is to get a client with either 0000XXXXXXXX or XXXXXXXX for possible input error.
        /// </summary>
        /// <param name="account_">File input account.</param>
        /// <returns>Client, read from database, matched with the input.</returns>
        public Client getClientByAcct(string account_)
        {
            string subsAcct = null;

            if (!account_.StartsWith(ACCT_PREFIX))
            {
                subsAcct = ACCT_PREFIX + account_;
            }
            else
            {
                subsAcct = account_;
            }

            foreach (Client client in StoredProcMgr.MANAGER.getClients())
            {
                if (client.getAccountId().Equals(account_) || client.getAccountId().Equals(subsAcct))
                {
                    return client;
                }
            }
            return null;
        }

        public List<String> getNewClients()
        {
            return newClientProc.getNewClients();
        }

        public void closeConn()
        {
            if (connection != null)
            {
                try
                {
                    connection.Close();
                    connection.Dispose();
                    logger.Info("Connection Closed");
                }
                catch (Exception e_)
                {
                    logger.Error("Failed to Close Connection");
                    logger.Error(e_.Message);
                    logger.Error(e_.StackTrace);
                }
            }
        }

        private void openConn(DBConfig config_)
        {
            logger.Info("Connect DataBase " + config_.getServer() + ";" + config_.getDatabase() + ";");
            if (config_ != null)
            {
                try
                {
                    string sqlConnection = "Server=" + config_.getServer() + ";";
                    sqlConnection = sqlConnection + "Database=" + config_.getDatabase() + ";";
                    sqlConnection = sqlConnection + "uid=" + config_.getUser() + ";";
                    sqlConnection = sqlConnection + "pwd=" + config_.getPassword();
                    connection = new SqlConnection(sqlConnection);
                    connection.Open();
                }
                catch (Exception e_)
                {
                    logger.Error("Failed to Connect Database");
                    logger.Error(e_.StackTrace);
                    throw new Exception("Failed to connect " + config_.getServer() + " " + config_.getDatabase(), e_);
                }
            }
            else
            {
                logger.Error("Failed to Connect Database, No Config File Found");
                throw new Exception("Failed to connect " + config_.getServer() + " " + config_.getDatabase() + " No Config File Found");
            }

            logger.Info("Connection built, Create tmp Table for Trade Query");
            try
            {
                createTmpTable();
                createIdxOnTmpTable();
            }
            catch (Exception e_)
            {
                logger.Error("Failed to Create TmpTable or TmpTableIdx");
                logger.Error(e_.StackTrace);
                throw new Exception("Failed to Create TmpTable or TmpTableIdx", e_);
            }
        }

        /*
         * Tmp Table for query the saved trades in order to generate a trading report. 
         */
        private void createTmpTable()
        {
            string sql = "create table #tmpAccountId (accountId varchar(15), seqNb int)";
            SqlCommand cmd = new SqlCommand(sql, connection);
            cmd.ExecuteNonQuery();
            cmd.Dispose();

            sql = "create table #tmpTradingDay(tradingDay datetime)";
            cmd = new SqlCommand(sql, connection);
            cmd.ExecuteNonQuery();

            sql = "create table #tmpOrderId(orderId varchar(50), seqNb int)";
            cmd = new SqlCommand(sql, connection);
            cmd.ExecuteNonQuery();

            cmd.Dispose();
        }

        private void createIdxOnTmpTable()
        {
            string sql = "create unique clustered index #pk_tmpAccountId on #tmpAccountId(accountId asc)";
            SqlCommand cmd = new SqlCommand(sql, connection);
            cmd.ExecuteNonQuery();
            cmd.Dispose();

            sql = "create unique clustered index #pk_tmpTradingDay on #tmpTradingDay(tradingDay asc)";
            cmd = new SqlCommand(sql, connection);
            cmd.ExecuteNonQuery();

            sql = "create unique clustered index #pk_tmpOrderId on #tmpOrderId(orderId asc)";
            cmd = new SqlCommand(sql, connection);
            cmd.ExecuteNonQuery();
            
            cmd.Dispose();
        }

        /*
         * Set TmpTradingDay whenever the target dates are known.
         * */
        public void updateTmpTradingDay(ReportFrequency freq_, List<string> dateSpan_)
        {
            if (freq_ == lastUpdateFreq)
            {
                return;
            }
            else
            {
                clientOrderProc.updateTmpTradingDay(dateSpan_, connection);
                lastUpdateFreq = freq_;
            }
        }

        public void updateTmpTradingDay(List<string> dateSpan_)
        {
            clientOrderProc.updateTmpTradingDay(dateSpan_, connection);
        }

        /**
         * Read all orders from Database.
         * 
         * Version 1.7
         * Revision Time : 2016-12-19
         * Notes : Removed exchangeOrderProc.readFromDataBase(client_, connection) which is used to read exchange orders 
         *          in order to get its count.
         *          For now, this operation is done when reading ClientOrder, and this operation initializes client order
         *          with slice count.
         *          Leave exchangeOrderProc.readFromDataBase(client_, connection) for possible future use.
         * */
        public void loadAllOrders(Client client_)
        {
            client_.clearTrades();
            clientOrderProc.readFromDataBase(client_, connection);
            //exchangeOrderProc.readFromDataBase(client_, connection);
        }

        private int insUpdClientOrders(List<Order> orders_, string instanceId_)
        {
            logger.Info("start insUpdClientOrders: " + orders_.Count);
            if (orders_.Count == 0)
                return 0;
            clientOrderProc.setCurrentInstance(instanceId_);
            return clientOrderProc.writeToDateBase(connection, orders_);
        }

        private int insUpdExchangeOrders(List<Order> orders_)
        {
            logger.Info("start insUpdExchangeOrders: " + orders_.Count);
            if (orders_.Count == 0)
                return 0;
            return exchangeOrderProc.writeToDateBase(connection, orders_);
        }

        //add by zhaoyu @ 20181228
        private int insUpdConditionOrders(List<Order> orders_)
        {
            logger.Info("start insUpdConditionOrders: " + orders_.Count);
            if (orders_.Count == 0)
                return 0;            
            return conditionOrderProc.writeToDateBase(connection, orders_);
        }
        public UpdatedCount insUpdTrades(List<Order> orders_, string instanceId_)
        {
            logger.Info("start insUpdTrades: " + instanceId_);
            //add by zhaoyu @ 20181228
            int conditionOrderCount = insUpdConditionOrders(orders_);
            int clientOrderCount = insUpdClientOrders(orders_, instanceId_);
            int exchgeOrderCount = insUpdExchangeOrders(orders_);
            
            return new UpdatedCount(clientOrderCount, exchgeOrderCount, conditionOrderCount);
        }

        public int getMultiplier(string symbol)
        {
            return this.multiplierProc.getMultiplier(symbol);
        }        
    }
}
