namespace TekSpeech.DialogAnalyzer.Lib.PDF
{
    #region Using Directives

    using Figlut.Server.Toolkit.Utilities;
    using PdfSharp.Drawing;
    using PdfSharp.Pdf;
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using TekSpeech.DialogAnalyzer.Lib.Configuration;
    using TekSpeech.DialogAnalyzer.Lib.Data;

    #endregion //Using Directives

    public class UserReportPdf
    {
        #region Constructors

        public UserReportPdf(
            AnalyzerUserReport userReport, 
            string title, 
            string logoFilePath,
            string waterMarkLogoFilePath,
            string customerName,
            string siteName,
            string projectOwner,
            string reportGeneratedBy,
            bool drawOriginatingVoiceCommands,
            bool drawOnlyUniqueOriginatingVoiceCommands,
            bool drawSecondOriginatingVoiceCommands,
            bool drawOnlyUniqueSecondOriginatingVoiceCommands)
        {
            _userReport = userReport;
            _title = title;
            _logoFilePath = logoFilePath;
            _waterMarkLogoFilePath = waterMarkLogoFilePath;
            _customerName = customerName;
            _siteName = siteName;
            _projectOwner = projectOwner;
            _reportGeneratedBy = reportGeneratedBy;

            _drawOriginatingVoiceCommands = drawOriginatingVoiceCommands;
            _drawOnlyUniqueOriginatingVoiceCommands = drawOnlyUniqueOriginatingVoiceCommands;
            _drawSecondOriginatingVoiceCommands = drawSecondOriginatingVoiceCommands;
            _drawOnlyUniqueSecondOriginatingVoiceCommands = drawOnlyUniqueSecondOriginatingVoiceCommands;
            
            _document = new PdfDocument();
            _document.Info.Title = _title;
            DrawDocument();
        }

        #endregion //Constructors

        #region Constants

        protected const int MARGIN_LENGTH = 30;
        protected const int TOP_MARGIN = 60;
        protected const string NEW_LINE = "\r\n";
        protected const int WORD_WRAP_MAX_LENGTH = 130;
        protected const int PAGE_BAR_WIDTH = 12;

        #endregion //Constants

        #region Fields

        private AnalyzerUserReport _userReport;
        private string _title;
        private string _logoFilePath;
        private string _waterMarkLogoFilePath;

        private string _customerName;
        private string _siteName;
        private string _projectOwner;
        private string _reportGeneratedBy;

        private bool _drawOriginatingVoiceCommands;
        private bool _drawOnlyUniqueOriginatingVoiceCommands;
        private bool _drawSecondOriginatingVoiceCommands;
        private bool _drawOnlyUniqueSecondOriginatingVoiceCommands;

        protected PdfDocument _document;
        protected PdfPage _currentPage;
        protected XGraphics _currentGraphics;
        protected Point _position;

        protected int _workingWidth;
        protected int _workingHeight;
        protected int _endOfPageHeight;

        protected byte[] _pdfBytes;

        private XFont _productNameFont = new XFont("Verdana", 30, XFontStyle.Bold | XFontStyle.Italic);
        private XFont _headingFont = new XFont("Verdana", 15, XFontStyle.Bold | XFontStyle.Italic);
        private XFont _tableHeadingFont = new XFont("Verdana", 10, XFontStyle.Bold | XFontStyle.Italic);
        private XFont _fieldFont = new XFont("Verdana", 7, XFontStyle.Bold);
        private XFont _detailsFont = new XFont("Verdana", 6, XFontStyle.Regular);

        #endregion //Fields

        #region Properties

        public AnalyzerUserReport UserReport
        {
            get { return _userReport; }
        }

        public string Title
        {
            get { return _title; }
        }

        #endregion //Properties

        #region Methods

        private void DrawDocument()
        {
            NewPageGraphics();
            DrawFrontPage();
            NewPageGraphics();
            DrawUsersAnalyzed();
            NewPageGraphics();
            DrawFilesAnalyzed();
            if (_drawOriginatingVoiceCommands &&
                (_userReport.OriginatingVoiceCommandTextWords != null) &&
                (_userReport.OriginatingVoiceCommandTextWords.Count > 0))
            {
                NewPageGraphics();
                DrawOriginatingVoiceCommands();
            }
            if (_drawSecondOriginatingVoiceCommands &&
                (_userReport.SecondOriginatingVoiceCommandTextWords != null) && 
                (_userReport.SecondOriginatingVoiceCommandTextWords.Count > 0))
            {
                NewPageGraphics();
                DrawSecondOriginatingVoiceCommands();
            }
            EndPdfDocument();
        }

        private void NewPageGraphics()
        {
            _currentPage = _document.AddPage();
            _currentGraphics = XGraphics.FromPdfPage(_currentPage);
            _position = new Point(MARGIN_LENGTH, TOP_MARGIN);
            _workingWidth = Convert.ToInt32(_currentPage.Width.Value - (2 * MARGIN_LENGTH));
            _workingHeight = Convert.ToInt32(_currentPage.Height.Value - (2 * TOP_MARGIN));
            _endOfPageHeight = Convert.ToInt32(_currentPage.Height - TOP_MARGIN);

            FileSystemHelper.ValidateFileExists(_waterMarkLogoFilePath);
            XImage logo = XImage.FromFile(_waterMarkLogoFilePath);
            int resizedLogoWidth = Convert.ToInt32(logo.PixelWidth * 0.5);
            int resizedLogoHeight = Convert.ToInt32(logo.PixelHeight * 0.5);
            int x = Convert.ToInt32(_currentPage.Width.Value - MARGIN_LENGTH) - resizedLogoWidth;
            int y = Convert.ToInt32(TOP_MARGIN / 3);
            Rectangle imageArea = new Rectangle(x, y, resizedLogoWidth, resizedLogoHeight);
            _currentGraphics.DrawImage(logo, imageArea);
        }

        public bool IsEndOfPage(bool createNewPageIfEndOfPage)
        {
            if ((_position.Y + _detailsFont.Height) >= _endOfPageHeight)
            {
                if (createNewPageIfEndOfPage)
                {
                    NewPageGraphics();
                }
                return true;
            }
            return false;
        }

        private void EndPdfDocument()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                _document.Save(ms);
                _pdfBytes = ms.ToArray();
            }
            _document.Close();
            _document.Dispose();
        }

        public void SaveToFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            using (BinaryWriter writer = new BinaryWriter(File.OpenWrite(filePath)))
            {
                writer.Write(_pdfBytes);
                writer.Flush();
                writer.Close();
            }
        }

        private void DrawFrontPage()
        {
            FileSystemHelper.ValidateFileExists(_logoFilePath);
            XImage logo = XImage.FromFile(_logoFilePath);
            int resizedLogoWidth = Convert.ToInt32(logo.PixelWidth * 0.75);
            int resizedLogoHeight = Convert.ToInt32(logo.PixelHeight * 0.75);
            Rectangle imageArea = new Rectangle(_position.X, _position.Y, resizedLogoWidth, resizedLogoHeight);
            _currentGraphics.DrawImage(logo, imageArea);
            _position.X = MARGIN_LENGTH;
            _position.Y = resizedLogoHeight + TOP_MARGIN;

            _currentGraphics.DrawString("TekSpeech Pro", _productNameFont, XBrushes.SteelBlue, new PointF(300, _position.Y - (resizedLogoHeight / 2)));

            _currentGraphics.DrawString(_title, _headingFont, XBrushes.SteelBlue, new PointF(220, _position.Y));
            _position.Y += (_headingFont.Height * 2);

            DrawpProjectInfo();
            DrawSearchCriteria();
            DrawSummary();
            DrawLogFileDates();
        }

        private void DrawpProjectInfo()
        {
            _currentGraphics.DrawString("PROJECT INFO", _tableHeadingFont, XBrushes.DarkOrange, new PointF(220, _position.Y));
            _position.Y += (_tableHeadingFont.Height * 2);

            _currentGraphics.DrawString("CUSTOMER NAME:", _fieldFont, XBrushes.Black, _position);
            _currentGraphics.DrawString(string.Format("{0}", _customerName), _fieldFont, XBrushes.SteelBlue, new PointF(220, _position.Y));
            _position.Y += _fieldFont.Height;
            _currentGraphics.DrawString("SPEECH SITE NAME:", _fieldFont, XBrushes.Black, _position);
            _currentGraphics.DrawString(string.Format("{0}", _siteName), _fieldFont, XBrushes.SteelBlue, new PointF(220, _position.Y));
            _position.Y += _fieldFont.Height;
            _currentGraphics.DrawString("PROJECT OWNER:", _fieldFont, XBrushes.Black, _position);
            _currentGraphics.DrawString(string.Format("{0}", _projectOwner), _fieldFont, XBrushes.SteelBlue, new PointF(220, _position.Y));
            _position.Y += _fieldFont.Height;
            _currentGraphics.DrawString("REPORT GENERATED BY:", _fieldFont, XBrushes.Black, _position);
            _currentGraphics.DrawString(string.Format("{0}", _reportGeneratedBy), _fieldFont, XBrushes.SteelBlue, new PointF(220, _position.Y));
            _position.Y += _fieldFont.Height;
            _currentGraphics.DrawString("DATE GENERATED:", _fieldFont, XBrushes.Black, _position);
            DateTime currentDate = DateTime.Now;
            _currentGraphics.DrawString(string.Format("{0}/{1}/{2}", currentDate.Year, currentDate.Month, currentDate.Day), _fieldFont, XBrushes.SteelBlue, new PointF(220, _position.Y));
            _position.Y += (_fieldFont.Height * 4);
        }

        private void DrawSearchCriteria()
        {
            _currentGraphics.DrawString("SEARCH CRITERIA", _tableHeadingFont, XBrushes.DarkOrange, new PointF(220, _position.Y));
            _position.Y += (_tableHeadingFont.Height * 2);

            _currentGraphics.DrawString("VOICE COMMAND TEXT SEARCHED:", _fieldFont, XBrushes.Black, _position);
            StringBuilder searchVoiceCommandText = new StringBuilder();
            _userReport.SearchVoiceCommandText.ForEach(p => searchVoiceCommandText.Append(string.Format("'{0}', ", p)));
            searchVoiceCommandText.Remove(searchVoiceCommandText.Length - 2, 2);
            _currentGraphics.DrawString(searchVoiceCommandText.ToString(), _fieldFont, XBrushes.SteelBlue, new PointF(220, _position.Y));
            _position.Y += _fieldFont.Height;

            _currentGraphics.DrawString("INITIATING VOICE COMMAND KEYWORDS:", _fieldFont, XBrushes.Black, _position);
            StringBuilder originatingVoiceCommandKeyWords = new StringBuilder();
            _userReport.OriginatingVoiceCommandTextWords.ForEach(p => originatingVoiceCommandKeyWords.Append(string.Format("'{0}', ", p)));
            originatingVoiceCommandKeyWords.Remove(originatingVoiceCommandKeyWords.Length - 2, 2);
            _currentGraphics.DrawString(originatingVoiceCommandKeyWords.ToString(), _fieldFont, XBrushes.SteelBlue, new PointF(220, _position.Y));
            _position.Y += _fieldFont.Height;

            if ((_userReport.SecondOriginatingVoiceCommandTextWords != null) && (_userReport.SecondOriginatingVoiceCommandTextWords.Count > 0))
            {
                _currentGraphics.DrawString("PREVIOUS VOICE COMMAND KEYWORDS:", _fieldFont, XBrushes.Black, _position);
                StringBuilder secondOriginatingVoiceCommandKeyWords = new StringBuilder();
                _userReport.SecondOriginatingVoiceCommandTextWords.ForEach(p => secondOriginatingVoiceCommandKeyWords.Append(string.Format("'{0}', ", p)));
                secondOriginatingVoiceCommandKeyWords.Remove(secondOriginatingVoiceCommandKeyWords.Length - 2, 2);
                _currentGraphics.DrawString(secondOriginatingVoiceCommandKeyWords.ToString(), _fieldFont, XBrushes.SteelBlue, new PointF(220, _position.Y));
                _position.Y += (_fieldFont.Height * 4);
            }
            else
            {
                _position.Y += (_fieldFont.Height * 3);
            }
        }

        private void DrawLogFileDates()
        {
            _currentGraphics.DrawString("ANALYZER LOG FILE DATES", _tableHeadingFont, XBrushes.DarkOrange, new PointF(220, _position.Y));
            _position.Y += (_tableHeadingFont.Height * 2);

            foreach (DateTime date in _userReport.LogFileCache.FileDates.Values)
            {
                _currentGraphics.DrawString(string.Format("{0}/{1}/{2}", date.Year, date.Month, date.Day), _fieldFont, XBrushes.SteelBlue, new PointF(220, _position.Y));
                if (!IsEndOfPage(true))
                {
                    _position.Y += _fieldFont.Height;
                }
            }
            _position.Y += (_fieldFont.Height * 3);
        }

        private void DrawSummary()
        {
            _currentGraphics.DrawString("SUMMARY", _tableHeadingFont, XBrushes.DarkOrange, new PointF(220, _position.Y));
            _position.Y += (_fieldFont.Height * 2);

            _currentGraphics.DrawString("TOTAL LOG FILES (ALL USERS):", _fieldFont, XBrushes.Black, _position);
            _currentGraphics.DrawString(_userReport.LogFileCache.Count.ToString(), _fieldFont, XBrushes.SteelBlue, new PointF(220, _position.Y));
            _position.Y += _fieldFont.Height;
            _currentGraphics.DrawString("TOTAL LOG DURATION (ALL USERS):", _fieldFont, XBrushes.Black, _position);
            _currentGraphics.DrawString(string.Format("{0} (Days:Hours:Minutes:Seconds:Milliseconds)", _userReport.TotalDuration), _fieldFont, XBrushes.SteelBlue, new PointF(220, _position.Y));
            _position.Y += _fieldFont.Height;
            _currentGraphics.DrawString("TOTAL COUNT (ALL USERS):", _fieldFont, XBrushes.Black, _position);
            _currentGraphics.DrawString(string.Format("{0}", _userReport.TotalOccurences), _fieldFont, XBrushes.SteelBlue, new PointF(220, _position.Y));
            _position.Y += _fieldFont.Height;
            _currentGraphics.DrawString("TOTAL TIME WASTED (ALL USERS):", _fieldFont, XBrushes.Black, _position);
            _currentGraphics.DrawString(string.Format("{0} (Days:Hours:Minutes:Seconds:Milliseconds)", _userReport.TotalTimeWasted), _fieldFont, XBrushes.SteelBlue, new PointF(220, _position.Y));
            _position.Y += _fieldFont.Height;
            _currentGraphics.DrawString("TOTAL % TIME WASTED (ALL USERS):", _fieldFont, XBrushes.Black, _position);
            _currentGraphics.DrawString(string.Format("{0}%", _userReport.TotalTimeWastedPercentage), _fieldFont, XBrushes.SteelBlue, new PointF(220, _position.Y));
            _position.Y += (_fieldFont.Height * 4);
        }

        private void DrawUsersAnalyzed()
        {
            _currentGraphics.DrawString("USERS ANALYZED", _tableHeadingFont, XBrushes.DarkOrange, new PointF(220, _position.Y));
            _position.Y += (_fieldFont.Height * 2);

            _currentGraphics.DrawString("USER NAME", _fieldFont, XBrushes.Black, new PointF(UserReportFieldPosition.USER_NAME_X, _position.Y));
            _currentGraphics.DrawString("USER ID", _fieldFont, XBrushes.SteelBlue, new PointF(UserReportFieldPosition.USER_ID_X, _position.Y));
            _currentGraphics.DrawString("COUNT", _fieldFont, XBrushes.Black, new PointF(UserReportFieldPosition.TOTAL_OCCURENCES_X, _position.Y));
            _currentGraphics.DrawString("COUNT/HR", _fieldFont, XBrushes.SteelBlue, new PointF(UserReportFieldPosition.OCCURENCES_PER_HOUR_X, _position.Y));
            _currentGraphics.DrawString("TIME WASTED", _fieldFont, XBrushes.Black, new PointF(UserReportFieldPosition.TIME_WASTED_X, _position.Y));
            _currentGraphics.DrawString("DURATION", _fieldFont, XBrushes.SteelBlue, new PointF(UserReportFieldPosition.DURATION_X, _position.Y));
            _currentGraphics.DrawString("% TIME WASTED", _fieldFont, XBrushes.Black, new PointF(UserReportFieldPosition.PERCENTAGE_TIME_WASTED, _position.Y));
            _currentGraphics.DrawString("AVG. TIME WASTED/COUNT", _fieldFont, XBrushes.SteelBlue, new PointF(UserReportFieldPosition.AVERAGE_SECONDS_WASTED_PER_OCCURENCE, _position.Y));
            _position.Y += (_fieldFont.Height * 2);
            foreach (AnalyzerSearchResult r in _userReport.SearchResults.OrderBy(p => p.OccurencesPerHour).ToList())
            {
                int userId = Convert.ToInt32(r.ID);
                User user = UserDictionary.Instance[userId];
                _currentGraphics.DrawString(user.UserName, _detailsFont, XBrushes.Black, new PointF(UserReportFieldPosition.USER_NAME_X, _position.Y));
                _currentGraphics.DrawString(user.UserId.ToString(), _detailsFont, XBrushes.SteelBlue, new PointF(UserReportFieldPosition.USER_ID_X, _position.Y));
                _currentGraphics.DrawString(r.TotalOccurences.ToString(), _detailsFont, XBrushes.Black, new PointF(UserReportFieldPosition.TOTAL_OCCURENCES_X, _position.Y));
                _currentGraphics.DrawString(r.OccurencesPerHour.ToString(), _detailsFont, XBrushes.SteelBlue, new PointF(UserReportFieldPosition.OCCURENCES_PER_HOUR_X, _position.Y));
                _currentGraphics.DrawString(r.TimeWasted.ToString(), _detailsFont, XBrushes.Black, new PointF(UserReportFieldPosition.TIME_WASTED_X, _position.Y));
                _currentGraphics.DrawString(r.Duration.ToString(), _detailsFont, XBrushes.SteelBlue, new PointF(UserReportFieldPosition.DURATION_X, _position.Y));
                _currentGraphics.DrawString(string.Format("{0}%", r.PercentageTimeWasted.ToString()), _detailsFont, XBrushes.Black, new PointF(UserReportFieldPosition.PERCENTAGE_TIME_WASTED, _position.Y));
                _currentGraphics.DrawString(string.Format("{0} sec", r.AverageSecondsWastedPerOccurence.ToString()), _detailsFont, XBrushes.SteelBlue, new PointF(UserReportFieldPosition.AVERAGE_SECONDS_WASTED_PER_OCCURENCE, _position.Y));
                if (!IsEndOfPage(true))
                {
                    _position.Y += _detailsFont.Height;
                }
            }
            _position.Y += (_fieldFont.Height * 2);
        }

        private void DrawFilesAnalyzed()
        {
            _currentGraphics.DrawString("FILES ANALYZED", _tableHeadingFont, XBrushes.DarkOrange, new PointF(220, _position.Y));
            _position.Y += (_fieldFont.Height * 2);

            _currentGraphics.DrawString("FILE NAME", _fieldFont, XBrushes.Black, new PointF(FileReportFieldPosition.FILE_NAME_X, _position.Y));
            _currentGraphics.DrawString("START DATE", _fieldFont, XBrushes.SteelBlue, new PointF(FileReportFieldPosition.START_DATE_TIME_X, _position.Y));
            _currentGraphics.DrawString("END DATE", _fieldFont, XBrushes.Black, new PointF(FileReportFieldPosition.END_DATE_TIME_X, _position.Y));
            _currentGraphics.DrawString("DURATION", _fieldFont, XBrushes.SteelBlue, new PointF(FileReportFieldPosition.DURATION_X, _position.Y));
            _currentGraphics.DrawString("LINE COUNT", _fieldFont, XBrushes.Black, new PointF(FileReportFieldPosition.TOTAL_LINE_COUNT_X, _position.Y));
            _currentGraphics.DrawString("USER NAME", _fieldFont, XBrushes.SteelBlue, new PointF(FileReportFieldPosition.USER_NAME_X, _position.Y));
            _currentGraphics.DrawString("USER ID", _fieldFont, XBrushes.Black, new PointF(FileReportFieldPosition.USER_ID_x, _position.Y));
            _position.Y += (_fieldFont.Height * 2);
            foreach (AnalyzerLogFile file in _userReport.LogFileCache.OrderBy(p => p.StartDateTime))
            {
                int userId = Convert.ToInt32(file.UserId);
                User user = UserDictionary.Instance[userId];
                _currentGraphics.DrawString(file.FileName, _detailsFont, XBrushes.Black, new PointF(FileReportFieldPosition.FILE_NAME_X, _position.Y));
                _currentGraphics.DrawString(file.StartDateTime.ToString(), _detailsFont, XBrushes.SteelBlue, new PointF(FileReportFieldPosition.START_DATE_TIME_X, _position.Y));
                _currentGraphics.DrawString(file.EndDateTime.ToString().ToString(), _detailsFont, XBrushes.Black, new PointF(FileReportFieldPosition.END_DATE_TIME_X, _position.Y));
                _currentGraphics.DrawString(file.Duration.ToString(), _detailsFont, XBrushes.SteelBlue, new PointF(FileReportFieldPosition.DURATION_X, _position.Y));
                _currentGraphics.DrawString(file.TotalLineCount.ToString(), _detailsFont, XBrushes.Black, new PointF(FileReportFieldPosition.TOTAL_LINE_COUNT_X, _position.Y));
                _currentGraphics.DrawString(user.UserName.ToString(), _detailsFont, XBrushes.SteelBlue, new PointF(FileReportFieldPosition.USER_NAME_X, _position.Y));
                _currentGraphics.DrawString(user.UserId.ToString(), _detailsFont, XBrushes.Black, new PointF(FileReportFieldPosition.USER_ID_x, _position.Y));
                if (!IsEndOfPage(true))
                {
                    _position.Y += _detailsFont.Height;
                }
            }
        }

        private void DrawOriginatingVoiceCommands()
        {
            _currentGraphics.DrawString("INITIATING VOICE COMMANDS", _tableHeadingFont, XBrushes.DarkOrange, new PointF(220, _position.Y));
            _position.Y += (_fieldFont.Height * 2);

            List<AnalyzerLogLine> originatingVoiceCommands = new List<AnalyzerLogLine>();
            Dictionary<string, long> uniqueOriginatingVoiceCommands = new Dictionary<string, long>();
            foreach (AnalyzerSearchResult searchResult in _userReport.SearchResults)
            {
                foreach (AnalyzerLogLine line in searchResult.OriginatingVoiceCommands)
                {
                    if (_drawOnlyUniqueOriginatingVoiceCommands)
                    {
                        if (!uniqueOriginatingVoiceCommands.ContainsKey(line.VoiceCommand))
                        {
                            uniqueOriginatingVoiceCommands.Add(line.VoiceCommand, 1);
                        }
                        else
                        {
                            uniqueOriginatingVoiceCommands[line.VoiceCommand] += 1;
                        }
                    }
                    else
                    {
                        originatingVoiceCommands.Add(line);
                    }
                }
            }
            if (_drawOnlyUniqueOriginatingVoiceCommands)
            {
                _currentGraphics.DrawString("COUNT", _fieldFont, XBrushes.Black, new PointF(VoiceCommandFieldPosition.COUNT_X, _position.Y));
            }
            _currentGraphics.DrawString("INITIATING VOICE COMMAND", _fieldFont, XBrushes.SteelBlue, new PointF(VoiceCommandFieldPosition.VOICE_COMMAND_X, _position.Y));
            _position.Y += (_fieldFont.Height * 2);

            if (_drawOnlyUniqueOriginatingVoiceCommands)
            {
                List<VoiceCommandCounter> voiceCommands = new List<VoiceCommandCounter>();
                uniqueOriginatingVoiceCommands.ToList().ForEach(p => voiceCommands.Add(new VoiceCommandCounter() { VoiceCommand = p.Key, Count = p.Value }));
                foreach (VoiceCommandCounter cmd in voiceCommands.OrderByDescending(p => p.Count))
                {
                    _currentGraphics.DrawString(cmd.Count.ToString(), _detailsFont, XBrushes.Black, new PointF(VoiceCommandFieldPosition.COUNT_X, _position.Y));
                    _currentGraphics.DrawString(cmd.VoiceCommand, _detailsFont, XBrushes.SteelBlue, new PointF(VoiceCommandFieldPosition.VOICE_COMMAND_X, _position.Y));
                    if (!IsEndOfPage(true))
                    {
                        _position.Y += _detailsFont.Height;
                    }
                }
            }
            else
            {
                foreach (AnalyzerLogLine cmd in originatingVoiceCommands)
                {
                    _currentGraphics.DrawString(cmd.VoiceCommand, _detailsFont, XBrushes.SteelBlue, new PointF(VoiceCommandFieldPosition.VOICE_COMMAND_X, _position.Y));
                    if (!IsEndOfPage(true))
                    {
                        _position.Y += _detailsFont.Height;
                    }
                }
            }
        }

        private void DrawSecondOriginatingVoiceCommands()
        {
            _currentGraphics.DrawString("PREVIOUS VOICE COMMANDS", _tableHeadingFont, XBrushes.DarkOrange, new PointF(220, _position.Y));
            _position.Y += (_fieldFont.Height * 2);
            List<AnalyzerLogLine> secondOriginatingVoiceCommands = new List<AnalyzerLogLine>();
            Dictionary<string, long> uniqueSecondOriginatingVoiceCommands = new Dictionary<string, long>();
            foreach (AnalyzerSearchResult searchResult in _userReport.SearchResults)
            {
                foreach (AnalyzerLogLine line in searchResult.SecondOriginatingVoiceCommands)
                {
                    if (_drawOnlyUniqueSecondOriginatingVoiceCommands)
                    {
                        if (!uniqueSecondOriginatingVoiceCommands.ContainsKey(line.VoiceCommand))
                        {
                            uniqueSecondOriginatingVoiceCommands.Add(line.VoiceCommand, 1);
                        }
                        else
                        {
                            uniqueSecondOriginatingVoiceCommands[line.VoiceCommand] += 1;
                        }
                    }
                    else
                    {
                        secondOriginatingVoiceCommands.Add(line);
                    }
                }
            }
            if (_drawOnlyUniqueSecondOriginatingVoiceCommands)
            {
                _currentGraphics.DrawString("COUNT", _fieldFont, XBrushes.Black, new PointF(VoiceCommandFieldPosition.COUNT_X, _position.Y));
            }
            _currentGraphics.DrawString("OTHER VOICE COMMAND", _fieldFont, XBrushes.SteelBlue, new PointF(VoiceCommandFieldPosition.VOICE_COMMAND_X, _position.Y));
            _position.Y += (_fieldFont.Height * 2);

            if (_drawOnlyUniqueSecondOriginatingVoiceCommands)
            {
                List<VoiceCommandCounter> voiceCommands = new List<VoiceCommandCounter>();
                uniqueSecondOriginatingVoiceCommands.ToList().ForEach(p => voiceCommands.Add(new VoiceCommandCounter() { VoiceCommand = p.Key, Count = p.Value }));
                foreach (VoiceCommandCounter cmd in voiceCommands.OrderByDescending(p => p.Count))
                {
                    _currentGraphics.DrawString(cmd.Count.ToString(), _detailsFont, XBrushes.Black, new PointF(VoiceCommandFieldPosition.COUNT_X, _position.Y));
                    _currentGraphics.DrawString(cmd.VoiceCommand, _detailsFont, XBrushes.SteelBlue, new PointF(VoiceCommandFieldPosition.VOICE_COMMAND_X, _position.Y));
                    if (!IsEndOfPage(true))
                    {
                        _position.Y += _detailsFont.Height;
                    }
                }
            }
            else
            {
                foreach (AnalyzerLogLine cmd in secondOriginatingVoiceCommands)
                {
                    _currentGraphics.DrawString(cmd.VoiceCommand, _detailsFont, XBrushes.SteelBlue, new PointF(VoiceCommandFieldPosition.VOICE_COMMAND_X, _position.Y));
                    if (!IsEndOfPage(true))
                    {
                        _position.Y += _detailsFont.Height;
                    }
                }
            }
        }

        #endregion //Methods
    }
}