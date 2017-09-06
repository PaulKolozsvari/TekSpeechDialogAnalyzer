namespace TekSpeech.DialogAnalyzer.Lib.Data
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    #endregion //Using Directives

    public class AnalyzerLogLine
    {
        #region Constructors

        public AnalyzerLogLine(long lineNumber, string fileName, string logLine)
        {
            _lineNumber = lineNumber;
            _fileName = fileName;
            _logLine = logLine;
            Parse();
        }

        #endregion //Constructors

        #region Fields

        private long _lineNumber;
        private string _fileName;
        private string _logLine;

        private string _id;

        private string _timeStampString;
        private DateTime _timeStamp;
        private string _parameter;
        private int _userId;
        private string _voiceCommandType;
        private string _voiceCommandParameter;
        private string _voiceCommand;

        #endregion //Fields

        #region Properties

        public long LineNumber
        {
            get { return _lineNumber; }
        }

        public string ID
        {
            get { return _id; }
        }

        public string TimeStampString
        {
            get { return _timeStampString; }
        }

        public DateTime TimeStamp
        {
            get { return _timeStamp; }
        }

        public string Parameter
        {
            get { return _parameter; }
        }

        public int UserId
        {
            get { return _userId; }
        }

        public string VoiceCommandType
        {
            get { return _voiceCommandType; }
        }

        public string VoiceCommandParameter
        {
            get { return _voiceCommandParameter; }
        }

        public string VoiceCommand
        {
            get { return _voiceCommand; }
        }

        #endregion //Properties

        #region Methods

        private void Parse()
        {
            string[] fields = _logLine.Split('|');

            _timeStampString = fields[0].Trim();
            _timeStamp = GetTimeStamp(_timeStampString);
            _parameter = fields[1].Trim();
            _userId = Convert.ToInt32(fields[2].Trim());
            _voiceCommandType = fields[3].Trim();
            _voiceCommandParameter = fields[4].Trim();
            _voiceCommand = fields[5].Trim();
            _id = GetId(_lineNumber, _fileName, _timeStampString, _userId);
        }

        public static string GetId(long lineNumber, string fileName, string timestampString, int userId)
        {
            return string.Format("{0}-{1}-{2}-{3}", lineNumber, fileName, timestampString, userId);
        }

        private DateTime GetTimeStamp(string timeStamp)
        {
            return DateTime.ParseExact(timeStamp, "yyyyMMddHHmmssFFF", null);
        }

        #endregion //Methods
    }
}
