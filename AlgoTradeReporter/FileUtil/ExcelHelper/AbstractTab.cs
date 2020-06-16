/* ==============================================================================
 * ClassName：AbstractTab
 * Description：
 * Author：zhaoyu
 * Created Time：2018/2/1 11:40:13
 * Revision Time：
 * Notes：
 * @version 1.0
 * 
 * @version 1.7
 * Revision Time: 2016-12-12
 * Notes : add HSSFWorkbook to createSheetHeader, which is used to create cell style.
* ==============================================================================*/


using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AlgoTradeReporter.FileUtil.ExcelHelper.Revised
{
    class AbstractTab
    {
        protected int currentLine;
        protected string[] columeName;

        public AbstractTab()
        {
            currentLine = 0;
        }

        protected void generateDateLine(List<string> tradingDays_, ISheet tab_, HSSFWorkbook wb_)
        {
            IRow row = tab_.CreateRow(currentLine++);
            ICell[] cells = new ICell[5];

            for (int i = 0; i < 5; i++)
            {
                cells[i] = row.CreateCell(i, CellType.String);
            }

            HSSFCellStyle style = CellStyle.headingStyle(wb_);

            cells[0].SetCellValue("TradingDay : ");
            cells[0].CellStyle = style;

            cells[1].SetCellValue("From ");
            cells[1].CellStyle = style;

            cells[2].SetCellValue(tradingDays_[0]);
            cells[2].CellStyle = style;

            cells[3].SetCellValue(" To ");
            cells[3].CellStyle = style;

            cells[4].SetCellValue(tradingDays_[tradingDays_.Count - 1]);
            cells[4].CellStyle = style;
        }

        protected void setAutoSizeColumn(ISheet tb_, int columeCount_)
        {
            for (int i = 0; i < columeCount_; i++)
            {
                tb_.AutoSizeColumn(i);
            }

            for (int i = 0; i < columeCount_; i++)
            {
                int autoWidth = tb_.GetColumnWidth(i);
                int targetWidth = (int)((float)autoWidth * 1.1);
                tb_.SetColumnWidth(i, targetWidth);
            }
        }

        protected void createSheetHeader(ISheet tb_, HSSFWorkbook wb_)
        {
            IRow row0 = tb_.CreateRow(currentLine++);
            int columeNb = columeName.Length;

            HSSFCellStyle style = CellStyle.headingStyle(wb_);

            for (int i = 0; i < columeNb; i++)
            {
                ICell cell = row0.CreateCell(i);
                cell.CellStyle = style;
                cell.SetCellValue(columeName[i]);
            }
            row0.Height = 1000;
        }
    }

    //HSSFCell cell = row.createCell((short)0);  
//    cellStyle.setWrapText(true);//先设置为自动换行     
//cell.setCellStyle(cellStyle);                            
//cell.setCellValue(new HSSFRichTextString("hello/r/n world!")); 
}
