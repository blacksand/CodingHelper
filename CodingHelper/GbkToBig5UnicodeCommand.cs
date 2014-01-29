
namespace Blacksand
{
    class GbkToBig5UnicodeCommand : EncodingConvertCommand
    {
        public GbkToBig5UnicodeCommand()
            : base("GbkToBig5_Unicode", "GBK to Big5 (Unicode)", "转换 GBK 编码文字到 Big5 编码。")
        {}

        protected override string Convert(string text)
        {
            return _converter.SimplifiedToTraditional(text);
        }
    }
}
