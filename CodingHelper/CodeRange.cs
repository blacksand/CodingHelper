using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.VCCodeModel;

namespace Blacksand {
class CodeRange
{
    public CodeRange(EditPoint beginPoint, EditPoint endPoint)
    {
        _beginPoint = beginPoint;
        _endPoint   = endPoint;
        Regularize();
    }

    // 获取选中的文本, 如无选择, 则获取全部文本
    static public CodeRange FromDocument(TextDocument doc)
    {
        CodeRange     range;
        TextSelection ts = doc.Selection;

        if (ts.IsEmpty)
        {
            EditPoint pt = ts.ActivePoint.CreateEditPoint();
            FileCodeModel2 m = doc.DTE.ActiveDocument.ProjectItem.FileCodeModel as VCFileCodeModel;

            if (m != null)
            {
                //PrintVcCodeElements(pt, m.Parent.ContainingProject.CodeModel.CodeElements);
                //PrintVcCodeElements(pt, (m as VCFileCodeModel).Functions);
                //PrintVcCodeElements(pt, (m as VCFileCodeModel).Classes);
                //PrintVcCodeElements(pt, (m as VCFileCodeModel).Structs);
                //PrintVcCodeElements(pt, (m as VCFileCodeModel).Namespaces);
            }
            else
            {
                m = doc.DTE.ActiveDocument.ProjectItem.FileCodeModel as FileCodeModel2;
                //PrintCodeElements(pt, m.CodeElements);
            }

            CodeElement elem = GetCodeElement(pt, m);

            if (elem != null)
            {
                range = new CodeRange(
                    elem.StartPoint.CreateEditPoint(),
                    elem.EndPoint.CreateEditPoint());
                Logger.WriteMessage("Find scope: " + elem.Kind + " - " + elem.Name);
            }
            else
            {
                range = new CodeRange(
                    doc.StartPoint.CreateEditPoint(),
                    doc.EndPoint.CreateEditPoint());
                Logger.WriteMessage("Can't find scope, use all document.");
            }
        }
        else
        {
            range = new CodeRange(
                ts.TopPoint.CreateEditPoint(), ts.BottomPoint.CreateEditPoint());
        }

        range.Regularize();
        return range;
    }

    // 调试用
    private static void PrintVcCodeElements(EditPoint pt, CodeElements allElems, int level = 1)
    {
        foreach (VCCodeElement e in allElems)
        {
            try
            {
                PrintPointVCCodeElement(level, pt, e);

                switch (e.Kind)
                {
                    case vsCMElement.vsCMElementClass:
                        PrintCodeElements(pt, (e as VCCodeClass).Members, level + 1);
                        break;
                    case vsCMElement.vsCMElementInterface:
                        PrintCodeElements(pt, (e as VCCodeInterface).Members, level + 1);
                        break;
                    case vsCMElement.vsCMElementStruct:
                        PrintCodeElements(pt, (e as VCCodeStruct).Members, level + 1);
                        break;
                    case vsCMElement.vsCMElementNamespace:
                        PrintCodeElements(pt, (e as VCCodeNamespace).Members, level + 1);
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                Logger.WriteMessage("Error: " + ex.Message);
            }
        }
    }

    // 调试用 需要递归寻找
    private static void PrintCodeElements(EditPoint pt, CodeElements allElems, int level = 1)
    {
        foreach (CodeElement2 e in allElems)
        {
            try
            {
                PrintPointCodeElement(level, pt, e);

                switch (e.Kind)
                {
                    case vsCMElement.vsCMElementClass:
                        PrintCodeElements(pt, (e as CodeClass2).Members, level + 1);
                        break;
                    case vsCMElement.vsCMElementInterface:
                        PrintCodeElements(pt, (e as CodeInterface2).Members, level + 1);
                        break;
                    case vsCMElement.vsCMElementStruct:
                        PrintCodeElements(pt, (e as CodeStruct2).Members, level + 1);
                        break;
                    case vsCMElement.vsCMElementNamespace:
                        PrintCodeElements(pt, (e as CodeNamespace).Members, level + 1);
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                Logger.WriteMessage("Error: " + ex.Message);
            }
        }
    }

    // 调试用
    private static void PrintPointVCCodeElement(int level, EditPoint pt, VCCodeElement e)
    {
        string msg = new string('-', level * 2);
        msg += e.Kind.ToString() + ": " + e.Name;
        msg += " (" + e.StartPoint.Line + ", " + e.EndPoint.Line + ")";
    
        try
        {
            if (pt.LessThan(e.StartPoint) || pt.GreaterThan(e.EndPoint))
            {
                // 不在范围内
            }
            else
            {
                msg += " <---------------------" ;
            }

            Logger.WriteMessage(msg);
        }
        catch
        {
            if (e.Name.Contains("Core"))
                Logger.WriteMessage(msg);
        }
    }

    // 调试用
    private static void PrintPointCodeElement(int level, EditPoint pt, CodeElement2 e)
    {
        string msg = new string('-', level * 2);
        msg += e.Kind.ToString() + ": " + e.Name;
        msg += " (" + e.StartPoint.Line + ", " + e.EndPoint.Line + ")";
    
        try
        {
            if (pt.LessThan(e.StartPoint) || pt.GreaterThan(e.EndPoint))
            {
                // 不在范围内
            }
            else
            {
                msg += " <---------------------" ;
            }

            Logger.WriteMessage(msg);
        }
        catch
        {
            if (e.Name.Contains("Core"))
                Logger.WriteMessage(msg);
        }
    }

    private static CodeElement GetCodeElement(EditPoint pt, FileCodeModel2 codeModel)
    {
        vsCMElement[] needTypes = new vsCMElement[] {
            vsCMElement.vsCMElementFunction,
            vsCMElement.vsCMElementProperty,
            vsCMElement.vsCMElementEnum,
            vsCMElement.vsCMElementUnion,
            vsCMElement.vsCMElementStruct,
            vsCMElement.vsCMElementInterface,
            vsCMElement.vsCMElementClass,
            vsCMElement.vsCMElementNamespace,
        };

        CodeElement elem = null;

        foreach (vsCMElement elemType in needTypes)
        {
            try
            {
                elem = codeModel.CodeElementFromPoint(pt, elemType);

                if (elem != null)
                    break;
            }
            catch
            {
            }
        }

        return elem;
    }

    // 去除前后空行, 计算第一列的缩进位置, 0 表示文本为空
    private int CalcIndentColumn()
    {
        int       col = 0;
        EditPoint pt  = BeginPoint.CreateEditPoint();

        if (pt.FindPattern("[^\\s]", (int) vsFindOptions.vsFindOptionsRegularExpression))
        {
            if (pt.LessThan(EndPoint))
                col = pt.DisplayColumn;
        }

        // abc
        return col;
    }

    public int TopLine
    {
        get { return _beginPoint.Line; }
    }

    public int BottomLine
    {
        get { return _endPoint.Line; }
    }

    public bool IsEmpty
    {
        get
        {
            if (_beginPoint.LessThan(_endPoint) && IndentColumn > 0)
                return false;

            return true;
        }
    }

    public string Text
    {
        get { return _beginPoint.GetText(_endPoint); }
    }

    public EditPoint BeginPoint
    {
        get { return _beginPoint; }
    }

    public EditPoint EndPoint
    {
        get { return _endPoint; }
    }

    public int IndentColumn
    {
        get;
        set;
    }

    internal void Regularize()
    {
        if (_beginPoint.GreaterThan(_endPoint))
        {
            EditPoint tmp = _beginPoint;
            _beginPoint = _endPoint;
            _endPoint   = _beginPoint;
        }

        _beginPoint.StartOfLine();
        _endPoint.EndOfLine();
        IndentColumn = CalcIndentColumn();
    }

    public override String ToString()
    {
        return String.Format("{4}: Begin({0}, {1}), End({2}, {3}), Indent: {5}",
                             new object[] {
                                 TopLine, BeginPoint.DisplayColumn, BottomLine,
                                 EndPoint.DisplayColumn,
                                 this.GetType(), IndentColumn
                             });
    }

    private EditPoint _beginPoint;
    private EditPoint _endPoint;
}

}
