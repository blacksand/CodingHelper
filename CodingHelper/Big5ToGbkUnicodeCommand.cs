
namespace Blacksand
{
    class Big5ToGbkUnicodeCommand : EncodingConvertCommand
    {
        public Big5ToGbkUnicodeCommand()
            : base("Big5ToGbk_Unicode", "Big5 to GBK (Unicode)", "转换 Big5 编码文字到 GBK 编码。")
        {}

        protected override string Convert(string text)
        {
            return _converter.TraditionalToSimplified(text);
        }
    }
}
