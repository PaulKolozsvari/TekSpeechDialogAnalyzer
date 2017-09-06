namespace TekSpeech.DialogAnalyzer.Lib.Data
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    #endregion //Using Directives

    public class AnalyzerSearchResult
    {
        #region Constructors

        public AnalyzerSearchResult(string id, List<string> searchText, string description, List<AnalyzerLogLine> occurences, TimeSpan duration, TimeSpan timeWasted)
        {
            _id = id;
            _searchText = searchText;
            _description = description;
            _occurences = occurences;
            _duration = duration;
            _timeWasted = timeWasted;
            _percentageTimeWasted = Math.Round(((_timeWasted.TotalSeconds / _duration.TotalSeconds) * 100), 2);
            if (_timeWasted.TotalSeconds > 0)
            {
                _averageSecondsWastedPerOccurence = Math.Round((_timeWasted.TotalSeconds / _occurences.Count), 2);
            }
            _totalOccurences = occurences.Count;
            _occurencesPerHour = Convert.ToDouble(_totalOccurences) / duration.TotalHours;
            _occurencesPerHour = Math.Round(_occurencesPerHour, 4);
            _originatingVoiceCommands = new List<AnalyzerLogLine>();
            _secondOriginatingVoiceCommands = new List<AnalyzerLogLine>();
        }

        #endregion //Constructors

        #region Fields

        private string _id;
        private List<string> _searchText;
        private string _description;
        private List<AnalyzerLogLine> _occurences;
        private TimeSpan _duration;
        private TimeSpan _timeWasted;
        private double _percentageTimeWasted;
        private long _totalOccurences;
        private double _occurencesPerHour;
        private double _averageSecondsWastedPerOccurence;
        private List<AnalyzerLogLine> _originatingVoiceCommands;
        private List<AnalyzerLogLine> _secondOriginatingVoiceCommands;

        #endregion //Fields

        #region Properties

        public string ID
        {
            get { return _id; }
        }

        public List<string> SearchText
        {
            get { return _searchText; }
        }

        public string Description
        {
            get { return _description; }
            set { _description = value; }
        }

        public List<AnalyzerLogLine> Occurences
        {
            get { return _occurences; }
        }

        public TimeSpan Duration
        {
            get { return _duration; }
        }

        public TimeSpan TimeWasted
        {
            get { return _timeWasted; }
            set { _timeWasted = value; }
        }

        public double PercentageTimeWasted
        {
            get { return _percentageTimeWasted; }
        }

        public long TotalOccurences
        {
            get { return _totalOccurences; }
        }

        public double OccurencesPerHour
        {
            get { return _occurencesPerHour; }
        }

        public double AverageSecondsWastedPerOccurence
        {
            get { return _averageSecondsWastedPerOccurence; }
        }

        public List<AnalyzerLogLine> OriginatingVoiceCommands
        {
            get { return _originatingVoiceCommands; }
        }

        public List<AnalyzerLogLine> SecondOriginatingVoiceCommands
        {
            get { return _secondOriginatingVoiceCommands; }
        }

        #endregion //Properties
    }
}
