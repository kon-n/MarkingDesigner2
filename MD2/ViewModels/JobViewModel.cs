using System.Windows;
using System.Windows.Media;
using MarkingDesigner.Models;

namespace MarkingDesigner.ViewModels
{
    public class ColorInfo
    {
        public string Name { get; set; } = "";
        public Brush Brush { get; set; } = Brushes.Black;
    }

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

        public static List<ColorInfo> AvailableColors { get; } = new List<ColorInfo>
        {
            new ColorInfo { Name = "Black", Brush = Brushes.Black },
            new ColorInfo { Name = "Red", Brush = Brushes.Red },
            new ColorInfo { Name = "Blue", Brush = Brushes.Blue },
            new ColorInfo { Name = "Green", Brush = Brushes.Green },
            new ColorInfo { Name = "Orange", Brush = Brushes.Orange },
            new ColorInfo { Name = "Purple", Brush = Brushes.Purple },
            new ColorInfo { Name = "Brown", Brush = Brushes.Brown },
            new ColorInfo { Name = "Navy", Brush = Brushes.Navy },
            new ColorInfo { Name = "Teal", Brush = Brushes.Teal },
            new ColorInfo { Name = "Maroon", Brush = Brushes.Maroon },
            new ColorInfo { Name = "Olive", Brush = Brushes.Olive },
            new ColorInfo { Name = "Gray", Brush = Brushes.Gray },
            new ColorInfo { Name = "Lime", Brush = Brushes.Lime },
            new ColorInfo { Name = "Aqua", Brush = Brushes.Aqua },
            new ColorInfo { Name = "Fuchsia", Brush = Brushes.Fuchsia },
            new ColorInfo { Name = "Gold", Brush = Brushes.Gold },
        };

        public static MarkingFonts[] AllMarkingFonts => (MarkingFonts[])Enum.GetValues(typeof(MarkingFonts));
        public static Trajectory[] AllTrajectories => (Trajectory[])Enum.GetValues(typeof(Trajectory));
        public static IncrementDisplay[] AllIncDisplays => (IncrementDisplay[])Enum.GetValues(typeof(IncrementDisplay));
        public static IncrementReset[] AllIncResets => (IncrementReset[])Enum.GetValues(typeof(IncrementReset));

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

        private int _rotationType = 1;
        public int RotationType
        {
            get => _rotationType;
            set { if (SetProperty(ref _rotationType, value)) UpdateCalculatedValues(); }
        }

        private Brush _stroke = Brushes.Black;
        public Brush Stroke
        {
            get => _stroke;
            set => SetProperty(ref _stroke, value);
        }

        private MarkingFonts _font = MarkingFonts.Font1;
        public MarkingFonts Font 
        { 
            get => _font; 
            set { if (SetProperty(ref _font, value)) UpdateCalculatedValues(); } 
        }

        private double _endX;
        public double EndX { get => _endX; set => SetProperty(ref _endX, Math.Round(value, 1)); }

        private double _endY;
        public double EndY { get => _endY; set => SetProperty(ref _endY, Math.Round(value, 1)); }

        private double _speed = 1000;
        public double Speed { get => _speed; set => SetProperty(ref _speed, Math.Round(value / 100.0) * 100.0); }

        private Trajectory _trajectory = Trajectory.Linear;
        public Trajectory Trajectory { get => _trajectory; set => SetProperty(ref _trajectory, value); }

        private bool _isLine;
        public bool IsLine { get => _isLine; set => SetProperty(ref _isLine, value); }

        private bool _isArc;
        public bool IsArc { get => _isArc; set => SetProperty(ref _isArc, value); }

        private double _arcCenterX;
        public double ArcCenterX { get => _arcCenterX; set => SetProperty(ref _arcCenterX, Math.Round(value, 1)); }

        private double _arcCenterY;
        public double ArcCenterY { get => _arcCenterY; set => SetProperty(ref _arcCenterY, Math.Round(value, 1)); }

        private double _arcAnglePitch;
        public double ArcAnglePitch { get => _arcAnglePitch; set => SetProperty(ref _arcAnglePitch, Math.Round(value, 1)); }

        // 3点入力用座標（MarkingSheet の Arc AX/AY/BX/BY/CX/CY に対応）
        private double _arcAx;
        public double ArcAx { get => _arcAx; set => SetProperty(ref _arcAx, Math.Round(value, 1)); }

        private double _arcAy;
        public double ArcAy { get => _arcAy; set => SetProperty(ref _arcAy, Math.Round(value, 1)); }

        private double _arcBx;
        public double ArcBx { get => _arcBx; set => SetProperty(ref _arcBx, Math.Round(value, 1)); }

        private double _arcBy;
        public double ArcBy { get => _arcBy; set => SetProperty(ref _arcBy, Math.Round(value, 1)); }

        private double _arcCx;
        public double ArcCx { get => _arcCx; set => SetProperty(ref _arcCx, Math.Round(value, 1)); }

        private double _arcCy;
        public double ArcCy { get => _arcCy; set => SetProperty(ref _arcCy, Math.Round(value, 1)); }

        private double _rangeTop;
        public double RangeTop { get => _rangeTop; set => SetProperty(ref _rangeTop, Math.Round(value, 1)); }

        private double _rangeBottom;
        public double RangeBottom { get => _rangeBottom; set => SetProperty(ref _rangeBottom, Math.Round(value, 1)); }

        private double _rangeLeft;
        public double RangeLeft { get => _rangeLeft; set => SetProperty(ref _rangeLeft, Math.Round(value, 1)); }

        private double _rangeRight;
        public double RangeRight { get => _rangeRight; set => SetProperty(ref _rangeRight, Math.Round(value, 1)); }

        private string _calendarString = "";
        public string CalendarString { get => _calendarString; set => SetProperty(ref _calendarString, value); }

        private IncrementFormat _incFormat = IncrementFormat.ZeroFill;
        public IncrementFormat IncFormat { get => _incFormat; set => SetProperty(ref _incFormat, value); }

        private string _incHeader = "";
        public string IncHeader { get => _incHeader; set => SetProperty(ref _incHeader, value); }

        private string _incFooter = "";
        public string IncFooter { get => _incFooter; set => SetProperty(ref _incFooter, value); }

        private int _incCurrentValue;
        public int IncCurrentValue { get => _incCurrentValue; set => SetProperty(ref _incCurrentValue, value); }

        private IncrementDisplay _incDisplay = IncrementDisplay.Numeric;
        public IncrementDisplay IncDisplay { get => _incDisplay; set => SetProperty(ref _incDisplay, value); }

        private int _incDigits = 1;
        public int IncDigits { get => _incDigits; set => SetProperty(ref _incDigits, Math.Clamp(value, 1, 10)); }

        private IncrementReset _incReset = IncrementReset.None;
        public IncrementReset IncReset { get => _incReset; set => SetProperty(ref _incReset, value); }

        private MarkingData _marking = new MarkingData();
        public MarkingData Marking
        {
            get => _marking;
            set
            {
                if (value == null) return; // Disallow null
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