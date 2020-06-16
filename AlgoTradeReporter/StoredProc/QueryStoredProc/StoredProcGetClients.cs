/* ==============================================================================
 * ClassName：StoredProcGetClients
 * Description：
 * Author：zhaoyu
 * Created Time：2015/2/27 15:59:14
 * Revision Time：
 * Notes：
 * @version 1.0
* ==============================================================================*/


using AlgoTradeReporter.Data;
using AlgoTradeReporter.Data.ClientInfo;
using AlgoTradeReporter.StoredProc.QueryStoredProc;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace AlgoTradeReporter.StoredProc
{
    class StoredProcGetClients : AbstractQueryStoredProc
    {
        private const string STORED_PROC_NAME = "spu_GetClientInfo";

        private const string SENDTOCLIENT = "1";
        private const string ISVALID = "Y";

        private List<Client> clientList;

        public StoredProcGetClients()
        {
            this.clientList = new List<Client>();
            this.storedProcName = STORED_PROC_NAME;
        }

        public List<Client> getClients()
        {
            return this.clientList;
        }


        protected override void parseResult(SqlDataReader reader_)
        {
            while (reader_.Read())
            {
                string accountId = reader_["accountId"].ToString();
                string clientName = reader_["clientName"].ToString();
                string email = reader_["email"].ToString();
                string repsentEmail = reader_["repsentEmail"].ToString();
                bool sendToClient = reader_["sendToClient"].ToString().Equals(ISVALID) ? true : false;
                //bool mergeOrder = reader_["mergeOrder"].ToString().Equals(ISVALID) ? true : false;
                string freq = reader_["reportFrequency"].ToString().ToUpper();
                ReportFrequency frequency = (ReportFrequency)System.Enum.Parse(typeof(ReportFrequency), freq);
                bool isValid = reader_["isValid"].ToString().Equals(ISVALID) ? true : false;
                string clientAbbr = reader_["clientAbbreviation"].ToString();

                clientList.Add(new Client(accountId, clientName, email, repsentEmail, sendToClient, frequency, isValid, clientAbbr));
            }
        }
    }
}
