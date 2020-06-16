/* ==============================================================================
 * ClassName：DateProperty
 * Description：What kind of day is today.
 * Author：zhaoyu
 * Created Time：2015/3/3 16:48:28
 * Revision Time：
 * Notes：
 * @version 1.0
* ==============================================================================*/


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AlgoTradeReporter.Data.Commons
{
    public enum DateProperty
    {
        Ordinary = 0,
        Last_Of_Week = 1,
        Last_Of_Month = 2,
        Last_Of_Week_Month = 3,
    }
}
