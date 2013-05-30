using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnvDTE;
using EnvDTE80;

namespace Blacksand
{
    class Logger
    {
        public static void WriteMessage(string message)
        {
            if (_outPane == null)
                throw new System.Exception("Logger is NULL");

            _outPane.OutputString(message + "\n");
        }

        public static bool Initialize(DTE2 application)
        {
            if (_outPane == null)
                _outPane = application.ToolWindows.OutputWindow.OutputWindowPanes.Add("CodingHelper");

            return _outPane != null;
        }

        private static OutputWindowPane _outPane;
    }
}
