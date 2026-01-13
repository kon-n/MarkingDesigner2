using System.Collections.ObjectModel;

namespace MarkingDesigner.ViewModels
{
    public class SequenceViewModel : BindableBase
    {
        public int SequenceId { get; set; }
        public string Title { get; set; } = "";
        public ObservableCollection<JobViewModel> Jobs { get; } = new ObservableCollection<JobViewModel>();
    }
}