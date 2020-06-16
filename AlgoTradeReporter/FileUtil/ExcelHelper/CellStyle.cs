using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AlgoTradeReporter.FileUtil.ExcelHelper
{
    class CellStyle
    {
        private static HSSFCellStyle generate(HSSFWorkbook wb_, int index, int r, int g, int b, bool bold)
        {
            short colorIndex = (short)index;

            HSSFCellStyle style = (HSSFCellStyle)(wb_.CreateCellStyle());
            HSSFPalette palette = ((HSSFWorkbook)wb_).GetCustomPalette();
            palette.SetColorAtIndex(colorIndex, (byte)r, (byte)g, (byte)b);
            style.FillForegroundColor = palette.GetColor(colorIndex).Indexed;
            style.FillPattern = FillPattern.SolidForeground;

            style.BorderBottom = NPOI.SS.UserModel.BorderStyle.Thin;
            style.BorderTop = NPOI.SS.UserModel.BorderStyle.Thin;
            style.BorderLeft = NPOI.SS.UserModel.BorderStyle.Thin;
            style.BorderRight = NPOI.SS.UserModel.BorderStyle.Thin;

            style.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Center;
            style.VerticalAlignment = NPOI.SS.UserModel.VerticalAlignment.Center;
            //是否换行  
            style.WrapText = true;

            if (bold)
            {
                IFont font = wb_.CreateFont();
                font.Boldweight = short.MaxValue;
                style.SetFont(font);
            }

            return style;
        }

        public static HSSFCellStyle headingStyle(HSSFWorkbook wb_)
        {
            return generate(wb_, 57, 174, 170, 170, true);
        }

        public static HSSFCellStyle contentStyle(HSSFWorkbook wb_)
        {
            return generate(wb_, 12, 255, 230, 153, false);
        }

        public static HSSFCellStyle footerStyle(HSSFWorkbook wb_)
        {
            return generate(wb_, 49, 180, 198, 231, true);
        }
    }
}
