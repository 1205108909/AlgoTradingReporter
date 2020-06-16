/* ==============================================================================
 * ClassName：ParaValidator
 * Description：
 * Author：zhaoyu
 * Created Time：2015/3/11 14:35:18
 * Revision Time：
 * Notes：Check the input mode and parameters so that invalid paras are refused 
 *        in order to avoid confusing work results.
 * @version 1.0
* ==============================================================================*/


using AlgoTradeReporter.Email;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AlgoTradeReporter
{
    class ParaValidator
    {
        private static ILog logger = log4net.LogManager.GetLogger(typeof(ParaValidator));

        /// <summary>
        /// Validate file and command input paras since so many combinations are so confusing.
        /// Rule 1. Saver mode, no file account is allowed.(All accounts must be covered)
        /// Rule 2. Regular mode, no file account is allowed. TradingDay must be one.
        /// Rule 3. Reporter mode, no file account is allowed. Report all account or nothing.
        /// Rule 4. CLIENT_REPORT, must input tradingDays and accounts from file.
        /// Rule 5. Manager report, must contains some valid input days.
        /// </summary>
        /// <param name="paras_">Input Runtime Paras</param>
        /// <returns></returns>
        public static bool isParaValid(RunnerParas paras_)
        {
            Mode mode = paras_.parseMode();
            string msg = null;
            if (mode.Equals(Mode.SAVER))
            {
                if (paras_.getAccounts().Count != 0)
                {
                    msg = "Account input is not valid for this mode " + mode.ToString();
                    addErrorMessage(msg);
                    return false;
                }
            }
            else if (mode.Equals(Mode.REGULAR))
            {
                if (paras_.getAccounts().Count != 0)
                {
                    msg = "Account input is not valid for this mode " + mode.ToString();
                    addErrorMessage(msg);
                    return false;
                }
                if (paras_.getTradingDays().Count != 1)
                {
                    msg = "More than 1 tradingDay is not valid for this mode " + mode.ToString();
                    addErrorMessage(msg);
                    return false;
                }
            }
            else if (mode.Equals(Mode.REPORTER))
            {
                //if (paras_.getAccounts().Count != 0)
                if (paras_.getAccounts().Count == 0)
                    {
                    msg = "Account input is not valid for this mode " + mode.ToString();
                    addErrorMessage(msg);
                    return false;
                }
                if (paras_.getTradingDays().Count != 1)
                {
                    msg = "More than 1 tradingDay is not valid for this mode " + mode.ToString();
                    addErrorMessage(msg);
                    return false;
                }
            }
            else if (mode.Equals(Mode.CLIENT_REPORT))
            {
                if (paras_.getTradingDays().Count == 0)
                {
                    msg = "Please specify days by file for this mode " + mode.ToString();
                    addErrorMessage(msg);
                    return false;
                }
            }
            else if (mode.Equals(Mode.MANAGER_REPORT))
            {
                if (paras_.getTradingDays().Count == 0)
                {
                    msg = "Please at least specify some tradingDays for mode " + mode.ToString();
                    addErrorMessage(msg);
                    return false;
                }
            }
            else
            {
                throw new InvalidOperationException("Input Mode " + mode.ToString() + " Not Supported.");
            }
            logger.Info("Para Validation Passed.");
            return true;

        }

        /// <summary>
        /// Add error message on console, logger, and email.
        /// </summary>
        /// <param name="msg_">Error msg.</param>
        private static void addErrorMessage(string msg_)
        {
            logger.Error(msg_);
            Console.WriteLine(msg_);
            ReportSenderMgr.SENDER.getExecReportSender().addMessage(msg_);
        }
    }
}
