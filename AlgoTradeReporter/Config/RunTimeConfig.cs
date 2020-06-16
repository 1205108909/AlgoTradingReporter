using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AlgoTradeReporter.Config
{
    class RunTimeConfig
    {
        private bool reportZeroQtyOrder;
        private string excelFolder;
        private bool keepAttachement;


        public RunTimeConfig(bool reportZeroQtyOrder_, string excelFolder_, bool keepAttachement_)
        {
            this.reportZeroQtyOrder = reportZeroQtyOrder_;
            this.excelFolder = excelFolder_;
            this.keepAttachement = keepAttachement_;
        }

        public bool reportZeroQtyOrders()
        {
            return this.reportZeroQtyOrder;
        }

        public string getExcelFolder()
        {
            return this.excelFolder;
        }

        public bool isKeepAttach()
        {
            return this.keepAttachement;
        }


    }
}
