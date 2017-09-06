namespace TekSpeech.DialogAnalyzer.Lib.Data
{
    #region Using Directives

    using Figlut.Server.Toolkit.Data;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    #endregion //Using Directives

    public class AnalyzerUserLogCache : EntityCache<int, AnalyzerUserLog>
    {
        #region Constructors

        public AnalyzerUserLogCache()
        {
        }

        #endregion //Constructors

        #region Methods

        public AnalyzerUserLog GetUserLog(int userId)
        {
            if (this.Exists(userId))
            {
                return this[userId];
            }
            return null;
        }

        #endregion //Methods
    }
}