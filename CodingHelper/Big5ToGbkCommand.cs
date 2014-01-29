
namespace Blacksand
{
    class Big5ToGbkCommand : EncodingConvertCommand
    {
        public Big5ToGbkCommand()
            : base("Big5ToGbk", "Big5 to GBK", "转换 Big5 编码文字到 GBK 编码。")
        {}

        protected override string Convert(string text)
        {
            text = _converter.ConvertToGBK(text);
            // 保留繁体即可
            // text = _converter.TraditionalToSimplified(text);
            return text;
        }
    }
}
