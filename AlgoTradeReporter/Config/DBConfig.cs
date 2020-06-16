/* ==============================================================================
 * ClassName：dbConfig
 * Description：
 * Author：zhaoyu
 * Created Time：2018/2/8 17:21:31
 * Revision Time：
 * Notes：
 * @version 1.0
* ==============================================================================*/


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AlgoTradeReporter.Config
{
    class DBConfig
    {
        private string server;
        private string database;
        private string user;
        private string password;

        /// <summary>
        /// Construct a DataBase config from config file.
        /// </summary>
        /// <param name="server_">server Ip</param>
        /// <param name="database_">DataBase Name</param>
        /// <param name="user_">User Name</param>
        /// <param name="password_">Password</param>
        public DBConfig(string server_, string database_, string user_, string password_)
        {
            this.server = server_;
            this.database = database_;
            this.user = user_;
            this.password = password_;
        }

        public string getServer()
        {
            return this.server;
        }

        public string getDatabase()
        {
            return this.database;
        }

        public string getUser()
        {
            return this.user;
        }

        public string getPassword()
        {
            return this.password;
        }
    }
}
