/* ==============================================================================
 * ClassName：AbstractQueryStoredProc
 * Description：
 * Author：zhaoyu
 * Created Time：2015/5/28 14:20:28
 * Revision Time：
 * Notes：
 * @version 1.0
* ==============================================================================*/


using log4net;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace AlgoTradeReporter.StoredProc.QueryStoredProc
{
    abstract class AbstractQueryStoredProc
    {
        private static ILog logger = log4net.LogManager.GetLogger(typeof(AbstractQueryStoredProc));

        protected string storedProcName;

        public AbstractQueryStoredProc()
        {

        }

        public void getFromDataBase(SqlConnection conn_)
        {
            SqlCommand cmd = new SqlCommand(storedProcName, conn_);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.ExecuteNonQuery();
            SqlDataReader reader = cmd.ExecuteReader();

            parseResult(reader);
            cmd.Dispose();
            reader.Close();
        }

        protected abstract void parseResult(SqlDataReader reader_);
    }
}
