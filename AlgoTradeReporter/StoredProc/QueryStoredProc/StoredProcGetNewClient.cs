/* ==============================================================================
 * ClassName：StoredProcGetNewClient
 * Description：If a client accountId exists in Exchange/Client order, but not in Clients table,
 *              it is identified as a new client (Not a perfect name anyway).
 *              This class retrive all such accountIds from database, which are composed into the 
 *              execution email. So that we could know it.
 * Author：zhaoyu
 * Created Time：2015/5/28 14:12:45
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
    class StoredProcGetNewClient : AbstractQueryStoredProc
    {
        private const string STORED_PROC_NAME = "spu_GetNewClient";

        private List<string> newClients;

        public StoredProcGetNewClient()
        {
            this.newClients = new List<String>();
            this.storedProcName = STORED_PROC_NAME;
        }

        public List<string> getNewClients()
        {
            return this.newClients;
        }

        protected override void parseResult(SqlDataReader reader_)
        {
            while (reader_.Read())
            {
                string accountId = reader_["accountId"].ToString();
                newClients.Add(accountId);
            }
        }
    }
}
