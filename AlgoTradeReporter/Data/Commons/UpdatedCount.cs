/* ==============================================================================
 * ClassName：Pair
 * Description：Client Order count, exchange order count are record when updated from log to database.
 * Author：zhaoyu
 * Created Time：2015/2/2 16:25:13
 * Revision Time：
 * Notes：
 * @version 1.0
* ==============================================================================*/


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AlgoTradeReporter.Util
{
    class UpdatedCount
    {
        private int clientOrderCount;
        private int exchangeOrderCount;
        //add by zhaoyu @20181228
        private int conditionOrderCount;

        public UpdatedCount(int clientOrderCount_, int exchangeOrderCount_, int conditionOrderCount_)
        {
            this.clientOrderCount = clientOrderCount_;
            this.exchangeOrderCount = exchangeOrderCount_;
            this.conditionOrderCount = conditionOrderCount_;
        }

        public int getClientOrderCount()
        {
            return this.clientOrderCount;
        }

        public int getExchangeOrderCount()
        {
            return this.exchangeOrderCount;
        }

        public int getConditionOrderCount()
        {
            return this.conditionOrderCount;
        }
    }
}
