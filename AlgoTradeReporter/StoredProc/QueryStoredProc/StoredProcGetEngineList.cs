/* ==============================================================================
 * ClassName：StoredProcGetEngineList
 * Description：
 * Author：zhaoyu
 * Created Time：2015/2/6 17:18:12
 * Revision Time：
 * Notes：
 * @version 1.0
* ==============================================================================*/


using AlgoTradeReporter.StoredProc.QueryStoredProc;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace AlgoTradeReporter.StoredProc
{
    class StoredProcGetEngineList : AbstractQueryStoredProc
    {
        private const string STORED_PROC_NAME = "spu_GetEngineList";

        private Dictionary<string, int> engineInstanceSeq;
        private List<string> engineDir;

        public StoredProcGetEngineList()
        {
            this.engineDir = new List<string>();
            this.engineInstanceSeq = new Dictionary<string, int>();
            this.storedProcName = STORED_PROC_NAME;
        }

        public List<string> getEngineDirs()
        {
            return this.engineDir;
        }

        public Dictionary<string, int> getEngineInstanceSeq()
        {
            return this.engineInstanceSeq;
        }

        /*
        public void getFromDataBase(SqlConnection conn_)
        {
            SqlCommand cmd = new SqlCommand("spu_GetEngineList", conn_);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.ExecuteNonQuery();
            SqlDataReader reader_ = cmd.ExecuteReader();

            while (reader_.Read())
            {
                string rootDir = reader_["ServerName"].ToString();
                string engineName = reader_["Instance"].ToString();
                int engineIndex = Int32.Parse(reader_["SeqNb"].ToString());
                engineInstanceSeq.Add(engineName, engineIndex);
                engineDir.Add("\\\\" + rootDir + "\\" + engineName);
            }
            cmd.Dispose();
            reader_.Close();
        }
        */


        protected override void parseResult(SqlDataReader reader_)
        {
            while (reader_.Read())
            {
                string rootDir = reader_["ServerName"].ToString();
                string engineName = reader_["Instance"].ToString();
                int engineIndex = Int32.Parse(reader_["SeqNb"].ToString());
                engineInstanceSeq.Add(engineName, engineIndex);
                engineDir.Add("\\\\" + rootDir + "\\" + engineName);
            }
        }
    }
}
