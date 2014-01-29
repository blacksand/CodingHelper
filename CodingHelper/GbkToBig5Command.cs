
namespace Blacksand
{
    class GbkToBig5Command : EncodingConvertCommand
    {
        public GbkToBig5Command()
            : base("GbkToBig5", "GBK to Big5", "转换 GBK 编码文字到 Big5 编码。")
        {}

        protected override string Convert(string text)
        {
            text = _converter.SimplifiedToTraditional(text);
            text = _converter.ConvertToBig5(text);

            return text;
        }
    }
}
