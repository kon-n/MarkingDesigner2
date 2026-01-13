namespace MarkingDesigner.ViewModels
{
    public class MarksViewModel : BindableBase
    {
        private string _text = "TEST";
        public string Text { get { return _text; } set { SetProperty(ref _text, value); } }
        private bool _isIncrement;
        public bool IsIncrement { get { return _isIncrement; } set { SetProperty(ref _isIncrement, value); } }
        private bool _isCalender;
        public bool IsCalender { get { return _isCalender; } set { SetProperty(ref _isCalender, value); } }
    }
}