using System.Windows;
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
                    UpdateCalculatedValues();
                }
            };
            MarkingFont.Initialized += () => UpdateCalculatedValues();
            UpdateCalculatedValues();
        }

        public int JobId { get; set; }

        private string _name = "";
        public string Name { get => _name; set => SetProperty(ref _name, value); }

        private double _startX;
        public double StartX
        {
            get => _startX;
            set { if (SetProperty(ref _startX, Math.Round(value, 1))) UpdateCalculatedValues(); }
        }

        private double _startY;
        public double StartY
        {
            get => _startY;
            set { if (SetProperty(ref _startY, Math.Round(value, 1))) UpdateCalculatedValues(); }
        }

        private double _height;
        public double Height
        {
            get => _height;
            set { if (SetProperty(ref _height, value)) UpdateCalculatedValues(); }
        }

        private double _pitchX;
        public double PitchX
        {
            get => _pitchX;
            set { if (SetProperty(ref _pitchX, value)) UpdateCalculatedValues(); }
        }

        private double _pitchY;
        public double PitchY
        {
            get => _pitchY;
            set { if (SetProperty(ref _pitchY, value)) UpdateCalculatedValues(); }
        }

        private int _rotationType = 0;
        public int RotationType
        {
            get => _rotationType;
            set { if (SetProperty(ref _rotationType, value)) UpdateCalculatedValues(); }
        }

        private MarkingFonts _font = MarkingFonts.FontA;
        public MarkingFonts Font 
        { 
            get => _font; 
            set { if (SetProperty(ref _font, value)) UpdateCalculatedValues(); } 
        }

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
                    UpdateCalculatedValues();
                }
            }
        }

        private void OnMarkingPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MarkingData.Text))
            {
                UpdateCalculatedValues();
            }
        }

        private bool _isSelected;
        public bool IsSelected { get => _isSelected; set => SetProperty(ref _isSelected, value); }

        private Rect _textBounds;
        public Rect TextBounds
        {
            get => _textBounds;
            private set { if (SetProperty(ref _textBounds, value)) { RaisePropertyChanged(nameof(OffsetX)); RaisePropertyChanged(nameof(OffsetY)); } }
        }

        public double OffsetX => TextBounds.IsEmpty ? 0 : TextBounds.Left;
        public double OffsetY => TextBounds.IsEmpty ? 0 : TextBounds.Top;

        private bool _isOutOfBounds;
        public bool IsOutOfBounds
        {
            get => _isOutOfBounds;
            private set => SetProperty(ref _isOutOfBounds, value);
        }

        private void UpdateCalculatedValues()
        {
            if (Device == null) return;

            // 文字列全体の詳細な境界を取得 (mm単位)
            var b = GCodeUtility.CalculateStringBounds(Marking?.Text ?? "", Font, Height, PitchX, PitchY, RotationType);
            TextBounds = b;

            if (b.IsEmpty)
            {
                IsOutOfBounds = false;
                return;
            }

            // 1文字目の「中心(0,0)」を StartX, StartY に置いた時の全体範囲
            double minX = StartX + b.Left;
            double maxX = StartX + b.Right;
            double minY = StartY + b.Top;
            double maxY = StartY + b.Bottom;

            // 物理的な稼働範囲 check
            // 0.1mm 程度の許容誤差
            bool outX = minX < -0.1 || maxX > Device.RangeX + 0.1;
            bool outY = minY < -0.1 || maxY > Device.RangeY + 0.1;

            IsOutOfBounds = outX || outY;
        }
    }
}