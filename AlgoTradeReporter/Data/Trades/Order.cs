/* ==============================================================================
 * ClassName：ClientOrder
 * Description：An order from the logs.
 * Author：zhaoyu
 * Created Time：2015/1/5 10:11:52
 * Revision Time：
 * Notes：
 * @version 1.0
* ==============================================================================*/


using AlgoTrading.Data;
using AlgoTradeReporter.Data.Commons;
using AlgoTrading.Engine.Util;
using AlgoTrading.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AlgoTradeReporter.Data.Trades
{
    class Order
    {
        private const char ACCOUNT_SPLITER = '_';
        private const int ACCOUNT_INDEX = 0;
        private const decimal SLIPAGE_SCALOR = 10000;
        private static decimal MAX_SLIPAGE = 10000;

        private readonly AlgoTrading.Util.OrderHandler orderHandler;
        private readonly DateTime tradingDay;

        private decimal orderSlipage;
        private decimal adv20;
        private decimal pctAdv20;
        private decimal ivwp;
        private decimal ivwpvs;
        private decimal actualPov;

        /// <summary>
        /// Construct an order from the logs.
        /// </summary>
        /// <param name="orderHandler_">Order Handler from logs</param>
        /// <param name="tradingDay_">TradingDay of this very trade</param>
        public Order(AlgoTrading.Util.OrderHandler orderHandler_, DateTime tradingDay_)
        {
            orderHandler = orderHandler_;
            tradingDay = tradingDay_;

            orderSlipage = 0;
            adv20 = 0;
            pctAdv20 = 0;
            ivwp = 0;
            ivwpvs = 0;
            actualPov = 0;
        }

        public AlgoTrading.Util.ClientOrder getClientOrder()
        {
            return orderHandler.getClientOrder();
        }

        public AlgoTrading.Util.OrderHandler getOrderHandler()
        {
            return orderHandler;
        }

        /// <summary>
        /// Compute the slipage and other factors of the order.
        /// These factors are not contained in the order, have to be computed.
        /// </summary>
        public void computeMarketVariables()
        {
            IntervalMarketData md = orderHandler.getEvaluationStates<IntervalMarketData>(orderHandler.transactionTime);
            RTMarketData rtmd = orderHandler.getEvaluationStates<RTMarketData>(orderHandler.transactionTime);

            decimal cumQty = orderHandler.getClientOrder().cumQty;
            //if (md.vwp != 0 && md.vwp != decimal.MinusOne && orderHandler.getClientOrder().cumQty > 0)
            if (md.vwp != 0 && md.vwp != decimal.MinusOne)
            {
                orderSlipage = orderHandler.getClientOrder().side == OrderSide.Sell ? (orderHandler.getClientOrder().avgPrice - md.vwp) / md.vwp : (md.vwp - orderHandler.getClientOrder().avgPrice) / md.vwp;
                orderSlipage *= SLIPAGE_SCALOR;
                ivwp = md.vwp;
                ivwpvs = md.vwpvs;
                actualPov = cumQty / ivwpvs;
            }

            if (rtmd.adv20 != 0 && rtmd.adv20 != decimal.MinusOne)
            {
                adv20 = rtmd.adv20;
                pctAdv20 = cumQty / adv20;
            }
            if (Math.Abs(orderSlipage) >= MAX_SLIPAGE)
            {
                orderSlipage = 0;
            }
        }

        public decimal getOrderSlipage()
        {
            return orderSlipage;
        }

        public decimal getAdv20()
        {
            return adv20;
        }

        public decimal getPctAdv20()
        {
            return pctAdv20;
        }

        public decimal getIVWP()
        {
            return ivwp;
        }

        public decimal getIVWPVS()
        {
            return ivwpvs;
        }

        public decimal getActualPov()
        {
            return actualPov;
        }

        /// <summary>
        /// Return accountId
        /// </summary>
        /// <returns>If account == null, parse from secondaryClOrdId; else return account</returns>
        public string getAccountId()
        {
            if (getClientOrder().account == null)
                return parseAccountId(orderHandler.getClientOrder().secondaryClOrdId);
            else
                return getClientOrder().account;
        }

        public DateTime getTradingDay()
        {
            return tradingDay;
        }

        public string getSymbol()
        {
            return orderHandler.getClientOrder().symbol;
        }

        private string parseAccountId(string secondaryClOrdId_)
        {
            string[] strings = secondaryClOrdId_.Split(ACCOUNT_SPLITER);
            return strings[ACCOUNT_INDEX];
        }

        /// <summary>
        /// Set cap and floor for the variables.
        /// </summary>
        public void validateOrderParas()
        {
            actualPov = validateBounds(actualPov, BoundConst.ACTUALPOV);
            pctAdv20 = validateBounds(pctAdv20, BoundConst.PCTADV20);
            orderSlipage = validateBounds(orderSlipage, BoundConst.SLIPAGE);
            adv20 = validateBounds(adv20, BoundConst.ADV20);
        }

        private decimal validateBounds(decimal value_, Bounds bounds_)
        {
            if (value_ < bounds_.lowerBound)
            {
                return bounds_.lowerBound;
            }
            else if (value_ > bounds_.upperBound)
            {
                return bounds_.upperBound;
            }
            else
            {
                return value_;
            }
        }

        /// <summary>
        /// Inner class contains cap and floor.
        /// </summary>
        private class Bounds
        {
            public decimal upperBound;
            public decimal lowerBound;

            public Bounds(decimal lowerBound_, decimal upperBound_)
            {
                upperBound = upperBound_;
                lowerBound = lowerBound_;
            }
        }

        /// <summary>
        /// Inner constants.
        /// </summary>
        private class BoundConst
        {
            public static Bounds ACTUALPOV = new Bounds(0, Convert.ToDecimal(999.999));
            public static Bounds PCTADV20 = new Bounds(0, Convert.ToDecimal(99.999));
            public static Bounds SLIPAGE = new Bounds(-10000, 10000);
            public static Bounds ADV20 = new Bounds(0, 999999999999);
        }
    }
}
