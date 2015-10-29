using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ParseProgram {
    class Program {
        static void Main(string[] args) {
            Console.Title = typeof(Program).Name;
            Run();

            //var a = new List<LogItem> {
            //    new LogItem {
            //        Name = "Greg",
            //        Age = "25",
            //        Reason = "Stomach Cramps"
            //    },
            //    new LogItem {
            //        Name = "Eric",
            //        Age = "25",
            //        Reason = "Headache"
            //    }
            //};
            //var searchTerm = "ch";
            //var allFields = typeof(LogItem).GetProperties();

            //var results = (from li in a
            //               where allFields.Any(f => ((string)f.GetValue(li)).Contains(searchTerm))
            //               select li).ToList();

            //Console.WriteLine(results);
        }

        static void Run() {
            while (true) {
                var consoleInput = ReadFromConsole();
                if (string.IsNullOrWhiteSpace(consoleInput)) {
                    continue;
                }
                try {
                    var cmd = new ConsoleCommand(consoleInput);

                    string result = Execute(cmd);
                    WriteToConsole(result);
                }
                catch (Exception ex) {
                    WriteToConsole(ex.Message);
                }
            }
        }

        static string Execute(ConsoleCommand command) {
            var entries = new List<LogItem>();
            Console.Clear();

            for (int i = 0; i < command.Arguments.Count; i += 2) {
                var cmd = command.Arguments[i];
                switch (cmd) {
                    case "-file":
                        entries = ParseFile(command.Arguments[i + 1]);
                        break;

                    case "-sort":
                        entries = SortLogs(entries, command.Arguments[i + 1]);
                        break;

                    case "-search":
                        entries = SearchLogs(entries, command.Arguments[i + 1]);
                        break;
                }
            }
            var sb = new StringBuilder();
            //var props = typeof(LogItem).GetProperties();
            foreach (var log in entries) {
                var logProps = log.GetType().GetProperties();
                foreach (var item in logProps) {
                    var name = item.Name;
                    var value = item.GetValue(log).ToString();
                    sb.AppendFormat("{0}: {1}",name, value);
                    sb.AppendLine();
                }
                sb.Append("\n==============END OF RESULT==============\n\n");
            }
            return sb.ToString();
        }

        //writes message to console
        static void WriteToConsole(string message = "") {
            if (message.Length > 0) {
                Console.WriteLine(message);
            }
        }

        //prompts the user for a command
        const string _readPrompt = "console>";
        public static string ReadFromConsole(string promptMessage = "") {
            Console.Write(_readPrompt + promptMessage);
            return Console.ReadLine();
        }

        static List<LogItem> SearchLogs() {
            return null;
        }

        /// <summary>
        /// Search through all properties to see if any contain the search term
        /// </summary>
        /// <param name="logs"></param>
        /// <param name="searchTerm"></param>
        /// <returns></returns>
        static List<LogItem> SearchLogs(List<LogItem> logs, string searchTerm = null) {
            //check to make sure there is a valid search term
            if (searchTerm != null) {
                var allProps = typeof(LogItem).GetProperties();
                var results = (from l in logs
                               where allProps.Any(p => ((string)p.GetValue(l)).Contains(searchTerm))
                               select l).ToList();
                return results;
            }
            return logs;
        }

        /// <summary>
        /// Sorts the Log entries based on user input, if no input is provided will sort by FacilityID
        /// </summary>
        /// <param name="logs"></param>
        /// <param name="order"></param>
        /// <returns></returns>
        static List<LogItem> SortLogs(List<LogItem> logs, string order = "FacilityID") {

            //defines parameter for OrderBy clause "(log =>..."
            var param = Expression.Parameter(typeof(LogItem), "log");

            //create lamba function useing the user provided input "order"
            var genericSortExp = Expression.Lambda<Func<LogItem, object>>(Expression.Property(param, order), param);

            var ordered = logs.AsQueryable().OrderBy(genericSortExp).ToList();
            return ordered;
        }


        /// <summary>
        /// Will look up a text file and parse the text to a list of logs
        /// </summary>
        /// <param name="fileLoc"></param>
        /// <returns></returns>
        static List<LogItem> ParseFile(string fileLoc) {
            var parsedLogs = new List<LogItem>();
            var newLog = new LogItem();//creates a new log item
            StreamReader reader = new StreamReader(fileLoc); //loads the file as var reader
            string line; //current line variable

            PropertyInfo[] logProps = typeof(LogItem).GetProperties();//gets all the properties of a logItem
            Regex regPattern = new Regex(@"([^:]*):\s*(.*)");//patten to get everything before and after the colon as groups
            var matches = new List<string>();

            //while not at the end of the file
            while (!reader.EndOfStream) {
                line = reader.ReadLine();//sets the current line
                matches.Clear();//clears matches list

                //adds each group to the matches list
                foreach (Match match in regPattern.Matches(line)) {
                    matches.Add(match.Groups[1].Value.ToString().Replace(" ", string.Empty));//removes and spaces
                    matches.Add(match.Groups[2].Value);
                }

                if (matches.Count > 0) {
                    foreach (var prop in logProps) {
                        if (prop.Name == matches[0]) {
                            prop.SetValue(newLog, matches[1]);
                            break;//exits the foreach loop
                        }
                    }
                }
                else if (line.IndexOf("END OF RESULT") != -1) {
                    parsedLogs.Add(newLog);
                    newLog = new LogItem();
                }
            }
            return parsedLogs;
        }
    }
}
