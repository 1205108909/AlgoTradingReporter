using AlgoTradeReporter.Data.ClientInfo;
using AlgoTradeReporter.Data.Commons;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgoTradeReporter.Util
{
    class DateTimeUtil
    {
        private static GregorianCalendar gc = new GregorianCalendar();

        public static string getToday()
        {
            return DateTime.Now.ToString("yyyyMMdd");
        }

        public static string getToday2()
        {
            return DateTime.Now.ToString("yyyy-MM-dd");
        }

        public static string formatDate(string date_)
        {
            return Convert.ToDateTime(date_).ToString("yyyy-MM-dd");
        }

        // FIXME Better in another seperate class?
        public static List<string> getDateSpan(string anchor_, ReportFrequency freq_, List<string> tradingDays_)
        {
            List<string> dateSpan = new List<string>();

            if (ReportFrequency.NONE == freq_)
            {
                return dateSpan;
            }
            else if (ReportFrequency.DAILY == freq_)
            {
                dateSpan.Add(anchor_);
                return dateSpan;
            }
            else
            {
                return getDateRange(anchor_, freq_, tradingDays_);
            }
        }

        private static List<string> getDateRange(string anchor_, ReportFrequency freq_, List<string> tradingDays_)
        {
            List<string> weekDays = new List<string>();
            int idx = tradingDays_.IndexOf(anchor_);
            string targetDay = null;
            for (int i = idx; i >= 0; i--)
            {
                targetDay = tradingDays_[i];
                if (isInSameRange(anchor_, targetDay, freq_))
                {
                    weekDays.Add(targetDay);
                }
                else
                {
                    break;
                }
            }
            return weekDays;
        }

        private static bool isInSameRange(string anchor_, string target_, ReportFrequency freq_)
        {
            if(ReportFrequency.WEEKLY == freq_)
            {
                return isSameWeek(anchor_, target_);
            }
            else
            {
                return isSameMonth(anchor_, target_);
            }
        }

        public static DateProperty getDateProperty(string today_, List<string> tradingDays_)
        {
            string nextDay_ = tradingDays_[tradingDays_.IndexOf(today_) + 1];

            bool isLastOfWk = isLastOfWeek(today_, nextDay_);
            bool isLastOfMth = isLastOfMonth(today_, nextDay_);

            if (isLastOfWk && isLastOfMth)
                return DateProperty.Last_Of_Week_Month;
            else if (isLastOfWk)
                return DateProperty.Last_Of_Week;
            else if (isLastOfMth)
                return DateProperty.Last_Of_Month;
            else
                return DateProperty.Ordinary;
        }

        private static DateTime getDateTime(string date_)
        {
            return DateTime.ParseExact(date_, "yyyyMMdd", System.Globalization.CultureInfo.CurrentCulture);
        }

        private static bool isLastOfWeek(string today_, string nextDay_)
        {
            if (getWkOfYear(today_) != getWkOfYear(nextDay_))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private static bool isLastOfMonth(string today_, string nextDay_)
        {
            if (getMthOfYear(today_) != getMthOfYear(nextDay_))
                return true;
            else
                return false;
        }

        private static int getWkOfYear(string date_)
        {
            return gc.GetWeekOfYear(getDateTime(date_), CalendarWeekRule.FirstDay, DayOfWeek.Monday);
        }

        private static int getMthOfYear(string date_)
        {
            return gc.GetMonth(getDateTime(date_));
        }

        private static bool isSameWeek(string date1_, string date2_)
        {
            return getWkOfYear(date1_) == getWkOfYear(date2_);
        }

        private static bool isSameMonth(string date1_, string date2_)
        {
            return getMthOfYear(date1_) == getMthOfYear(date2_);
        }
    }
}
