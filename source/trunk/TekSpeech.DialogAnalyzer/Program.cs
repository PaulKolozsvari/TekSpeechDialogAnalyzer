namespace TekSpeech.DialogAnalyzer
{
    #region Using Directives

    using Figlut.Server.Toolkit.Utilities;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows.Forms;
    using TekSpeech.DialogAnalyzer.Lib.Configuration;
    using TekSpeech.DialogAnalyzer.Lib.Data;
    using TekSpeech.DialogAnalyzer.Lib.PDF;

    #endregion //Using Directives

    static class Program
    {
        #region Fields

        private static MainForm _mainForm;

        #endregion //Fields

        #region Methods

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            try
            {
                UserDictionary.Instance.LoadFromFile(null);
                User user = UserDictionary.Instance[620];

                if (args.Length == 1 && args[0].Trim().ToLower() == "/console")
                {
                    AnalyzerLogFileCache logFileCache = new AnalyzerLogFileCache(@"C:\Docs\Datasmith\DatasmithDotNet\DigisticsWarehousing\Digistics.Mobile\code\Digistics.Speech\AnalyzerLogs", true);
                    //foreach (AnalyzerLogFile file in logFileCache)
                    //{
                    //    file.SearchVoiceCommandOccurences(new List<string>() { "Incorrect check digits." }, false);
                    //}
                    AnalyzerUserReport report = new AnalyzerUserReport(
                        logFileCache,
                        new List<string>() { "Incorrect check digits." },
                        new List<string>() { "Go to", "bin" },
                        null);

                    //AnalyzerUserReport report = new AnalyzerUserReport(
                    //    logFileCache,
                    //    new List<string>() { "Batch too short to check." },
                    //    new List<string>() { "Batch code?" });

                    //AnalyzerUserReport report = new AnalyzerUserReport(
                    //    logFileCache,
                    //    new List<string>() { "Invalid Batch" },
                    //    new List<string>() { "Batch code?" },
                    //    new List<string>() { "Go to", "bin" });

                    //AnalyzerUserReport report = new AnalyzerUserReport(
                    //    logFileCache,
                    //    new List<string>() { "Invalid Batch", "Batch too short to check." },
                    //    new List<string>() { "Batch code?" });

                    string customerName = "Digistics";
                    string siteName = "KFC Pretoria";
                    string projectOwner = "Ken Scott";
                    string reportGeneratedBy = "Paul Kolozsvari";
                    DateTime currentDate = DateTime.Now;
                    string outputFileName = string.Format(
                        "DatasmithSpeechAnalyzerReport_{0}_{1}_{2}-{3}-{4}.pdf",
                        customerName,
                        siteName,
                        currentDate.Year,
                        currentDate.Month,
                        currentDate.Day);

                    string outputFilePath = Path.Combine(@"C:\Docs\Datasmith\DatasmithDotNet\TekSpeech.DialogAnalyzer\OutputReport", outputFileName);
                    string logoFilePath = Path.Combine(Information.GetExecutingDirectory(), "TekSpeechPro_324x324.jpg");
                    string waterMarkLogoFilePath = Path.Combine(Information.GetExecutingDirectory(), "DatasmithLogo.png");
                    UserReportPdf pdf = new UserReportPdf(
                        report,
                        "Speech Analyzer Logs Report",
                        logoFilePath,
                        waterMarkLogoFilePath,
                        customerName,
                        siteName,
                        projectOwner,
                        reportGeneratedBy,
                        true,
                        true,
                        true,
                        true);
                    pdf.SaveToFile(outputFilePath);
                }
                else
                {
                    Application.ThreadException += Application_ThreadException;
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    _mainForm = new MainForm();
                    Application.Run(_mainForm);
                }
                //Console.ReadLine();
            }
            catch (Exception ex)
            {
                ExceptionHandler.HandleException(ex);
                throw;
            }
        }

        static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            if (!ExceptionHandler.HandleException(e.Exception))
            {
                if (_mainForm != null)
                {
                    _mainForm.ForceClose = true;
                    Application.Exit();
                }
            }
        }

        #endregion //Methods
    }
}
