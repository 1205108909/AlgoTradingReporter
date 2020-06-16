/* ==============================================================================
 * ClassName：SavedExchangeOrder
 * Description：
 * Author：zhaoyu
 * Created Time：2018/2/2 9:06:28
 * Revision Time：
 * Notes：
 * @version 1.0
* ==============================================================================*/


using AlgoTrading.Engine.AlgoDecision;
using AlgoTrading.Engine.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AlgoTradeReporter.Data.Trades
{
    class SavedExchangeOrder
    {
        private string orderId;
        private string sliceId;
        private decimal price;
        private decimal quantity;
        private decimal cumQty;
        private decimal leavesQty;
        private OrderStatus status;
        private OrderType type;
        private decimal sliceAvgPrice;
        private DecisionType decision;
        private PlacementCategory category;
        private string effectiveTime;
        private string expireTime;

        public SavedExchangeOrder(
            string orderId_,
            string sliceId_,
            decimal price_,
            decimal quantity_,
            decimal cumQty_,
            decimal leavesQty_,
            int status_,
            int type_,
            decimal sliceAvgPrice_,
            int decision_,
            int category_,
            string effectiveTime_,
            string expireTime_
            )
        {
            this.orderId = orderId_;
            this.sliceId = sliceId_;
            this.price = price_;
            this.quantity = quantity_;
            this.cumQty = cumQty_;
            this.leavesQty = leavesQty_;
            this.status = (OrderStatus)status_;
            this.type = (OrderType)type_;
            this.sliceAvgPrice = sliceAvgPrice_;
            this.decision = (DecisionType)decision_;
            this.category = (PlacementCategory)category_;
            this.effectiveTime = effectiveTime_;
            this.expireTime = expireTime_;
        }

        public string getOrderId()
        {
            return orderId;
        }
        public string getSliceId()
        {
            return sliceId;
        }
        public decimal getPrice()
        {
            return price;
        }
        public decimal getQuantity()
        {
            return quantity;
        }
        public decimal getCumQty()
        {
            return cumQty;
        }
        public decimal getLeavesQty()
        {
            return leavesQty;
        }
        public string getStatus()
        {
            return status.ToString();
        }
        public string getType()
        {
            return type.ToString();
        }
        public decimal getAvgPrice()
        {
            return sliceAvgPrice;
        }
        public string getDecision()
        {
            return decision.ToString();
        }
        public string getCategory()
        {
            return this.category.ToString();
        }
        public string getEffectiveTime()
        {
            return this.effectiveTime;
        }
        public string getExpireTime()
        {
            return this.expireTime;
        }
    }
}
