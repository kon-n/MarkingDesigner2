using MarkingDesigner.ViewModels;

namespace MarkingDesigner.Models
{
    /// <summary>
    /// MarkingSheet のインクリメント機能に相当する情報コンテナ。
    /// まずは最小限の構造のみを保持し、詳細仕様は後から拡張する。
    /// </summary>
    public class IncrementInfo : BindableBase
    {
        private int _initialValue;
        public int InitialValue
        {
            get => _initialValue;
            set => SetProperty(ref _initialValue, value);
        }

        private int _currentValue;
        public int CurrentValue
        {
            get => _currentValue;
            set => SetProperty(ref _currentValue, value);
        }

        private string _header = "";
        public string Header
        {
            get => _header;
            set => SetProperty(ref _header, value);
        }

        private string _footer = "";
        public string Footer
        {
            get => _footer;
            set => SetProperty(ref _footer, value);
        }

        private int _digits = 1;
        public int Digits
        {
            get => _digits;
            set => SetProperty(ref _digits, Math.Clamp(value, 1, 10));
        }

        private IncrementFormat _format = IncrementFormat.ZeroFill;
        public IncrementFormat Format
        {
            get => _format;
            set => SetProperty(ref _format, value);
        }

        private IncrementReset _reset = IncrementReset.None;
        public IncrementReset Reset
        {
            get => _reset;
            set => SetProperty(ref _reset, value);
        }
    }
}

