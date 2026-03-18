using MarkingDesigner.ViewModels;

namespace MarkingDesigner.Models
{
    /// <summary>
    /// MarkingSheet のカレンダー機能に相当する情報コンテナ。
    /// まずはフォーマット文字列とオフセットのみを保持する簡易版。
    /// </summary>
    public class CalendarInfo : BindableBase
    {
        // 例: "yyyyMMdd", "yy/MM/dd HH:mm" など
        private string _format = "";
        public string Format
        {
            get => _format;
            set => SetProperty(ref _format, value);
        }

        // 日付・時刻のオフセット（日単位）
        private int _dayOffset;
        public int DayOffset
        {
            get => _dayOffset;
            set => SetProperty(ref _dayOffset, value);
        }
    }
}

