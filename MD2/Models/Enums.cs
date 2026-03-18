namespace MarkingDesigner.Models
{
    public enum MarkingFonts { Font1, Font2, Font3 }
    public enum Quadrants { _1st, _2nd, _3rd, _4th }
    public enum Devices { None, VM2030, VM5030, VM3730A, VMM1 }

    public enum Trajectory { Linear, Arc, Rotation }
    public enum IncrementFormat { ZeroFill, LeftAlign, RightAlign }
    public enum IncrementDisplay { Numeric, Hex, Alpha }
    public enum IncrementReset { None, Monthly, Daily, Hourly }

    // 1文字分のデータ定義
    public class NCData
    {
        public int Code { get; set; }
        public string GCode { get; set; } = string.Empty;
        public System.Windows.Rect Bounds { get; set; } = System.Windows.Rect.Empty;

        public NCData(int code, string gcode)
        {
            Code = code;
            GCode = gcode;
        }
    }
}