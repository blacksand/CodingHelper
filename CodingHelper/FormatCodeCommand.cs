using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnvDTE;
using EnvDTE80;

namespace Blacksand
{
    class FormatCodeCommand : HelperCommand
    {
        public FormatCodeCommand()
            : base("FormatCode", "格式化选中的文本, 如果没有选择文本, 则格式化整个文件。")
        {}

        public override object Execute(DTE2 application, AddIn addin, object varIn)
        {
            TextDocument doc = (TextDocument) application.ActiveDocument.Object("TextDocument");

            if (doc != null)
            {
	            try
	            {
	                CodeRange range = CodeRange.FromDocument(doc);
                    //////////////////////////////////////////////////////////////////////////
                    Logger.WriteMessage(range.ToString());
                    //////////////////////////////////////////////////////////////////////////

                    if (!range.IsEmpty)
	                {
		                string text = range.Text;
                        string extName = System.IO.Path.GetExtension(application.ActiveDocument.Name);
                        
                        ExternalFormat(ref text, extName);
                        //ExternalFormat(ref text, application.ActiveDocument.);

		                ApplyIndent(ref text, range.IndentColumn);
		                PutTextBack(range, text);
                        //doc.Selection.MoveToPoint(range.BeginPoint, false);
                        //doc.Selection.MoveToPoint(range.EndPoint, true);
	                }
	            }
	            catch (Exception ex)
	            {
	                Logger.WriteMessage("格式化代码时发生错误: " + ex.Message);
	            }
            }

            return null;
        }

        // 将文本写入临时文件, 调用外部程序格式化文本
        private void ExternalFormat(ref string text, string extName)
        {
            TempFile tmpFile = new TempFile(extName);
            tmpFile.Write(text);

            string cmd = Properties.Settings.Default.format_prog;
            string param = Properties.Settings.Default.format_param
                .Replace("$(InFile)", tmpFile.Name)
                .Replace("$(OutFile)", tmpFile.Name);
            Logger.WriteMessage("call: " + cmd + " " + param);

            System.Diagnostics.Process proc = new System.Diagnostics.Process();
            proc.StartInfo.FileName = cmd;
            proc.StartInfo.Arguments = param;
            proc.StartInfo.WorkingDirectory = tmpFile.Path;
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.RedirectStandardError = true;
            proc.StartInfo.CreateNoWindow = true;

            if (proc.Start())
            {
                string msg = proc.StandardOutput.ReadToEnd();
                if (msg.Length > 0) Logger.WriteMessage(msg);

                msg = proc.StandardError.ReadToEnd();
                if (msg.Length > 0) Logger.WriteMessage(msg);
            }
            else
            {
                throw new Exception("执行外部程序出错: " + proc.StandardError.ReadToEnd());
            }

            text = tmpFile.Read();
        }

        // 将首行缩进应用到格式化后的文本上
        private void ApplyIndent(ref string text, int firstColumn)
        {
            int indentCount = firstColumn - 1;

            if (indentCount > 0 && text.Length > 0)
            {
                StringBuilder sb = new StringBuilder();
                foreach (string line in text.Split(new string[]{"\r\n", "\r", "\n"}, StringSplitOptions.None))
                {
                    if (line.Length > 0)
                        sb.AppendLine(line.Insert(0, new String(' ', indentCount)));
                    else
                        sb.AppendLine(line);
                }

                text = sb.ToString();
            }
        }

        // 将文本放回应用程序
        private void PutTextBack(CodeRange range, string text)
        {
            if (!range.EndPoint.AtEndOfDocument)
                text = text.TrimEnd();

            range.BeginPoint.ReplaceText(range.EndPoint, text,
                (int) vsEPReplaceTextOptions.vsEPReplaceTextKeepMarkers +
                (int) vsEPReplaceTextOptions.vsEPReplaceTextTabsSpaces +
                (int)vsEPReplaceTextOptions.vsEPReplaceTextNormalizeNewlines);
        }

    }
}
