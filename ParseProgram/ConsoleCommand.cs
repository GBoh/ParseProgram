using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ParseProgram {
    class ConsoleCommand {

        private List<string> _arguments;
        public List<string> Arguments {
            get {
                return _arguments;
            }
        }

        public ConsoleCommand(string input) {
            //magic regex line to split on spaces but contain quotes
            var stringArr = Regex.Split(input, "(?<=^[^\"]*(?:\"[^\"]*\"[^\"]*)*) (?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)");

            _arguments = new List<string>();
            for (int i = 0; i < stringArr.Length; i++) {
                var inputArgument = stringArr[i];
                string argument = inputArgument;

                //gets quoted text
                var regex = new Regex("\"(.*?)\"", RegexOptions.Singleline);
                var match = regex.Match(inputArgument);

                if (match.Captures.Count > 0) {
                    var captureQuotedText = new Regex("[^\"]*[^\"]");
                    var quoted = captureQuotedText.Match(match.Captures[0].Value);
                    argument = quoted.Captures[0].Value;
                }
                _arguments.Add(argument);
            }
        }
    }
}
