/* ==============================================================================
 * ClassName：TradeTab
 * Description：
 * Author：zhaoyu
 * Created Time：2018/2/1 10:24:03
 * Revision Time：
 * Notes：
 * @version 1.0
 * 
 * @version 1.7
 * Revision Time: 2016-12-12
 * Notes :Change Colume header to Colume_Headers. Remove old Colume_Name_Merge_Order/Colume_Name_Not_Merge_Order
 *          Always go to not merge order, which means orders of same symbol are listed seperataly. 
 *        Remove old addFootLine and replace with a new one with demanded info.
 *        Remove old createTradeTab method, replace with a new one with new column headers and cell format(color, style and etc.).
* ==============================================================================*/


using AlgoTradeReporter.Data;
using AlgoTradeReporter.Data.Trades;
using AlgoTradeReporter.StoredProc;
using AlgoTradeReporter.Util;
using log4net;
using NPOI.HSSF.UserModel;
using NPOI.HSSF.Util;
using NPOI.SS.UserModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AlgoTradeReporter.FileUtil.ExcelHelper.Revised
{
    class TradeTab : AbstractTab
    {
        private static ILog logger = log4net.LogManager.GetLogger(typeof(TradeTab));

        private const int MINIMUM_COUNT_FOR_STDEV = 20;
        //private static string[] Colume_Name_Merge_Order = { "Account", "Symbol", "Name", "MarginType", "Side", "AvgPrice", "CumQty", "SlipageInBps", "ParticipationRate", "Date" };
        //private static string[] Colume_Name_Not_Merge_Order = { "Account", "Symbol", "Name", "MarginType", "Side", "AvgPrice", "CumQty", "SlipageInBps", "ParticipationRate", "EffectiveTime", "ExpireTime" };

        private static string[] Colume_Headers = { "Symbol\r\n证券代码", "Name\r\n证券名称", "EffectiveTime\r\n开始时间",
                                                     "ExpireTime\r\n结束时间", "Side\r\n买卖方向", "AvgPrice\r\n成交均价",
                                                     "CumQty\r\n成交总量", "SlipageInBps\r\n滑点差" };



        private const int HEAD_ROW_INDEX = 1;

        public TradeTab()
        {

        }

        public ClientTradeSummary createTradeTab(List<SavedClientOrder> orders_, Client client_, HSSFWorkbook wb_, string tabName)
        {
            this.columeName = Colume_Headers;
            ISheet tb = wb_.CreateSheet(tabName);
            createSheetHeader(tb, wb_);
            int columeNb = this.columeName.Length;

            List<LineContent> lines = this.buildExcelLines(orders_, client_);
            List<decimal> slipList = new List<decimal>();
            List<decimal> turnoverList = new List<decimal>();

            HSSFCellStyle style = CellStyle.contentStyle(wb_);

            foreach (LineContent line in lines)
            {
                IRow row = tb.CreateRow(currentLine++);
                ICell[] cells = new ICell[columeNb];
                for (int j = 0; j < columeNb; j++)
                {
                    cells[j] = row.CreateCell(j);
                    cells[j].CellStyle = style;
                }

                cells[0].SetCellValue(line.symbol);
                cells[1].SetCellValue(line.stockName);
                cells[2].SetCellValue(line.effectiveTime);
                cells[3].SetCellValue(line.expireTime);
                cells[4].SetCellValue(line.side);
                cells[5].SetCellValue(Double.Parse(MathUtil.round(line.avgPrice).ToString()));
                cells[6].SetCellValue(Double.Parse(line.volume.ToString()));
                cells[7].SetCellValue(Double.Parse(MathUtil.round(line.slipage).ToString()));

                slipList.Add(line.slipage);
                turnoverList.Add(line.turnover);
            }

            ClientTradeSummary tradeSummary = new ClientTradeSummary(client_, orders_);
            this.addFootLine(wb_, tb, turnoverList, slipList, tradeSummary.getTotalTurnover(), tradeSummary.getCancelRate(), tradeSummary.getFillRate());
            this.setAutoSizeColumn(tb, columeNb);
            currentLine = 0;
            return tradeSummary;
        }

        private void addFootLine(HSSFWorkbook wb_, ISheet tb, List<decimal> turnoverList_, List<decimal> slipList_, decimal turnover_,
             decimal cancelRate_, decimal fillRate_)
        {
            currentLine++;  // Add one blank line.

            HSSFCellStyle style = CellStyle.footerStyle(wb_);

            int columeNb = columeName.Length;
            IRow summaryRow = tb.CreateRow(currentLine++);
            
            ICell[] summaryCells = new ICell[columeNb];
            for (int j = 0; j < columeNb; j++)
            {
                summaryCells[j] = summaryRow.CreateCell(j);
                summaryCells[j].SetCellValue("");
            }

            summaryRow = tb.CreateRow(currentLine++);
            summaryRow.Height = 500;
            summaryCells = new ICell[columeNb];
            for (int j = 0; j < columeNb; j++)
            {
                summaryCells[j] = summaryRow.CreateCell(j);
            }

            summaryCells[0].SetCellValue("合计");
            summaryCells[0].CellStyle = style;

            summaryCells[1].SetCellValue("撤单率");
            summaryCells[1].CellStyle = style;
            
            summaryCells[2].SetCellValue("成交额(元)");
            summaryCells[2].CellStyle = style;

            summaryCells[3].SetCellValue("滑点差(bp)");
            summaryCells[3].CellStyle = style;

            summaryCells[4].SetCellValue("成交额加权滑点差(bp)");
            summaryCells[4].CellStyle = style;

            summaryRow = tb.CreateRow(currentLine++);
            summaryRow.Height = 500;
            summaryCells = new ICell[columeNb];
            for (int j = 0; j < columeNb; j++)
            {
                summaryCells[j] = summaryRow.CreateCell(j);
                if (j >= 0 && j <= 4)
                    summaryCells[j].CellStyle = style;
            }
            summaryCells[1].SetCellValue(NumberFormat.asPercent(MathUtil.round(cancelRate_)));
            summaryCells[2].SetCellValue(NumberFormat.toMoney(MathUtil.round(turnover_)));
            summaryCells[3].SetCellValue(MathUtil.round(MathUtil.computeAvgSlipage(slipList_)).ToString());
            summaryCells[4].SetCellValue(MathUtil.round(MathUtil.computeWeightedSlipage(slipList_, turnoverList_)).ToString());            
        }


        /// <summary>
        /// Build an excel row info from saved client order. All orders are listed separeatedly from version 1.7.
        /// Those clients with merge order option set are logged with an error but not actually do merge order operation.
        /// MergeOrder mode is not supported anymore.
        /// </summary>
        /// <param name="orders_">saved client orders</param>
        /// <param name="client_">client info.</param>
        /// <returns></returns>
        private List<LineContent> buildExcelLines(List<SavedClientOrder> orders_, Client client_)
        {
            return this.buildExcelLinesNotMergeOrder(orders_, client_);
            //if (client_.shouldMergeOrders())
            //{
            //    logger.Error("Client is set to MERGE ORDER, which is not supported anymore.");
            //    return this.buildExcelLinesNotMergeOrder(orders_, client_);
            //    //return this.buildExcelLinesMergeOrder(orders_, client_);
            //}
            //else
            //{
            //    return this.buildExcelLinesNotMergeOrder(orders_, client_);
            //}
        }

        private List<LineContent> buildExcelLinesNotMergeOrder(List<SavedClientOrder> orders_, Client client_)
        {
            List<LineContent> lines = new List<LineContent>();
            decimal avgPrice = 0;
            decimal vwSlipage = 0;
            decimal volume = 0;
            foreach (SavedClientOrder order in orders_)
            {
                avgPrice = order.getAvgPrice();
                vwSlipage = order.getSlipage();
                volume = order.getCumQty();
                lines.Add(
                    new LineContent(client_.getAccountId(), order.getSymbol(), order.getStockName(), order.getSide().ToString(),
                        order.getMarginType().ToString(),
                avgPrice, volume, vwSlipage, order.getParticipation(),
                order.getEffectiveTime(), order.getExpireTime(), order.getTurnover()));
            }
            return lines;
        }

        /// <summary>
        /// Build lines and merge orders. Same orders are merged first.
        /// Since reporting merged orders is not supported anymore, this line is to be deleted.
        /// </summary>
        /// <param name="orders_"></param>
        /// <param name="client_"></param>
        /// <returns>Lines to add to the tab.</returns>
        /// This method is not used anymore. All orders are listed seperatedly.
        private List<LineContent> buildExcelLinesMergeOrder(List<SavedClientOrder> orders_, Client client_)
        {
            Dictionary<SavedClientOrder, List<SavedClientOrder>> orderMap = this.getGroupedOrders(orders_);
            List<LineContent> lines = new List<LineContent>();
            decimal avgPrice = 0;
            decimal vwSlipage = 0;
            decimal volume = 0;
            decimal turnover = 0;
            foreach (var order in orderMap)
            {
                SavedClientOrder orderKey = order.Key;
                List<SavedClientOrder> orderList = order.Value;

                decimal tmpAvgPrice = 0;
                decimal tmpAvgSlipage = 0;
                decimal tmpVWSlipage = 0;
                decimal tmpVolume = 0;
                decimal tmpTurnover = 0;

                foreach (SavedClientOrder tmpOrder in orderList)
                {
                    tmpAvgSlipage += tmpOrder.getSlipage();
                    tmpVWSlipage += tmpOrder.getSlipage() * tmpOrder.getCumQty();
                    tmpAvgPrice += tmpOrder.getAvgPrice() * tmpOrder.getCumQty();
                    tmpVolume += tmpOrder.getCumQty();
                    tmpTurnover += tmpOrder.getTurnover();
                }

                avgPrice = tmpAvgPrice / tmpVolume;
                vwSlipage = tmpVWSlipage / tmpVolume;
                volume = tmpVolume;
                turnover = tmpTurnover;
                lines.Add(
                    new LineContent(client_.getAccountId(), orderKey.getSymbol(), orderKey.getStockName(), orderKey.getSide().ToString(),
                        orderKey.getMarginType().ToString(),
                        avgPrice, volume, vwSlipage, orderKey.getParticipation(),
                        orderKey.getTradingDay(), turnover));
            }
            return lines;
        }

        /*
          * Group the orders according to their symbol and side (direction)
          * Used to build merged order lines. To be deleted.
          * */
        private Dictionary<SavedClientOrder, List<SavedClientOrder>> getGroupedOrders(List<SavedClientOrder> orders_)
        {
            Dictionary<SavedClientOrder, List<SavedClientOrder>> orderMap = new Dictionary<SavedClientOrder, List<SavedClientOrder>>();
            foreach (SavedClientOrder order in orders_)
            {
                if (orderMap.ContainsKey(order))
                {
                    orderMap[order].Add(order);
                }
                else
                {
                    List<SavedClientOrder> tmpList = new List<SavedClientOrder>();
                    tmpList.Add(order);
                    orderMap.Add(order, tmpList);
                }
            }
            return orderMap;
        }

        /// <summary>
        /// Excel line headings.
        /// </summary>
        private class LineContent
        {
            public string accountId;
            public string symbol;
            public string stockName;
            public string side;
            public string marginType;
            public decimal avgPrice;
            public decimal volume;
            public decimal slipage;
            public decimal participationRate;
            public string tradingDay;
            public string effectiveTime;
            public string expireTime;
            public decimal turnover;

            public LineContent(string accountId_, string symbol_, string stockName_, string side_,
                string marginType_, 
                decimal avgPrice_, decimal volume_, decimal slipage_, decimal participationRate_,
                string tradingDay_, decimal turnover_)
            {
                this.accountId = accountId_;
                this.symbol = symbol_;
                this.stockName = stockName_;
                this.side = side_;
                this.marginType = marginType_;
                this.avgPrice = avgPrice_;
                this.volume = volume_;
                this.slipage = slipage_;
                this.participationRate = participationRate_;
                this.tradingDay = tradingDay_;
                this.turnover = turnover_;
            }

            public LineContent(string accountId_, string symbol_, string stockName_, string side_,
                string marginType_,
                decimal avgPrice_, decimal volume_, decimal slipage_, decimal participationRate_, 
                string effectiveTime_, string expireTime_, decimal turnover_)
            {
                this.accountId = accountId_;
                this.symbol = symbol_;
                this.stockName = stockName_;
                this.side = side_;
                this.marginType = marginType_;
                this.avgPrice = avgPrice_;
                this.volume = volume_;
                this.slipage = slipage_;
                this.participationRate = participationRate_;
                this.effectiveTime = effectiveTime_;
                this.expireTime = expireTime_;
                this.turnover = turnover_;
            }
        }
    }


}
