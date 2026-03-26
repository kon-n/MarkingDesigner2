using MarkingDesigner.Models;

namespace MarkingDesigner.ViewModels
{
    /// <summary>UI 用: 2次元種別と表示ラベル。</summary>
    public sealed class TwoDSymbolOption
    {
        public TwoDSymbolOption(TwoDSymbolKind kind, string label)
        {
            Kind = kind;
            Label = label;
        }

        public TwoDSymbolKind Kind { get; }
        public string Label { get; }
    }
}
