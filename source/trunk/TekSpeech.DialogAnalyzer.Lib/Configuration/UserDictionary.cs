namespace TekSpeech.DialogAnalyzer.Lib.Configuration
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

    public class UserDictionary : EntityCache<int, User>
    {
        #region Singleton Setup

        private static UserDictionary _instance;

        public static UserDictionary Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new UserDictionary();
                }
                return _instance;
            }
        }

        #endregion //Singleton Setup

        #region Constants

        public const string FILE_NAME = "UserDictionary.xml";

        #endregion //Constants

        #region Fields

        private string _filePath;

        #endregion //Fields

        #region Properties

        public string FilePath
        {
            get
            {
                if (string.IsNullOrEmpty(_filePath))
                {
                    _filePath = Path.Combine(Information.GetExecutingDirectory(), FILE_NAME);
                }
                return _filePath;
            }
        }

        #endregion //Properties
    }
}
