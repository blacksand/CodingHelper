using System;
using System.Text;
using EnvDTE;
using EnvDTE80;

namespace Blacksand
{
    class CommentSurroundCommand : HelperCommand
    {
        public CommentSurroundCommand()
            : base("CommentSurround", "环绕注释", "为选中的代码行生成环绕注释。")
        {}

        public override object Execute(DTE2 application, AddIn addin, object varIn)
        {
            Document theDoc = application.ActiveDocument;

            if (theDoc != null)
            {
	            try
	            {
                    application.UndoContext.Open("Comment surround");
                    TextDocument doc = (TextDocument) theDoc.Object("TextDocument");
                    TextSelection ts = doc.Selection;
                    EditPoint bp = ts.TopPoint.CreateEditPoint();
                    EditPoint ep = ts.BottomPoint.CreateEditPoint();

                    // 1. 无选中块
                    //  - 空行         在当前行之前插入空注释行
                    //  - 注释行       用填充字符填充当前行
                    //  - 代码行       在当前行之前插入注释行(合并已存在的注释行)
                    //
                    // 2. 有选中内容
                    //  - 在 bp 前插入注释行
                    //  - 在 ep 后插入注释行

                    if (ts.IsEmpty)
                    {
                        string lineText = bp.GetLines(bp.Line, bp.Line + 1).TrimStart();

                        if (lineText.Length == 0)
                        {
                            InsertEmptyCommentLine(bp);
                        }
                        else if (lineText.StartsWith("//"))
                        {
                            UpdateCommentLine(bp);
                        }
                        else
                        {
                            InsertCommentLine(bp);
                        }
                    }
                    else
                    {
                        if (!ep.AtStartOfLine)
                            ep.LineDown();

                        if (ep.GetLines(ep.Line, ep.Line + 1).TrimStart().StartsWith("//"))
                            ep.LineDown();

                        InsertCommentLine(ep);
                        InsertCommentLine(bp);
                    }
	            }
	            catch (Exception ex)
	            {
	                Logger.WriteMessage("生成环绕注释时发生错误: " + ex.Message);
	            }
                finally
                {
                    application.UndoContext.Close();
                }
            }

            return null;
        }

        private void InsertEmptyCommentLine(EditPoint bp)
        {
            bp.StartOfLine();
            bp.Insert("\n");
            bp.LineUp();
            UpdateCommentLine(bp);
        }

        private void UpdateCommentLine(EditPoint bp)
        {
            EditPoint ep = bp.CreateEditPoint();
            ep.EndOfLine();
            bp.StartOfLine();

            string text = "// " + bp.GetText(ep).Trim(" \t/-=*".ToCharArray());
            bp.Delete(ep);
            ep.Insert(text);
            //ReplaceText(bp, ep, text);

            int indent = ep.LineCharOffset;
            bp.SmartFormat(ep);
            indent = ep.LineCharOffset - indent;

            char c = Properties.Settings.Default.comment_pad;
            int length = Properties.Settings.Default.comment_length;
            int pad = Math.Max(0, length - GetTextByteLength(text) - indent);

            if (ep.GetText(-1) == " ")
                ep.Insert("".PadRight(pad, c));
            else
                ep.Insert(" ".PadRight(pad, c));
        }

        private void ReplaceText(EditPoint bp, EditPoint ep, string text)
        {
            bp.ReplaceText(ep, text,
                (int) vsEPReplaceTextOptions.vsEPReplaceTextKeepMarkers +
                (int) vsEPReplaceTextOptions.vsEPReplaceTextTabsSpaces);
        }

        private Int32 GetTextByteLength(string text)
        {
            Encoding enc = Encoding.GetEncoding("gb2312");
            return enc.GetByteCount(text);
        }

        private void InsertCommentLine(EditPoint bp)
        {
            bp.StartOfLine();

            if (!bp.AtStartOfDocument)
            {
                string preLine = bp.GetLines(bp.Line - 1, bp.Line).TrimStart();

                if ((preLine.StartsWith("//")))
                {
                    bp.LineUp();
                    UpdateCommentLine(bp);
                    return;
                }
            }

            InsertEmptyCommentLine(bp);
        }
    }
}
