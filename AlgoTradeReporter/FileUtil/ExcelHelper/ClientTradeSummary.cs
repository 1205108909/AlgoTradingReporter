/* ==============================================================================
 * ClassName：ClientTradeSummary
 * Description：
 * Author：zhaoyu
 * Created Time：2015/5/29 11:11:20
 * Revision Time：
 * Notes：
 * @version 1.0
* ==============================================================================*/


using AlgoTradeReporter.Config;
using AlgoTradeReporter.Data;
using AlgoTradeReporter.Data.Trades;
using AlgoTrading.Engine.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AlgoTradeReporter.FileUtil.ExcelHelper
{
    class ClientTradeSummary
    {
        private string accountId;
        private string abbrName;
        private decimal turnover;

        private decimal sentQty;
        private decimal filledQty;
        private decimal sliceCount;
        private decimal cancelCount;

        private decimal fillRate;
        private decimal cancelRate;
        private Dictionary<OrderAlgo, StrategyStatistics> tradeResult;

        // List of slipages of each client order.
        private List<decimal> slipages;

        public string getAccountId()
        {
            return this.accountId;
        }

        public string getAbbrName()
        {
            return this.abbrName;
        }

        public decimal getTotalTurnover()
        {
            return turnover;
        }

        public decimal getFillRate()
        {
            return this.fillRate;
        }

        public decimal getCancelRate()
        {
            return this.cancelRate;
        }

        public decimal getSentQty()
        {
            return this.sentQty;
        }
        public decimal getFilledQty()
        {
            return this.filledQty;
        }
        public decimal getSliceCount()
        {
            return this.sliceCount;
        }
        public decimal getCancelCount()
        {
            return this.cancelCount;
        }
        public List<decimal> getSlipages()
        {
            return this.slipages;
        }


        /// <summary>
        /// Create a client trade summary using her orders.
        /// </summary>
        /// <param name="client_"></param>
        /// <param name="orders"></param>
        public ClientTradeSummary(Client client_, List<SavedClientOrder> orders)
        {
            this.accountId = client_.getAccountId();
            this.abbrName = client_.getClientName();
            this.tradeResult = new Dictionary<OrderAlgo, StrategyStatistics>();
            this.slipages = new List<decimal>();
            
            foreach (OrderAlgo algo in Enum.GetValues(typeof(OrderAlgo)))
            {
                tradeResult.Add(algo, new StrategyStatistics(algo));
            }

            compute(orders);
            foreach (SavedClientOrder order in orders)
            {
                // slipage is only valid for order with some volume.
                if (order.getCumQty() != 0)
                {
                    slipages.Add(order.getSlipage());
                }
            }
        }

        private void compute(List<SavedClientOrder> orders_)
        {
            computeStrategyStatistics(orders_);
            computeTotalTurnover();
            computeFillRate(orders_);
            computeCancelRate(orders_);
        }

        private void computeTotalTurnover()
        {
            turnover = 0;
            foreach (OrderAlgo algo in Enum.GetValues(typeof(OrderAlgo)))
            {
                turnover += tradeResult[algo].getTurnover();
            }
        }

        private void computeStrategyStatistics(List<SavedClientOrder> orders_)
        {
            foreach (SavedClientOrder order in orders_)
            {
                tradeResult[order.getAlgo()].addAnClientOrder(order);
            }
            foreach (OrderAlgo algo in tradeResult.Keys)
            {
                tradeResult[algo].computeSlipage();
            }
        }

        private void computeFillRate(List<SavedClientOrder> orders_)
        {
            foreach(SavedClientOrder order in orders_)
            {
                sentQty += order.getSentQty();
                filledQty += order.getFilledQty();
            }
            if (sentQty == 0)
                this.fillRate = 0;
            else
                this.fillRate = filledQty / sentQty;
        }

        private void computeCancelRate(List<SavedClientOrder> orders_)
        {
            foreach (SavedClientOrder order in orders_)
            {
                //sliceCount += order.getSliceCount(); modify by zhaoyu @ 20180625
                sliceCount += order.getFilledCount();
                cancelCount += order.getCancelCount();
                Console.Out.WriteLine(order.getOrderId() + " CancelCount: " + order.getCancelCount() + " ,FilledCount: " + order.getSliceCount());
            }
            if (sliceCount == 0)
                this.cancelRate = 0;
            else
                this.cancelRate = cancelCount / (sliceCount + cancelCount);
        }

        public StrategyStatistics getByAlgo(OrderAlgo algo_)
        {
            return tradeResult[algo_];
        }

        public Dictionary<OrderAlgo, StrategyStatistics> get()
        {
            return tradeResult;
        }
    }
}
