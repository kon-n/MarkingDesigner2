namespace MarkingDesigner.Models
{
    public enum MarkingFonts { FontA, FontB, FontC }
    public enum Quadrants { _1st, _2nd, _3rd, _4th }
    public enum Devices { None, VM2030, VM5030, VM3730A, VMM1 }

    // 1文字分のデータ定義
    public class NCData
    {
        public int Code { get; set; }
        public string GCode { get; set; }

        public NCData(int code, string gcode)
        {
            Code = code;
            GCode = gcode;
        }
    }
}