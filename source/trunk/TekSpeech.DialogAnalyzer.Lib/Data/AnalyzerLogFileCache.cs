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

    public class AnalyzerLogFileCache : EntityCache<string, AnalyzerLogFile>
    {
        #region Constructors

        public AnalyzerLogFileCache(string searchDirectory, bool recursiveFileSearch)
        {
            _searchDirectory = searchDirectory;
            _recursiveFileSearch = recursiveFileSearch;
            _userLogCache = new AnalyzerUserLogCache();
            BuildCache();
        }

        #endregion //Constructors

        #region Constants

        public const string LOG_FILE_EXTENSION = "*.log";

        #endregion //Constants

        #region Fields

        private string _searchDirectory;
        private bool _recursiveFileSearch;
        private long _totalFileCount;
        private AnalyzerUserLogCache _userLogCache;

        private TimeSpan _totalDuration;
        private int _totalUsers;
        private List<int> _userIds;
        private Dictionary<string, DateTime> _fileDates;

        #endregion //Fields

        #region Properties

        public string SearchDirectory
        {
            get { return _searchDirectory; }
        }

        public bool RecursiveFileSearch
        {
            get { return _recursiveFileSearch; }
        }

        public long TotalFileCount
        {
            get { return _totalFileCount; }
        }

        public AnalyzerUserLogCache UserLogCache
        {
            get { return _userLogCache; }
        }

        public int TotalUsers
        {
            get { return _totalUsers; }
        }

        public List<int> UserIds
        {
            get { return _userIds; }
        }

        public Dictionary<string, DateTime> FileDates
        {
            get { return _fileDates; }
        }

        #endregion //Properties

        #region Methods

        private void BuildCache()
        {
            FileSystemHelper.ValidateDirectoryExists(_searchDirectory);
            List<string> filePaths = null;
            if (_recursiveFileSearch)
            {
                filePaths = Directory.GetFiles(_searchDirectory, LOG_FILE_EXTENSION, SearchOption.AllDirectories).ToList();
            }
            else
            {
                filePaths = Directory.GetFiles(_searchDirectory, LOG_FILE_EXTENSION, SearchOption.TopDirectoryOnly).ToList();
            }
            _fileDates = new Dictionary<string, DateTime>();
            foreach (string f in filePaths)
            {
                AnalyzerLogFile file = new AnalyzerLogFile(f);
                string startDateTimeKey = string.Format("{0}/{1}/{2}", file.StartDateTime.Year, file.StartDateTime.Month, file.StartDateTime.Day);
                string endDateTimeKey = string.Format("{0}/{1}/{2}", file.EndDateTime.Year, file.EndDateTime.Month, file.EndDateTime.Day);
                if (!_fileDates.ContainsKey(startDateTimeKey))
                {
                    _fileDates.Add(startDateTimeKey, file.StartDateTime);
                }
                else if (!_fileDates.ContainsKey(endDateTimeKey))
                {
                    _fileDates.Add(endDateTimeKey, file.EndDateTime);
                }
                if (this.Exists(file.FileName))
                {
                    AnalyzerLogFile otherFile = this[file.FileName];
                    throw new ArgumentException(string.Format("More than one log file with the same name: {0} and {1}", file.FilePath, otherFile.FilePath));
                }
                this.Add(file.FileName, file);
            }
            Profile();
        }

        private void Profile()
        {
            _totalDuration = new TimeSpan(0, 0, 0);
            _totalFileCount = _entities.Count;
            foreach (AnalyzerLogFile file in this)
            {
                _totalDuration = _totalDuration.Add(file.Duration);
                if (file.UserId == 0)
                {
                    throw new NullReferenceException(string.Format("User ID not set on file: {0}", file.FilePath));
                }
                AnalyzerUserLog userLog = _userLogCache.GetUserLog(file.UserId);
                if (userLog == null)
                {
                    userLog = new AnalyzerUserLog(file.UserId);
                    _userLogCache.Add(file.UserId, userLog);
                }
                userLog.TotalDuration = userLog.TotalDuration.Add(file.Duration);
                int fileLineCount = 1;
                foreach (AnalyzerLogLine line in file)
                {
                    if (file.UserId != line.UserId)
                    {
                        throw new ArgumentException(string.Format(
                            "Expected file {0} to have entries only for user {1}, but entries for user {2} also found.",
                            file.FilePath,
                            file.UserId,
                            line.UserId));
                    }
                    string userLogEntryId = string.Format("{0}-{1}-{2}", Guid.NewGuid().ToString(), fileLineCount, line.ID);
                    userLog.Add(line);
                    fileLineCount++;
                }
            }
            _totalUsers = _userLogCache.Count;
            _userIds = (from u in _userLogCache
                        select u.UserId).ToList<int>();
        }

        #endregion //Methods
    }
}
