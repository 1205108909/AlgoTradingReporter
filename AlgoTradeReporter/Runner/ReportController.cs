/* ==============================================================================
 * ClassName：ReportRunner
 * Description：
 * Author：zhaoyu
 * Created Time：2015/1/7 15:55:01
 * Revision Time：
 * Notes：
 * @version 1.0
* ==============================================================================*/


using AlgoTradeReporter.Config;
using AlgoTradeReporter.Data.Trades;
using AlgoTradeReporter.FileUtil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using AlgoTradeReporter.StoredProc;
using System.IO;
using AlgoTradeReporter.Util;
using AlgoTradeReporter.Email;
using log4net;
using AlgoTradeReporter.Data;
using AlgoTradeReporter.Data.Commons;
using AlgoTradeReporter.Data.ClientInfo;
using System.Net.Mail;
using System.Threading;
using AlgoTradeReporter.FileUtil.ExcelHelper.Revised;

namespace AlgoTradeReporter
{
    class ReportController
    {
        private static ILog logger = log4net.LogManager.GetLogger(typeof(ReportController));

        private RunnerParas paras;
        private ReportRunner runner;

        public ReportController()
        {
            this.runner = new ReportRunner();
            this.paras = new RunnerParas();
        }

        public void init(string[] args_)
        {
            paras.parseArgs(args_);
            runner.init(paras);
        }

        /// <summary>
        /// Do the job here. First Validate the input, the job is done accounding to the mode.
        /// </summary>
        public void run()
        {
            if (ParaValidator.isParaValid(paras))
            {
                ReportSenderMgr.SENDER.getExecReportSender().initExecReport(paras);

                Mode mode = paras.parseMode();
                if (Mode.REGULAR == mode)
                {
                    runner.writeOrderToDb();
                    runner.sendScheduledReport();
                }
                else if (Mode.REPORTER == mode)
                {
                    runner.sendScheduledReport();
                }
                else if (Mode.SAVER == mode)
                {
                    runner.writeOrderToDb();
                }
                else if (Mode.CLIENT_REPORT == mode)
                {
                    runner.sendSpecifiedClientReport();
                }
                else if (Mode.MANAGER_REPORT == mode)
                {
                    runner.sendManagerReport();
                }
                else
                {
                    logger.Error("Invalid Running Mode " + mode);
                }
            }
            else
            {
                ReportSenderMgr.SENDER.getExecReportSender().generateInvalidParaEmail(paras.parseMode());
            }
            ReportSenderMgr.SENDER.sendExecutionReport();
            runner.afterWork();
        }
    }
}
