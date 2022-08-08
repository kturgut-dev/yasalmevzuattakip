using KYS.YasalMevzuatTakip.Helpers;
using log4net;
using log4net.Appender;
using log4net.Repository.Hierarchy;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace KYS.YasalMevzuatTakip.Helper
{
    public class Loog4NetHelper
    {
        private log4net.ILog _Log;
        public Models.ProjectSetting _projectSetting { get; private set; }
        public Loog4NetHelper(Type type)
        {
            log4net.LogicalThreadContext.Properties["ProjectName"] = Assembly.GetCallingAssembly().GetName().Name;
            _Log = log4net.LogManager.GetLogger(type);
            _projectSetting = ConfigurationManagerHelper.GetProjectSetting();
        }

        public void Error(string exMsg)
        {
            if (_projectSetting.LoggingIsOn)
                _Log.Error(string.Format("{0}", exMsg));
        }

        public void Info(string exMsg)
        {
            _Log.Info(string.Format("{0}", exMsg));
        }

        public void Fatal(string exMsg)
        {
            if (_projectSetting.LoggingIsOn)
                _Log.Fatal(string.Format("{0}", exMsg));
        }

        public void Warn(string msg)
        {
            if (_projectSetting.LoggingIsOn)
                _Log.Warn(string.Format("{0}", msg));
        }
    }

}
