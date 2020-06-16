/* ==============================================================================
 * ClassName：ManagerTab
 * Description：
 * Author：zhaoyu
 * Created Time：2018/2/1 11:25:54
 * Revision Time：
 * Notes：
 * @version 1.0
 * 
 * @version 1.7
 * Revision Time : 2016-12-12
 * Notes : Revised Tab format and cell format.
* ==============================================================================*/


using AlgoTradeReporter.Config;
using AlgoTradeReporter.Util;
using AlgoTrading.Engine.Util;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace AlgoTradeReporter.FileUtil.ExcelHelper.Revised
{
    class ManagerTab : AbstractTab
    {
        private const string ACCOUNT_COLUME = "ACCOUNT";
        private const string NAME_COLUME = "NAME";
        private const string TURNOVER_COLUME = "TURNOVER";
        private const string FILLRATE_COLUME = "FILL RATE";
        private const string CANCEL_COLUME = "CANCEL RATE";
        private string[] ALGO_RESULT_COLUMES = { "ClientOrder", "ExchangeOrder", "WeightedSlipage", "Turnover" };

        private const int FIRST_NUMERIC_COLUME = 5;

        private int columeEachAlgo;
        private int algoNb;
        private int columeNb;
        private List<OrderAlgo> sortedAlgos;
        private AggregatedStatistics aggregatedStatistics;
       
        public ManagerTab()
        {
            columeEachAlgo = ALGO_RESULT_COLUMES.Length;
            algoNb = Enum.GetValues(typeof(OrderAlgo)).Length;

            columeNb = FIRST_NUMERIC_COLUME + columeEachAlgo * algoNb;
            sortedAlgos =new List<OrderAlgo>();
            aggregatedStatistics = new AggregatedStatistics();
        }

        public void createManagerTab(List<ClientTradeSummary> tradeStatistics_, List<string> tradingDays_, HSSFWorkbook wb_, string tabName_)
        {      
            ISheet tb = wb_.CreateSheet(tabName_);
            wb_.SetSheetOrder(tabName_, 0);
            sortAlgos();

            generateDateLine(tradingDays_, tb, wb_);
            addStrategyLine(tb, wb_);
            addColumeNameLine(tb, wb_);
            addStatistics(tradeStatistics_, tb, wb_);

            computeAggregateStatistics(tradeStatistics_);
            addSummaryLine(tb, wb_);

            setAutoSizeColumn(tb, columeNb);
            mergeCells(tb);
            currentLine = 0;
        }

        private void sortAlgos()
        {
            Array algos = Enum.GetValues(typeof(OrderAlgo));

            // To make sure VWAP then TWAP then others sequence.
            if (!sortedAlgos.Contains(OrderAlgo.VWAP))
                sortedAlgos.Add(OrderAlgo.VWAP);
            if (!sortedAlgos.Contains(OrderAlgo.UNKNOWN_ALGO))
                sortedAlgos.Add(OrderAlgo.UNKNOWN_ALGO);
            int index = 1;
            foreach (OrderAlgo algo in algos)
            {
                if (!sortedAlgos.Contains(algo))
                {                    
                    sortedAlgos.Insert(index ++, algo);
                }
            }
        }

        private void addStrategyLine(ISheet tab_, HSSFWorkbook wb_)
        {
            IRow row = tab_.CreateRow(currentLine++);
            ICell[] cells = new ICell[columeNb];
            
            int firstNonBlankCell = 0;
            HSSFCellStyle style = CellStyle.headingStyle(wb_);

            int algoIndex = 0;
            for (int i = FIRST_NUMERIC_COLUME; i < columeNb; i++)
            {
                cells[i] = row.CreateCell(i, CellType.String);
                if ((i - FIRST_NUMERIC_COLUME) % columeEachAlgo == 0)
                {
                    cells[i].SetCellValue(sortedAlgos[algoIndex].ToString());
                    cells[i].CellStyle = style;
                    algoIndex++;
                    if(firstNonBlankCell == 0)
                    {
                        firstNonBlankCell = i - ALGO_RESULT_COLUMES.Length / 2;
                    }
                }
                else
                {
                    cells[i].SetCellValue("");
                    cells[i].CellStyle = style;
                }
            }

            for (int i = 0; i <= 4; i++)
            {
                cells[i] = row.CreateCell(i, CellType.String);
                cells[i].CellStyle = style;
                cells[i].SetCellValue("");
            }

            cells[1].SetCellValue("Total");
        }


        private void mergeCells(ISheet tab_)
        {
            int lineNb = 1;
            CellRangeAddress totalRegion = new CellRangeAddress(lineNb, lineNb, 1, 4);
            tab_.AddMergedRegion(totalRegion);


            for (int i = 0; i < sortedAlgos.Count; i++)
            {
                CellRangeAddress region = new CellRangeAddress(lineNb, lineNb,
                    FIRST_NUMERIC_COLUME + i * ALGO_RESULT_COLUMES.Length, FIRST_NUMERIC_COLUME + (i + 1) * ALGO_RESULT_COLUMES.Length - 1);

                tab_.AddMergedRegion(region);
            }
        }


        private void addColumeNameLine(ISheet tab_, HSSFWorkbook wb_)
        {
            Array algos = Enum.GetValues(typeof(OrderAlgo));
            IRow row = tab_.CreateRow(currentLine++);
            ICell[] cells = new ICell[columeNb];
            HSSFCellStyle style = CellStyle.headingStyle(wb_);

            cells[0] = row.CreateCell(0, CellType.String);
            cells[0].SetCellValue(ACCOUNT_COLUME);
            cells[0].CellStyle = style;

            cells[1] = row.CreateCell(1, CellType.String);
            cells[1].SetCellValue(NAME_COLUME);
            cells[1].CellStyle = style;

            cells[2] = row.CreateCell(2, CellType.String);
            cells[2].SetCellValue(TURNOVER_COLUME);
            cells[2].CellStyle = style;

            cells[3] = row.CreateCell(3, CellType.String);
            cells[3].SetCellValue(FILLRATE_COLUME);
            cells[3].CellStyle = style;

            cells[4] = row.CreateCell(4, CellType.String);
            cells[4].SetCellValue(CANCEL_COLUME);
            cells[4].CellStyle = style;

            int index = FIRST_NUMERIC_COLUME;
            for (int i = 0; i < algos.Length; i++)
            {
                for (int j = 0; j < ALGO_RESULT_COLUMES.Length; j++)
                {
                    cells[index] = row.CreateCell(index, CellType.String);
                    cells[index].SetCellValue(ALGO_RESULT_COLUMES[j]);
                    cells[index].CellStyle = style;
                    index++;
                }
            }
        }

        private void addStatistics(List<ClientTradeSummary> tradeStatistics_, ISheet tab_, HSSFWorkbook wb_)
        {
            HSSFCellStyle leftColStyle = CellStyle.headingStyle(wb_);
            HSSFCellStyle contentStyle = CellStyle.contentStyle(wb_);

            foreach(ClientTradeSummary summary in tradeStatistics_)
            {
                IRow row = tab_.CreateRow(currentLine++);
                ICell[] cells = new ICell[columeNb];
                cells[0] = row.CreateCell(0, CellType.String);
                cells[0].SetCellValue(summary.getAccountId());
                cells[0].CellStyle = leftColStyle;

                cells[1] = row.CreateCell(1, CellType.String);
                cells[1].SetCellValue(summary.getAbbrName());
                cells[1].CellStyle = leftColStyle;

                cells[2] = row.CreateCell(2, CellType.Numeric);
                cells[2].SetCellValue((double)MathUtil.round(summary.getTotalTurnover(), 2));
                cells[2].CellStyle = leftColStyle;

                cells[3] = row.CreateCell(3, CellType.Numeric);
                cells[3].SetCellValue((double)MathUtil.round(summary.getFillRate(), 4));
                cells[3].CellStyle = leftColStyle;

                cells[4] = row.CreateCell(4, CellType.Numeric);
                cells[4].SetCellValue((double)MathUtil.round(summary.getCancelRate(), 4));
                cells[4].CellStyle = leftColStyle;

                int firstPosition = FIRST_NUMERIC_COLUME;
                int offset = 0;
                int algoNb = 0;

                foreach (OrderAlgo algo in sortedAlgos)
                {
                    StrategyStatistics algoStrategy = summary.getByAlgo(algo);
                    offset = columeEachAlgo * algoNb + firstPosition;

                    cells[offset + 0] = row.CreateCell(offset + 0, CellType.Numeric);
                    cells[offset + 0].SetCellValue((double)MathUtil.round(algoStrategy.getOrderCount(), 2));
                    cells[offset + 0].CellStyle = contentStyle;

                    cells[offset + 1] = row.CreateCell(offset + 1, CellType.Numeric);
                    cells[offset + 1].SetCellValue((double)MathUtil.round(algoStrategy.getSliceCount(), 2));
                    cells[offset + 1].CellStyle = contentStyle;

                    cells[offset + 2] = row.CreateCell(offset + 2, CellType.Numeric);
                    cells[offset + 2].SetCellValue((double)MathUtil.round(algoStrategy.getSlipage(), 2));
                    cells[offset + 2].CellStyle = contentStyle;

                    cells[offset + 3] = row.CreateCell(offset + 3, CellType.Numeric);
                    cells[offset + 3].SetCellValue((double)MathUtil.round(algoStrategy.getTurnover(), 2));
                    cells[offset + 3].CellStyle = contentStyle;

                    algoNb++;
                }
            }
        }

        private void addSummaryLine(ISheet tab_, HSSFWorkbook wb_)
        {
            HSSFCellStyle style = CellStyle.footerStyle(wb_);

            currentLine++; currentLine++;
            IRow row = tab_.CreateRow(currentLine++);
            ICell[] cells = new ICell[columeNb];

            cells[1] = row.CreateCell(1, CellType.String);
            cells[1].SetCellValue("TOTAL");
            cells[1].CellStyle = style;

            for (int i = 2; i < 6; i++)
            {
                cells[i] = row.CreateCell(i, CellType.String);
                cells[i].SetCellValue(ALGO_RESULT_COLUMES[i - 2]);
                cells[i].CellStyle = style;
            }
            cells[6] = row.CreateCell(6, CellType.String);
            cells[6].SetCellValue("Slipage Stdev");
            cells[6].CellStyle = style;

            cells[7] = row.CreateCell(7, CellType.String);
            cells[7].SetCellValue("Cancel Rate");
            cells[7].CellStyle = style;

            cells[8] = row.CreateCell(8, CellType.String);
            cells[8].SetCellValue("Fill Rate");
            cells[8].CellStyle = style;

            writeAggreagetResult(tab_, wb_);
        }

        private void writeAggreagetResult(ISheet tab_, HSSFWorkbook wb_)
        {
            HSSFCellStyle style = CellStyle.footerStyle(wb_);

            IRow row = tab_.CreateRow(currentLine++);
            ICell[] cells = new ICell[columeNb];

            cells[2] = row.CreateCell(2, CellType.String);
            cells[2].SetCellValue(NumberFormat.asInt(aggregatedStatistics.getOrderCount()));
            cells[2].CellStyle = style;

            cells[3] = row.CreateCell(3, CellType.String);
            cells[3].SetCellValue(NumberFormat.asInt(aggregatedStatistics.getSliceCount()));
            cells[3].CellStyle = style;

            cells[4] = row.CreateCell(4, CellType.String);
            cells[4].SetCellValue(string.Format("{0:N}", aggregatedStatistics.getSlipage()));
            cells[4].CellStyle = style;

            cells[5] = row.CreateCell(5, CellType.String);
            cells[5].SetCellValue(string.Format("{0:N}", aggregatedStatistics.getTurnover()));
            cells[5].CellStyle = style;

            cells[6] = row.CreateCell(6, CellType.String);
            cells[6].SetCellValue(string.Format("{0:N}", aggregatedStatistics.getSlipageStdev()));
            cells[6].CellStyle = style;

            cells[7] = row.CreateCell(7, CellType.String);
            cells[7].SetCellValue(NumberFormat.asPercent(MathUtil.round(aggregatedStatistics.getCancelRate())));
            cells[7].CellStyle = style;

            cells[8] = row.CreateCell(8, CellType.String);
            cells[8].SetCellValue(NumberFormat.asPercent(MathUtil.round(aggregatedStatistics.getFillRate())));
            cells[8].CellStyle = style;
        }

        private void computeAggregateStatistics(List<ClientTradeSummary> tradeStatistics_)
        {
            aggregatedStatistics.compute(tradeStatistics_);
        }
    }
}
