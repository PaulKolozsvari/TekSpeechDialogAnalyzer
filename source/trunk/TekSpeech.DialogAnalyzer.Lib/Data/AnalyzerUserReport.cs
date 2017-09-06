namespace TekSpeech.DialogAnalyzer.Lib.Data
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    #endregion //Using Directives

    public class AnalyzerUserReport
    {
        #region Constructors

        public AnalyzerUserReport(
            AnalyzerLogFileCache logFileCache, 
            List<string> searchVoiceCommandText, 
            List<string> originatingVoiceCommandTextWords,
            List<string> secondOriginatingVoiceCommandTextWords)
        {
            _logFileCache = logFileCache;
            _searchVoiceCommandText = searchVoiceCommandText;
            _originatingVoiceCommandTextWords = originatingVoiceCommandTextWords;
            _secondOriginatingVoiceCommandTextWords = secondOriginatingVoiceCommandTextWords;
            GenerateReport();
        }

        #endregion //Constructors

        #region Fields

        private AnalyzerLogFileCache _logFileCache;
        private List<string> _searchVoiceCommandText;
        private List<string> _originatingVoiceCommandTextWords;
        private List<string> _secondOriginatingVoiceCommandTextWords;
        private List<AnalyzerSearchResult> _searchResults;

        private decimal _totalOccurences;
        private TimeSpan _totalDuration;
        private TimeSpan _totalTimeWasted;
        private double _totalTimeWastedPercentage;

        #endregion //Fields

        #region Properties

        public AnalyzerLogFileCache LogFileCache
        {
            get { return _logFileCache; }
        }

        public List<string> SearchVoiceCommandText
        {
            get { return _searchVoiceCommandText; }
        }

        public List<string> OriginatingVoiceCommandTextWords
        {
            get { return _originatingVoiceCommandTextWords; }
        }

        public List<string> SecondOriginatingVoiceCommandTextWords
        {
            get { return _secondOriginatingVoiceCommandTextWords; }
        }

        public List<AnalyzerSearchResult> SearchResults
        {
            get { return _searchResults; }
        }

        public decimal TotalOccurences
        {
            get { return _totalOccurences; }
        }

        public TimeSpan TotalDuration
        {
            get { return _totalDuration; }
        }

        public TimeSpan TotalTimeWasted
        {
            get { return _totalTimeWasted; }
        }

        public double TotalTimeWastedPercentage
        {
            get { return _totalTimeWastedPercentage; }
        }

        #endregion //Properties

        #region Methods

        public void GenerateReport()
        {
            _searchResults = new List<AnalyzerSearchResult>();
            foreach (AnalyzerUserLog userLog in _logFileCache.UserLogCache)
            {
                AnalyzerSearchResult result = userLog.SearchVoiceCommandOccurences(_searchVoiceCommandText, false, _originatingVoiceCommandTextWords, _secondOriginatingVoiceCommandTextWords);
                _searchResults.Add(result);
            }
            //searchResults.OrderBy(p => p.OccurencesPerHour).ToList().ForEach(p => Console.WriteLine(p.Description));
            _totalOccurences = _searchResults.Sum(p => p.TotalOccurences);

            _totalDuration = new TimeSpan();
            _searchResults.ForEach(p => _totalDuration = _totalDuration.Add(p.Duration));

            _totalTimeWasted = new TimeSpan();
            _searchResults.ForEach(p => _totalTimeWasted = _totalTimeWasted.Add(p.TimeWasted));

            _totalTimeWastedPercentage = Math.Round(((_totalTimeWasted.TotalSeconds / _totalDuration.TotalSeconds) * 100), 2);
        }

        #endregion //Methods
    }
}
