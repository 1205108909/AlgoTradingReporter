/* ==============================================================================
 * ClassName：SavedClientOrder
 * Description：Saved client order from Database, used to construct client report
 * Author：zhaoyu
 * Created Time：2015/3/3 10:45:10
 * Revision Time：
 * Notes：
 * @version 1.0
 * 
 * Revision Time：2016-12-02
 * Notes：Add multiplier to this class, and default set to -1.
 *        This multiplier is set to a valid number if this instrument is repo or futures or 
 *        anything with a multiplier. This multiplier is used to calculate turnover or use price * volume if 
 *        multiplier is -1.
 *        
 * @version 1.7
 * Revision Time :2016-12-12
 * Notes :Remove Multiplier, which is not used. Turnover is calculated in this class when constructed.
* ==============================================================================*/


using AlgoTradeReporter.StoredProc;
using AlgoTradeReporter.Util;
using AlgoTrading.Engine.Util;
using AlgoTrading.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AlgoTradeReporter.Data.Trades
{
    class SavedClientOrder
    {
        private string acct;
        private string orderId;
        private string symbol;
        private string stockName;

        private MarginType marginType;
        private decimal participationRate;

        private OrderAlgo algo;
        private OrderSide side;
        private decimal avgPrice;
        private decimal slipageInBps;
        private decimal cumQty;
        private string tradingDay;
        private string effectiveTime;
        private string expireTime;

        private int sliceCount;
        private int cancelCount;
        private int sentQty;
        private int filledQty;

        //add by zhaoyu @ 20180625
        private int filledCount;

        // Initilized when construction.
        private decimal turnover = 0;
        
        // The exchangeOrders and its count exchangeOrderCount are not used in this version. For relevent info is read 
        // when reading ClientOrder. Leave here for future use.
        List<SavedExchangeOrder> exchangeOrders;
        private int exchangeOrderCount;
        private SecurityType securityType;


        /// <summary>
        /// Construct an order from Database, used to prepare client report. Turnover is calculate according to symbol type.
        /// </summary>
        /// <param name="acct_">client account</param>
        /// <param name="symbol_">stock symbol</param>
        /// <param name="stockName_">stock name in Chinese</param>
        /// <param name="side_">Direction of the trade</param>
        /// <param name="avgPrice_">Average execution price</param>
        /// <param name="slipageInBps_">Order slipage.</param>
        /// <param name="cumQty_">Order cumQty</param>
        /// <param name="tradingDay_">Order tradingDay</param>
        /// <param name="effectiveTime_">Order start time</param>
        /// <param name="expireTime_">order end time</param>
        /// <param name="securityType">order end time</param>
        public SavedClientOrder(string acct_, string orderId_, string symbol_, string stockName_,
            MarginType marginType_, decimal participationRate_,
            OrderAlgo algo_, OrderSide side_,
            decimal avgPrice_, decimal slipageInBps_, decimal cumQty_, string tradingDay_,
            string effectiveTime_, string expireTime_, int sliceCount_, int cencelCount_, int filledCount_,
            int sentQty_, int filledQty_, SecurityType securityType_)
        {
            this.acct = acct_;
            this.orderId = orderId_;
            this.symbol = symbol_;
            this.stockName = stockName_;

            this.marginType = marginType_;
            this.participationRate = participationRate_;
            this.algo = algo_;
            this.side = side_;
            this.avgPrice = avgPrice_;
            this.slipageInBps = slipageInBps_;
            this.cumQty = cumQty_;
            this.tradingDay = tradingDay_;
            this.effectiveTime = effectiveTime_;
            this.expireTime = expireTime_;
            this.exchangeOrders = new List<SavedExchangeOrder>();
            this.exchangeOrderCount = 0;

            this.sliceCount = sliceCount_;
            this.cancelCount = cencelCount_;
            this.sentQty = sentQty_;
            this.filledQty = filledQty_;
            this.filledCount = filledCount_;
            this.securityType = securityType_;

            //if (StoredProcMgr.MANAGER.isRepo(this.symbol))
            if(securityType.Equals(AlgoTrading.Data.SecurityType.RPO))
            {
                this.turnover = this.cumQty * StoredProcMgr.MANAGER.getMultiplier(this.symbol);
            }
            //else if (StoredProcMgr.MANAGER.isFuture(this.symbol))
            else if (securityType.Equals(AlgoTrading.Data.SecurityType.FTR))
            {
                if (symbol.StartsWith("IF"))
                {
                    this.turnover = this.cumQty * this.avgPrice * 300;
                } else if (symbol.StartsWith("IH"))
                {
                    this.turnover = this.cumQty * this.avgPrice * 300;
                } else if (symbol.StartsWith("IC"))
                {
                    this.turnover = this.cumQty * this.avgPrice * 200;
                }
            }
            else if (securityType.Equals(AlgoTrading.Data.SecurityType.BDC))
            {
                if (symbol.EndsWith("sh"))
                {
                    this.turnover = this.cumQty * this.avgPrice * 10;
                }
                else
                {
                    this.turnover = this.cumQty * this.avgPrice;
                }
            }
            //else if (StoredProcMgr.MANAGER.isEqt(this.symbol))
            else if (securityType.Equals(AlgoTrading.Data.SecurityType.EQA) || securityType.Equals(AlgoTrading.Data.SecurityType.FDO) || securityType.Equals(AlgoTrading.Data.SecurityType.FDC))
            {
                this.turnover = this.cumQty * this.avgPrice;
            }

        }

        public string getOrderId()
        {
            return this.orderId;
        }

        public string getSymbol()
        {
            return this.symbol;
        }

        public string getStockName()
        {
            return this.stockName;
        }

        public MarginType getMarginType()
        {
            return this.marginType;
        }

        public decimal getParticipation()
        {
            return this.participationRate;
        }

        public OrderAlgo getAlgo()
        {
            return this.algo;
        }

        public OrderSide getSide()
        {
            return this.side;
        }

        public string getTradingDay()
        {
            return DateTimeUtil.formatDate(this.tradingDay);
        }

        public decimal getAvgPrice()
        {
            return this.avgPrice;
        }

        public decimal getSlipage()
        {
            return this.slipageInBps;
        }

        public decimal getCumQty()
        {
            return this.cumQty;
        }
        public string getEffectiveTime()
        {
            return this.effectiveTime;
        }
        public string getExpireTime()
        {
            return this.expireTime;
        }

        public int getFilledCount()
        {
            return this.filledCount;
        }
        public void addOneExchangeorder(SavedExchangeOrder exchangeOrder_)
        {
            this.exchangeOrders.Add(exchangeOrder_);
        }

        // increase exchange order count by one.
        public void incExchangeOrderCount()
        {
            this.exchangeOrderCount++;
        }
        public List<SavedExchangeOrder> getExchangeOrders()
        {
            return this.exchangeOrders;
        }
        public int getExchangeOrderCount()
        {
            return this.exchangeOrderCount;
        }

        public int getSliceCount()
        {
            return this.sliceCount;
        }
        public int getCancelCount()
        {
            return this.cancelCount;
        }

        public int getSentQty()
        {
            return this.sentQty;
        }
        public int getFilledQty()
        {
            return this.filledQty;
        }


        public decimal getTurnover()
        {
            return this.turnover;
        }

        public SecurityType GetSecurityType()
        {
            return this.securityType;
        }

        /// <summary>
        /// Orders of same stock name, direction and tradingDay are considered same.
        /// When set to merge order, these orders will be merged.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (null == obj)
            {
                return false;
            }

            SavedClientOrder order = obj as SavedClientOrder;
            if (null == (System.Object)order)
            {
                return false;
            }

            return this.symbol.Equals(order.symbol) && this.side.Equals(order.side) && this.tradingDay.Equals(order.tradingDay);
        }

        public bool Equals(SavedClientOrder order_)
        {
            if (null == (object)order_)
            {
                return false;
            }
            return this.symbol.Equals(order_.symbol) && this.side.Equals(order_.side) && this.tradingDay.Equals(order_.tradingDay);
        }

        public override int GetHashCode()
        {
            return symbol.GetHashCode() ^ side.GetHashCode() ^ tradingDay.GetHashCode();
        }
    }
}
