/* ==============================================================================
 * ClassName：ReportFrequency
 * Description：How Frequency this client need report.
 * Author：zhaoyu
 * Created Time：2015/3/2 16:16:16
 * Revision Time：
 * Notes：
 * @version 1.0
* ==============================================================================*/


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AlgoTradeReporter.Data.ClientInfo
{
    public enum ReportFrequency
    {
        NONE = 0,
        DAILY = 1,
        WEEKLY = 2,
        MONTHLY = 3,
    }
}
