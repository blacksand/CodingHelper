using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic;

namespace Blacksand
{
    class ChineseConverter
    {
        static private ChineseConverter _instance = null;
        private Encoding _gbkEncoding = null;
        private Encoding _big5Encoding = null;

        private ChineseConverter()
        {
            _gbkEncoding = Encoding.GetEncoding("gbk");
            _big5Encoding = Encoding.GetEncoding("big5");
        }

        static public ChineseConverter Get()
        {
            if (_instance == null)
                _instance = new ChineseConverter();

            return _instance;
        }

        internal string SimplifiedToTraditional(string str)
        {
            return Strings.StrConv(str, VbStrConv.TraditionalChinese, 2052);
        }

        internal string TraditionalToSimplified(string str)
        {
            return Strings.StrConv(str, VbStrConv.SimplifiedChinese, 2052);
        }

        internal string ConvertToGBK(string text)
        {
            byte[] chs = _gbkEncoding.GetBytes(text);
            return _big5Encoding.GetString(chs);
        }

        internal string ConvertToBig5(string text)
        {
            byte[] cht = _big5Encoding.GetBytes(text);
            return _gbkEncoding.GetString(cht);
        }
    }
}
