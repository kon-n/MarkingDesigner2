using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace MarkingDesigner.Models
{
    public static class MarkingFont
    {
        // 修正: 初期値を設定してNull警告を回避
        public static Dictionary<MarkingFonts, Dictionary<byte, NCData>> Fonts { get; private set; } = new();

        public static bool IsInitialized { get; private set; } = false;

        public static void Initialize()
        {
            if (IsInitialized) return;

            Fonts = new Dictionary<MarkingFonts, Dictionary<byte, NCData>>();

            foreach (MarkingFonts font in Enum.GetValues(typeof(MarkingFonts)))
            {
                Fonts[font] = new Dictionary<byte, NCData>();
            }

            try
            {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                LoadFontFile(MarkingFonts.FontA, Path.Combine(baseDir, "Fonts", "GG110712411.ASC"));
                LoadFontFile(MarkingFonts.FontB, Path.Combine(baseDir, "Fonts", "GG110712412.ASC"));
                LoadFontFile(MarkingFonts.FontC, Path.Combine(baseDir, "Fonts", "GG110712413.ASC"));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Font Load Error: " + ex.Message);
                CreateMockData();
            }

            IsInitialized = true;
        }

        private static void LoadFontFile(MarkingFonts font, string filePath)
        {
            if (!File.Exists(filePath)) return;

            var lines = File.ReadAllLines(filePath);
            var dict = Fonts[font];

            int currentCode = -1;
            List<string> currentGCodeLines = new List<string>();

            Regex regexN = new Regex(@"^N(\d+)");

            foreach (var line in lines)
            {
                string trimLine = line.Trim();
                if (string.IsNullOrEmpty(trimLine)) continue;
                if (trimLine.StartsWith("%")) continue;

                var match = regexN.Match(trimLine);
                if (match.Success)
                {
                    if (currentCode != -1 && currentGCodeLines.Count > 0)
                    {
                        string joinedGCode = string.Join(" ", currentGCodeLines);
                        dict[(byte)currentCode] = new NCData(currentCode, joinedGCode);
                    }

                    if (int.TryParse(match.Groups[1].Value, out int code))
                    {
                        currentCode = code;
                        currentGCodeLines.Clear();
                        string content = trimLine.Substring(match.Length).Trim();
                        if (!string.IsNullOrEmpty(content))
                        {
                            currentGCodeLines.Add(content);
                        }
                    }
                }
                else if (currentCode != -1)
                {
                    currentGCodeLines.Add(trimLine);
                }
            }

            if (currentCode != -1 && currentGCodeLines.Count > 0)
            {
                string joinedGCode = string.Join(" ", currentGCodeLines);
                dict[(byte)currentCode] = new NCData(currentCode, joinedGCode);
            }
        }

        private static void CreateMockData()
        {
            var dict = Fonts[MarkingFonts.FontA];
            dict[65] = new NCData(65, "G00 X0 Y0 G01 X50 Y100 G01 X100 Y0");
        }
    }
}