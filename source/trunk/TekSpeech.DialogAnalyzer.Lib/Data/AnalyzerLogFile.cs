namespace TekSpeech.DialogAnalyzer.Lib.Data
{
    #region Using Directives

    using Figlut.Server.Toolkit.Data;
    using Figlut.Server.Toolkit.Utilities;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    #endregion //Using Directives

    public class AnalyzerLogFile : EntityCache<long, AnalyzerLogLine>
    {
        #region Constructors

        public AnalyzerLogFile(string filePath)
        {
            _filePath = filePath;
            _fileName = Path.GetFileName(_filePath);
            Analyze();
        }

        #endregion //Constructors

        #region Fields

        private string _filePath;
        private string _fileName;
        private DateTime _startDateTime;
        private DateTime _endDateTime;
        private TimeSpan _duration;
        private long _totalLineCount;
        private int _userId;

        #endregion //Fields

        #region Properties

        public int UserId
        {
            get { return _userId; }
        }

        public string FilePath
        {
            get { return _filePath; }
        }

        public string FileName
        {
            get { return _fileName; }
        }

        public DateTime StartDateTime
        {
            get { return _startDateTime; }
        }

        public DateTime EndDateTime
        {
            get { return _endDateTime; }
        }

        public TimeSpan Duration
        {
            get { return _duration; }
        }

        public long TotalLineCount
        {
            get { return _totalLineCount; }
        }

        #endregion //Properties

        #region Methods

        private void Analyze()
        {
            FileSystemHelper.ValidateFileExists(_filePath);
            string[] fileNameSplit = Path.GetFileNameWithoutExtension(_fileName).Split('_');
            if (fileNameSplit.Length != 3)
            {
                throw new ArgumentException(string.Format("Invalid format of log file name: {0}", _fileName));
            }
            _userId = Convert.ToInt32(fileNameSplit[0]);
            using (StreamReader reader = new StreamReader(_filePath))
            {
                long lineNumber = 1;
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    if (string.IsNullOrEmpty(line))
                    {
                        continue;
                    }
                    AnalyzerLogLine logLine = new AnalyzerLogLine(lineNumber, _fileName, line);
                    if (_userId != logLine.UserId)
                    {
                        //throw new ArgumentException(string.Format(
                        //    "Expected file {0} to have entries only for user {1}, but entries for user {2} also found.",
                        //    _filePath,
                        //    _userId,
                        //    logLine.UserId));
                        continue;
                    }
                    this.Add(logLine.LineNumber, logLine);
                    lineNumber++;
                }
                reader.Close();
            }
            Profile();
        }

        private void Profile()
        {
            _startDateTime = _entities[1].TimeStamp;
            _endDateTime = _entities[_entities.Count].TimeStamp;
            _duration = _endDateTime.Subtract(_startDateTime);
            _totalLineCount = _entities.Count;
        }

        public AnalyzerSearchResult SearchVoiceCommandOccurences(List<string> searchVoiceCommandText, bool caseSensitive)
        {
            List<AnalyzerLogLine> occurences = new List<AnalyzerLogLine>();
            List<string> searchVoiceCommandTextLower = new List<string>();
            searchVoiceCommandText.ForEach(p => searchVoiceCommandTextLower.Add(p.ToLower()));
            foreach(AnalyzerLogLine line in this)
            {
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
            }


            AnalyzerSearchResult result = new AnalyzerSearchResult(_filePath, searchVoiceCommandText, null, occurences, _duration, new TimeSpan());
            result.Description = string.Format(
                "{0} '{1}' for {2} in {2}:{4}:{5}:{6}:{7} at {8}/hour",
                result.TotalOccurences.ToString().PadRight(10),
                searchVoiceCommandText,
                _fileName,
                (_duration.Days.ToString() + "D").PadRight(5),
                (_duration.Hours.ToString() + "H").PadRight(5),
                (_duration.Minutes.ToString() + "M").PadRight(5),
                (_duration.Seconds.ToString() + "S").PadRight(5),
                (_duration.Milliseconds.ToString() + "MS").PadRight(5),
                result.OccurencesPerHour.ToString().PadRight(5));
            result.Description = string.Format("'{0}' for {1}", searchVoiceCommandText, _fileName);
            return result;
        }

        #endregion //Methods
    }
}
