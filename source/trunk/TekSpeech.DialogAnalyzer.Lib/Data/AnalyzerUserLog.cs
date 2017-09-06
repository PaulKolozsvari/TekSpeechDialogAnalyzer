namespace TekSpeech.DialogAnalyzer.Lib.Data
{
    #region Using Directives

    using Figlut.Server.Toolkit.Data;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using TekSpeech.DialogAnalyzer.Lib.Configuration;

    #endregion //Using Directives

    public class AnalyzerUserLog : List<AnalyzerLogLine>
    {
        #region Constructors

        public AnalyzerUserLog(int userId)
        {
            _userId = userId;
        }

        #endregion //Constructors

        #region Fields

        private int _userId;
        private TimeSpan _totalDuration;

        #endregion //Fields

        #region Properties

        public int UserId
        {
            get { return _userId; }
        }

        public TimeSpan TotalDuration
        {
            get { return _totalDuration; }
            set { _totalDuration = value; }
        }

        #endregion //Properties

        #region Methods

        public AnalyzerSearchResult SearchVoiceCommandOccurences(
            List<string> searchVoiceCommandText, 
            bool caseSensitive, 
            List<string> originatingVoiceCommandTextWords,
            List<string> secondOriginatingVoiceCommandTextWords)
        {
            List<AnalyzerLogLine> occurences = new List<AnalyzerLogLine>();
            List<string> searchVoiceCommandTextLower = new List<string>();
            searchVoiceCommandText.ForEach(p => searchVoiceCommandTextLower.Add(p.ToLower()));
            TimeSpan totalTimeWasted = new TimeSpan();
            List<AnalyzerLogLine> originatingVoiceCommands = new List<AnalyzerLogLine>();
            List<AnalyzerLogLine> secondOriginatingVoiceCommands = new List<AnalyzerLogLine>();
            for (int i = 0; i < this.Count; i++)
            {
                AnalyzerLogLine line = this[i];
                string valueToSearch = caseSensitive ? line.VoiceCommand : line.VoiceCommand.ToLower();
                List<string> valueToSearchFor = caseSensitive ? searchVoiceCommandText : searchVoiceCommandTextLower;
                bool match = false;
                foreach (string value in valueToSearchFor)
                {
                    if (valueToSearch.Contains(value))
                    {
                        match = true;
                        break;
                    }
                }
                if (!match)
                {
                    continue;
                }
                occurences.Add(line);
                if (originatingVoiceCommandTextWords == null)
                {
                    continue;
                }
                int previousLineIndex = i;
                AnalyzerLogLine originatingLine = GetOriginatingLine(caseSensitive, i, originatingVoiceCommandTextWords);
                AnalyzerLogLine nextOriginatingLogLine = GetNextOriginatingLine(caseSensitive, i, originatingVoiceCommandTextWords);
                if (originatingLine != null && nextOriginatingLogLine != null)
                {
                    TimeSpan commandTimeWasted = nextOriginatingLogLine.TimeStamp.Subtract(originatingLine.TimeStamp);
                    totalTimeWasted = totalTimeWasted.Add(commandTimeWasted);
                    originatingVoiceCommands.Add(originatingLine);
                }
                AnalyzerLogLine secondOriginatingLine = GetOriginatingLine(caseSensitive, i, secondOriginatingVoiceCommandTextWords);
                if (secondOriginatingLine != null)
                {
                    secondOriginatingVoiceCommands.Add(secondOriginatingLine);
                }
            }

            AnalyzerSearchResult result = new AnalyzerSearchResult(_userId.ToString(), searchVoiceCommandText, null, occurences, _totalDuration, totalTimeWasted);
            User user = UserDictionary.Instance[_userId];
            string userString = user == null ? _userId.ToString() : string.Format("{0}({1})", user.UserName, _userId);
            result.Description = string.Format(
                "{0} '{1}' for user {2} in {3}:{4}:{5}:{6}:{7} at {8}/hour. Time wasted: {9}/{10} {11}%",
                result.TotalOccurences.ToString().PadRight(10),
                searchVoiceCommandText,
                userString.PadRight(25),
                (_totalDuration.Days.ToString() + "D").PadRight(5),
                (_totalDuration.Hours.ToString() + "H").PadRight(5),
                (_totalDuration.Minutes.ToString() + "M").PadRight(5),
                (_totalDuration.Seconds.ToString() + "S").PadRight(5),
                (_totalDuration.Milliseconds.ToString() + "MS").PadRight(5),
                result.OccurencesPerHour.ToString().PadRight(5),
                result.TimeWasted,
                result.Duration,
                result.PercentageTimeWasted);

            result.OriginatingVoiceCommands.Clear();
            originatingVoiceCommands.ForEach(p => result.OriginatingVoiceCommands.Add(p));
            result.SecondOriginatingVoiceCommands.Clear();
            secondOriginatingVoiceCommands.ForEach(p => result.SecondOriginatingVoiceCommands.Add(p));

            return result;
        }

        private AnalyzerLogLine GetOriginatingLine(bool caseSensitive, int currentLineIndex, List<string> originatingVoiceCommandTextWords)
        {
            if ((currentLineIndex - 1) < 0 || (originatingVoiceCommandTextWords == null))
            {
                return null; //Before the beginning of the list.
            }
            currentLineIndex--;
            while (currentLineIndex > -1) //Traverse back up the list to look for originating line causing the searchVoiceCommandText to be written.
            {
                string originatingValueToSearch = caseSensitive ? this[currentLineIndex].VoiceCommand : this[currentLineIndex].VoiceCommand.ToLower();
                for (int k = 0; k < originatingVoiceCommandTextWords.Count; k++)
                {
                    if (DataShaper.StringContainsKeywords(this[currentLineIndex].VoiceCommand, caseSensitive, originatingVoiceCommandTextWords))
                    {
                        return this[currentLineIndex];
                    }
                }
                currentLineIndex--;
            }
            return null;
        }

        private AnalyzerLogLine GetNextOriginatingLine(bool caseSensitive, int currentLineIndex, List<string> originatingVoiceCommandTextWords)
        {
            if ((currentLineIndex + 1) >= this.Count)
            {
                return null; //After the end of the list.
            }
            currentLineIndex++;
            while (currentLineIndex < this.Count)
            {
                if (DataShaper.StringContainsKeywords(this[currentLineIndex].VoiceCommand, caseSensitive, originatingVoiceCommandTextWords))
                {
                    return this[currentLineIndex];
                }
                currentLineIndex++;
            }
            return null;
        }

        #endregion //Methods
    }
}
