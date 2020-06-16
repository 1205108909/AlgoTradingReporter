using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;

namespace AlgoTradeReporter
{
    class Options
    {
        // FIXME Remove this option, use date range option.
        [Option('f', "DateFile", Required = false,
            HelpText = "File address contains trading days to be processed")]
        public string DateFile { get; set; }
 
        // FIXME Remove this option, use date range option.
        [Option('d', "TradingDay", Required=false,
            HelpText = "Cmd input From:To Or single date.")]
        public string TradingDay {get; set;}

        [Option('c', "Config File", Required = false,
            HelpText = "Config file")]
        public string ConfigFile { get; set; }

        [Option('l', "Is log Roots from Config files", Required = false,
            HelpText = "If true, read log root dir from config file")]
        public bool LogFromConfig { get; set; }

        [Option('a', "Account File ", Required = false,
            HelpText = "Account File Contains accounts for which to generate report")]
        public string AccountFile { get; set; }

        [Option('m', "RunMode", Required = false,
            HelpText = "Run mode selection, REGULAR, SAVER, REPORTER, CLIENT_REPORT, MANAGER_REPORT")]
        public string RunMode { get; set; }

        [Option('t', "To List", Required = false,
            HelpText = "File contains To emails. Replace all recipents from DataBase with this mannul input.")]
        public string ToList { get; set; }

        [ParserState]
        public IParserState LastParserState { get; set; }

        [HelpOption('h', HelpText = "Print Help.")]
        public string GetUsage()
        {
            return HelpText.AutoBuild(this,
                (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current));
        }
    }
}
