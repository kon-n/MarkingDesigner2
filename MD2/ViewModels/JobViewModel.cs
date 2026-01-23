using MarkingDesigner.Models;

namespace MarkingDesigner.ViewModels
{
    public class JobViewModel : BindableBase
    {
        public DeviceViewModel Device { get; }
        public JobViewModel(DeviceViewModel device)
        {
            Device = device;
            Device.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(Device.RangeX) || e.PropertyName == nameof(Device.RangeY))
                {
                    RaisePropertyChanged(nameof(IsOutOfBounds));
                }
            };
        }

        public int JobId { get; set; }

        private string _name = "";
        public string Name { get => _name; set => SetProperty(ref _name, value); }

        private double _startX;
        public double StartX
        {
            get => _startX;
            set 
            { 
                double rounded = Math.Round(value, 1);
                if (SetProperty(ref _startX, rounded)) RaisePropertyChanged(nameof(IsOutOfBounds)); 
            }
        }

        private double _startY;
        public double StartY
        {
            get => _startY;
            set 
            { 
                double rounded = Math.Round(value, 1);
                if (SetProperty(ref _startY, rounded)) RaisePropertyChanged(nameof(IsOutOfBounds)); 
            }
        }

        private double _height;
        public double Height
        {
            get => _height;
            set { if (SetProperty(ref _height, value)) RaisePropertyChanged(nameof(IsOutOfBounds)); }
        }

        private double _pitchX;
        public double PitchX
        {
            get => _pitchX;
            set { if (SetProperty(ref _pitchX, value)) RaisePropertyChanged(nameof(IsOutOfBounds)); }
        }

        private double _pitchY;
        public double PitchY
        {
            get => _pitchY;
            set { if (SetProperty(ref _pitchY, value)) RaisePropertyChanged(nameof(IsOutOfBounds)); }
        }

        private int _rotationType = 1;
        public int RotationType
        {
            get => _rotationType;
            set { if (SetProperty(ref _rotationType, value)) RaisePropertyChanged(nameof(IsOutOfBounds)); }
        }

        public MarkingFonts Font { get; set; } = MarkingFonts.FontA;

        private MarkingData _marking = new MarkingData();
        public MarkingData Marking
        {
            get => _marking;
            set
            {
                if (_marking != null) _marking.PropertyChanged -= OnMarkingPropertyChanged;
                if (SetProperty(ref _marking, value))
                {
                    if (_marking != null) _marking.PropertyChanged += OnMarkingPropertyChanged;
                    RaisePropertyChanged(nameof(IsOutOfBounds));
                }
            }
        }

        private void OnMarkingPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MarkingData.Text))
            {
                RaisePropertyChanged(nameof(IsOutOfBounds));
            }
        }

        private bool _isSelected;
        public bool IsSelected { get => _isSelected; set => SetProperty(ref _isSelected, value); }

        public bool IsOutOfBounds
        {
            get
            {
                if (Device == null) return false;

                double len = Marking?.Text?.Length ?? 0;
                if (len == 0) return false;

                // 文字の幅を高さの約60%と推定（より正確な判定のため）
                double estCharW = Math.Abs(Height) * 0.6;
                double estCharH = Math.Abs(Height);

                // 90度回転系の場合は、個々の文字の幅と高さが入れ替わる
                if (RotationType == 1 || RotationType == 3)
                {
                    double tmp = estCharW;
                    estCharW = estCharH;
                    estCharH = tmp;
                }

                // 全体のサイズ推算
                double w = PitchX * (len - 1) + (PitchX >= 0 ? estCharW : -estCharW);
                double h = PitchY * (len - 1) + (PitchY >= 0 ? estCharH : -estCharH);

                double x1 = StartX;
                double x2 = StartX + w;
                double y1 = StartY;
                double y2 = StartY + h;

                double minX = Math.Min(x1, x2);
                double maxX = Math.Max(x1, x2);
                double minY = Math.Min(y1, y2);
                double maxY = Math.Max(y1, y2);

                // 0.1mm 程度の許容誤差を持たせる
                bool outX = minX < -0.1 || maxX > Device.RangeX + 0.1;
                bool outY = minY < -0.1 || maxY > Device.RangeY + 0.1;

                return outX || outY;
            }
        }
    }
}