using MarkingDesigner.ViewModels;

namespace MarkingDesigner.Models
{
    public class MarkingData : BindableBase
    {
        private string _text = "";
        public string Text { get => _text; set => SetProperty(ref _text, value); }

        private bool _isIncrement;
        public bool IsIncrement { get => _isIncrement; set => SetProperty(ref _isIncrement, value); }
    }
}