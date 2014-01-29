using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnvDTE;
using EnvDTE80;

namespace Blacksand
{
    abstract class EncodingConvertCommand : HelperCommand
    {
        protected ChineseConverter _converter = ChineseConverter.Get();

        public EncodingConvertCommand(string name, string text, string description)
            : base(name, text, description)
        {}

        abstract protected string Convert(string str);
            
        public override object Execute(DTE2 application, AddIn addin, object varIn)
        {
            Document theDoc = application.ActiveDocument;

            if (theDoc != null)
            {
                try
                {
                    application.UndoContext.Open("Convert String Encoding");
                    TextDocument doc = (TextDocument)theDoc.Object("TextDocument");
                    TextSelection ts = doc.Selection;

                    if (!ts.IsEmpty)
                    {
                        string text = Convert(ts.Text);
                        EditPoint bp = ts.TopPoint.CreateEditPoint();
                        bp.ReplaceText(ts.BottomPoint, text,
                            (int) vsEPReplaceTextOptions.vsEPReplaceTextKeepMarkers);
                    }
                }
                catch (Exception ex)
                {
	                Logger.WriteMessage("转换文字编码时发生错误: " + ex.Message);
                }
                finally
                {
                    application.UndoContext.Close();
                }

            }

            return null;
        }
    }
}
