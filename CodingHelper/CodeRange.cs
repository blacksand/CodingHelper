using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnvDTE;

namespace Blacksand
{
    class CodeRange
    {
        public CodeRange(EditPoint beginPoint, EditPoint endPoint)
        {
            _beginPoint = beginPoint;
            _endPoint = endPoint;
            Regularize();
        }
        // 获取选中的文本, 如无选择, 则获取全部文本
        static public CodeRange FromDocument(TextDocument doc)
        {
            CodeRange range;
            TextSelection ts = doc.Selection;

            if (ts.IsEmpty)
            {
	            EditPoint pt = ts.ActivePoint.CreateEditPoint();

	            CodeElement elem = pt.get_CodeElement(vsCMElement.vsCMElementFunction);
	            if (elem == null) elem = pt.get_CodeElement(vsCMElement.vsCMElementClass);
	
	            if (elem != null)
	                range = new CodeRange(elem.StartPoint.CreateEditPoint(), elem.EndPoint.CreateEditPoint());
	            else
	                range = new CodeRange(doc.StartPoint.CreateEditPoint(), doc.EndPoint.CreateEditPoint());
            }
            else
            {
                range = new CodeRange(ts.TopPoint.CreateEditPoint(), ts.BottomPoint.CreateEditPoint());
            }

            range.Regularize();
            return range;
        }

        // 去除前后空行, 计算第一列的缩进位置, 0 表示文本为空
        private int CalcIndentColumn()
        {
            int col = 0;
            EditPoint pt = BeginPoint.CreateEditPoint();
            
            if (pt.FindPattern("[^\\s]", (int) vsFindOptions.vsFindOptionsRegularExpression))
            {
                if (pt.LessThan(EndPoint))
                    col = pt.DisplayColumn;
            }
            
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
            get { return _indentColumn; }
            set { _indentColumn = value; }
        }
         
        internal void Regularize()
        {
            if (_beginPoint.GreaterThan(_endPoint))
            {
                EditPoint tmp = _beginPoint;
                _beginPoint = _endPoint;
                _endPoint = _beginPoint;
            }

            _beginPoint.StartOfLine();
            _endPoint.EndOfLine();
            _indentColumn = CalcIndentColumn();
        }

        public override String ToString()
        {
            return String.Format("{4}: Begin({0}, {1}), End({2}, {3}), Indent: {5}",
                new object[]{
                    TopLine, BeginPoint.DisplayColumn, BottomLine, EndPoint.DisplayColumn,
                    this.GetType(), IndentColumn});
        }

        private int _indentColumn;
        private EditPoint _beginPoint;
        private EditPoint _endPoint;
    }

}
