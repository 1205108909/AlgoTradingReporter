/* ==============================================================================
 * ClassName：StrategyStatistics
 * Description：
 * Author：zhaoyu
 * Created Time：2015/5/29 11:14:00
 * Revision Time：
 * Notes：
 * @version 1.0
 * 
 * @version 1.6
 * 2016-10-24
 * Modified addAnClientOrder() method, so that turnover is calculated with consideration of instrument type
 * For Rpos, average price is not used for calculation. A multiplier is introduced to note how much turnover is 
 * for each qty traded.
 * 
 * @version 1.7
 * Revision Time :2016-12-12
 * Notes : Instead of calculating turnover when addAnClientOrder(), slipage is pre-calcualted when constructing SavedClientorder.
 *          And weighted slipage is calculated using turnover as weight.
* ==============================================================================*/


using AlgoTradeReporter.Config;
using AlgoTradeReporter.Data.Trades;
using AlgoTradeReporter.StoredProc;
using AlgoTrading.Engine.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AlgoTradeReporter.FileUtil.ExcelHelper
{
    class StrategyStatistics
    {
        private OrderAlgo algo;
        private decimal turnover;
        private int orderCount;
        private int sliceCount;
        private decimal slipage;     //weighted slipage of the specific strategy of a specific client.

        public StrategyStatistics(OrderAlgo algo_)
        {
            this.algo = algo_;
            this.turnover = 0;
            this.slipage = 0;
            this.orderCount = 0;
            this.sliceCount = 0;
        }

        public void addAnClientOrder(SavedClientOrder order_)
        {
            orderCount++;
            sliceCount += order_.getSliceCount();
            turnover += order_.getTurnover();
            slipage += order_.getSlipage() * order_.getTurnover();

            //string symbol = order_.getSymbol();
            //if (StoredProcMgr.MANAGER.isRepo(symbol))
            //{
            //    int multiplier = StoredProcMgr.MANAGER.getMultiplier(symbol);
            //    turnover += multiplier * order_.getCumQty();
            //}
            //else
            //{
            //    turnover += order_.getCumQty() * order_.getAvgPrice();
            //}
            //slipage += order_.getSlipage() * order_.getCumQty() * order_.getAvgPrice();
        }

        public void computeSlipage()
        {
            if (turnover != 0)
                slipage /= turnover;
            else
                slipage = 0;
        }

        public OrderAlgo getAlgo()
        {
            return this.algo;
        }

        public void setTurnover(decimal turnover_)
        {
            this.turnover = turnover_;
        }
        public decimal getTurnover()
        {
            return this.turnover;
        }

        public void setOrderCount(int orderCount_)
        {
            this.orderCount = orderCount_;
        }
        public int getOrderCount()
        {
            return this.orderCount;
        }

        public void setSliceCount(int sliceCount_)
        {
            this.sliceCount = sliceCount_;
        }
        public int getSliceCount()
        {
            return this.sliceCount;
        }

        public void setSlipage(decimal slipage_)
        {
            this.slipage = slipage_;
        }
        public decimal getSlipage()
        {
            return this.slipage;
        }
    }
}
