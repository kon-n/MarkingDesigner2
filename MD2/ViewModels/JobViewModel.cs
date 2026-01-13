using MarkingDesigner.Models;

namespace MarkingDesigner.ViewModels
{
    public class JobViewModel : BindableBase
    {
        public DeviceViewModel Device { get; }
        public JobViewModel(DeviceViewModel device) { Device = device; }

        public int JobId { get; set; }

        private string _name = "";
        public string Name { get => _name; set => SetProperty(ref _name, value); }

        private double _startX;
        public double StartX { get => _startX; set => SetProperty(ref _startX, value); }

        private double _startY;
        public double StartY { get => _startY; set => SetProperty(ref _startY, value); }

        private double _height;
        public double Height { get => _height; set => SetProperty(ref _height, value); }

        private double _pitchX;
        public double PitchX { get => _pitchX; set => SetProperty(ref _pitchX, value); }

        private double _pitchY;
        public double PitchY { get => _pitchY; set => SetProperty(ref _pitchY, value); }

        // 追加: 回転タイプ (0-7, 初期値1: -90度)
        private int _rotationType = 1;
        public int RotationType { get => _rotationType; set => SetProperty(ref _rotationType, value); }

        public MarkingFonts Font { get; set; } = MarkingFonts.FontA;

        private MarkingData _marking = new MarkingData();
        public MarkingData Marking { get => _marking; set => SetProperty(ref _marking, value); }

        private bool _isSelected;
        public bool IsSelected { get => _isSelected; set => SetProperty(ref _isSelected, value); }
    }
}