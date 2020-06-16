/* ==============================================================================
 * ClassName：AggregatedStatistics
 * Description：
 * Author：zhaoyu
 * Created Time：2018/2/9 14:41:00
 * Revision Time：
 * Notes：
 * @version 1.0
 * 
 * @version 1.7
 * Revision Time :2016-12-12
 * Notes :Change weighted slipage from volume weighted slipage to turnover weighted slipage.
 *          Turnover is obtained from algoStatistics.getTurnover(), which is built from SavedClientOrder.
* ==============================================================================*/


using AlgoTradeReporter.Util;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AlgoTradeReporter.FileUtil.ExcelHelper
{
    
    class AggregatedStatistics
    {
        private static ILog logger = log4net.LogManager.GetLogger(typeof(AggregatedStatistics));

        private decimal turnover;
        private int orderCount;
        private int sliceCount;
        private decimal slipage;
        private decimal slipageStdev;

        private decimal fillRate;
        private decimal cancelRate;
        

        public AggregatedStatistics()
        {
            turnover = 0;
            orderCount = 0;
            sliceCount = 0;
            slipage = 0;
            slipageStdev = 0;
            fillRate = 0;
            cancelRate = 0;
        }

        public void compute(List<ClientTradeSummary> tradeStatistics_)
        {
            computeTurnover(tradeStatistics_);
            computeOrderCount(tradeStatistics_);
            computeSliceCount(tradeStatistics_);
            computeSlipage(tradeStatistics_);
            computeFillRate(tradeStatistics_);
            computeCancelRate(tradeStatistics_);
        }

        private void computeTurnover(List<ClientTradeSummary> tradeStatistics_)
        {
            foreach (ClientTradeSummary statistics in tradeStatistics_)
            {
                foreach (StrategyStatistics algoStatistics in statistics.get().Values)
                {
                    turnover += algoStatistics.getTurnover();
                }
            }
        }

        private void computeOrderCount(List<ClientTradeSummary> tradeStatistics_)
        {
            foreach (ClientTradeSummary statistics in tradeStatistics_)
            {
                foreach (StrategyStatistics algoStatistics in statistics.get().Values)
                {
                    orderCount += algoStatistics.getOrderCount();
                }
            }

        }

        private void computeSliceCount(List<ClientTradeSummary> tradeStatistics_)
        {
            foreach (ClientTradeSummary statistics in tradeStatistics_)
            {
                foreach (StrategyStatistics algoStatistics in statistics.get().Values)
                {
                    sliceCount += algoStatistics.getSliceCount();
                }
            }
        }

        private void computeSlipage(List<ClientTradeSummary> tradeStatistics_)
        {
            List<decimal> slipageCache = new List<decimal>();

            foreach (ClientTradeSummary statistics in tradeStatistics_)
            {
                slipageCache.AddRange(statistics.getSlipages());
                foreach (StrategyStatistics algoStatistics in statistics.get().Values)
                {
                    slipage += algoStatistics.getTurnover() * algoStatistics.getSlipage();
                }
            }
            if(turnover == 0)
            {
                slipage = 0;
            }
            else
            {
                slipage /= turnover;
            }

            try
            {
                if (slipageCache.Count == 0 || slipageCache.Count == 1)
                {
                    slipageStdev = 0;
                }
                else
                {
                    slipageStdev = MathUtil.round(MathUtil.computeStdDev(slipageCache));
                }
            }
            catch (Exception exception)
            {
                logger.Error("Failed to calculate slipage standard devatiation and set to 0. Reason:");
                logger.Error(exception.Message);
                slipageStdev = 0;
            }
        }

        private void computeFillRate(List<ClientTradeSummary> tradeStatistics_)
        {
            decimal sentQty = 0;
            decimal fillQty = 0;

            foreach(ClientTradeSummary statistics in tradeStatistics_)
            {
                sentQty += statistics.getSentQty();
                fillQty += statistics.getFilledQty();
            }
            if (sentQty == 0)
                this.fillRate = 0;
            else
                this.fillRate = fillQty / sentQty;
        }

        public void computeCancelRate(List<ClientTradeSummary> tradeStatistics_)
        {
            decimal cancelCount = 0;
            decimal sliceCount = 0;

            foreach(ClientTradeSummary statistics in tradeStatistics_)
            {
                cancelCount += statistics.getCancelCount();
                sliceCount += statistics.getSliceCount();
            }
            if (sliceCount == 0)
                this.cancelRate = 0;
            else
                this.cancelRate = cancelCount / sliceCount;
        }

        public decimal getTurnover()
        {
            return turnover;
        }
        public int getOrderCount()
        {
            return orderCount;
        }
        public int getSliceCount()
        {
            return sliceCount;
        }
        public decimal getSlipage()
        {
            return slipage;
        }
        public decimal getSlipageStdev()
        {
            return slipageStdev;
        }
        public decimal getFillRate()
        {
            return this.fillRate;
        }
        public decimal getCancelRate()
        {
            return this.cancelRate;
        }

    }
}
