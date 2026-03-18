using MarkingDesigner.ViewModels;

namespace MarkingDesigner.Models
{
    public class MarkingData : BindableBase
    {
        private string _text = "";
        public string Text { get => _text; set => SetProperty(ref _text, value); }

        private bool _isIncrement;
        public bool IsIncrement { get => _isIncrement; set => SetProperty(ref _isIncrement, value); }

        // MarkingSheet 互換用: 連番情報
        private IncrementInfo _incrementInfo = new IncrementInfo();
        public IncrementInfo IncrementInfo
        {
            get => _incrementInfo;
            set => SetProperty(ref _incrementInfo, value ?? new IncrementInfo());
        }

        // MarkingSheet 互換用: カレンダー情報
        private CalendarInfo _calendarInfo = new CalendarInfo();
        public CalendarInfo CalendarInfo
        {
            get => _calendarInfo;
            set => SetProperty(ref _calendarInfo, value ?? new CalendarInfo());
        }
    }
}