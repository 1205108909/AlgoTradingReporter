using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AlgoTradeReporter.FileUtil.ExcelHelper
{
    class NumberFormat
    {

        public static String asPercent(decimal percentage)
        {
            return String.Format("{0:P2}", percentage);
        }

        public static String asInt(decimal deci)
        {
            return String.Format("{0:N0}", Math.Floor(deci));
        }

        public static String toMoney(decimal money)
        {
            return money.ToString("C"); ;
        }

    }
}
