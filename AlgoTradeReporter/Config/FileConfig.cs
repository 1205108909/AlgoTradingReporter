/* ==============================================================================
 * ClassName：fileConfig
 * Description：
 * Author：zhaoyu
 * Created Time：2018/2/9 17:01:22
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
    class FileConfig
    {
        private const char ROOT_DIR_SPLITER = ';';
        private const string EMPTY_STRING = "";

        private List<string> rootDirs;
        private string orderFileName;

        /// <summary>
        /// Construct File Config
        /// </summary>
        /// <param name="rootDirs_">log base dir</param>
        /// <param name="orderFileName_">the folder name that contains the orders log</param>
        /// <param name="keepAttachment_">If the report excel is kept</param>
        public FileConfig(string rootDirs_, string orderFileName_)
        {
            this.rootDirs = new List<string>();
            string[] dirs = rootDirs_.Split(ROOT_DIR_SPLITER);
            foreach (string dir in dirs)
            {
                if (dir.Equals(EMPTY_STRING))
                    continue;
                this.rootDirs.Add(dir);
            }
            this.orderFileName = orderFileName_;
        }

        public string getOrderFileName()
        {
            return this.orderFileName;
        }

        public List<string> getRootDirs()
        {
            return this.rootDirs;
        }

    }
}
