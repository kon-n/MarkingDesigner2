using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Text.RegularExpressions;

namespace MarkingDesigner.Models
{
    public static class GCodeUtility
    {
        public static Rect CalculateBounds(string gcodeStr)
        {
            var geo = ParseToGeometry(gcodeStr);
            if (geo == null || geo.Children.Count == 0) return Rect.Empty;
            return geo.Bounds;
        }

        public static GeometryGroup ParseToGeometry(string gcodeStr)
        {
            var gg = new GeometryGroup();
            Point currentPos = new Point(0, 0);
            Point nextPos = currentPos;
            double curI = 0, curJ = 0;
            int mode = 0;

            bool xSet = false, ySet = false, iSet = false, jSet = false;

            var tokenRegex = new Regex(@"([GMXYIJ])([\-0-9\.]+)");
            var matches = tokenRegex.Matches(gcodeStr);

            void ExecuteBlock()
            {
                if (!xSet && !ySet && !iSet && !jSet) return;

                if (mode == 0)
                {
                    currentPos = nextPos;
                }
                else if (mode == 1)
                {
                    if (currentPos != nextPos)
                    {
                        var pf = new PathFigure { StartPoint = currentPos };
                        pf.Segments.Add(new LineSegment(nextPos, true));
                        gg.Children.Add(new PathGeometry(new[] { pf }));
                        currentPos = nextPos;
                    }
                }
                else if (mode == 2 || mode == 3)
                {
                    double r = Math.Sqrt(curI * curI + curJ * curJ);
                    if (r > 1e-6)
                    {
                        // 180度を超える場合は LargeArc を true にする等の計算が必要だが、
                        // 境界計算用であれば近似でも十分な場合が多い。
                        // ここでは原本のロジック（MarkingObjectにあるもの）を移設。
                        
                        double cx = currentPos.X + curI;
                        double cy = currentPos.Y + curJ;
                        double startAngle = Math.Atan2(currentPos.Y - cy, currentPos.X - cx);
                        double endAngle = Math.Atan2(nextPos.Y - cy, nextPos.X - cx);
                        double sweep = endAngle - startAngle;
                        bool isCW = (mode == 2);
                        if (isCW) { if (sweep > 0) sweep -= 2 * Math.PI; }
                        else { if (sweep < 0) sweep += 2 * Math.PI; }

                        SweepDirection dir = (mode == 2) ? SweepDirection.Counterclockwise : SweepDirection.Clockwise;
                        bool isLargeArc = Math.Abs(sweep) > Math.PI + 0.001;

                        if (currentPos != nextPos)
                        {
                            var pf = new PathFigure { StartPoint = currentPos };
                            pf.Segments.Add(new ArcSegment(nextPos, new Size(r, r), 0, isLargeArc, dir, true));
                            gg.Children.Add(new PathGeometry(new[] { pf }));
                        }
                    }
                    currentPos = nextPos;
                }

                xSet = ySet = iSet = jSet = false;
                curI = 0; curJ = 0;
            }

            foreach (Match m in matches)
            {
                string type = m.Groups[1].Value;
                if (!double.TryParse(m.Groups[2].Value, out double val)) continue;

                if (type == "G") { ExecuteBlock(); mode = (int)val; }
                else if (type == "X") { if (xSet) ExecuteBlock(); nextPos.X = val; xSet = true; }
                else if (type == "Y") { if (ySet) ExecuteBlock(); nextPos.Y = val; ySet = true; }
                else if (type == "I") { if (iSet) ExecuteBlock(); curI = val; iSet = true; }
                else if (type == "J") { if (jSet) ExecuteBlock(); curJ = val; jSet = true; }
            }
            ExecuteBlock();
            return gg;
        }

        public static Rect CalculateStringBounds(string text, MarkingFonts fontType, double charHeight, double pitchX, double pitchY, int rotationType)
        {
            if (string.IsNullOrEmpty(text) || !MarkingFont.IsInitialized) return Rect.Empty;
            if (!MarkingFont.Fonts.ContainsKey(fontType)) return Rect.Empty;

            var fontDict = MarkingFont.Fonts[fontType];
            double scale = charHeight / 1000.0;
            double curX = 0, curY = 0;
            
            Rect combined = Rect.Empty;

            double angle = 0;
            switch (rotationType)
            {
                case 1: angle = -90; break;
                case 2: angle = 180; break;
                case 3: angle = 90; break;
                case 4: angle = -45; break;
                case 5: angle = -135; break;
                case 6: angle = 135; break;
                case 7: angle = 45; break;
            }

            foreach (char c in text)
            {
                if (fontDict.TryGetValue((byte)c, out var nc))
                {
                    // 各文字の Bounds を計算し、スケール・回転・移動を適用
                    Rect b = nc.Bounds;
                    if (!b.IsEmpty)
                    {
                        // 1. スケール
                        Rect scaled = new Rect(b.X * scale, b.Y * scale, b.Width * scale, b.Height * scale);
                        
                        // 2. 回転 (0,0) まわりへの配置が前提なので、四隅を回転させて Bounds を再計算するのが正確
                        Rect rotated = RotateRect(scaled, angle);

                        // 3. 移動
                        rotated.Offset(curX, curY);

                        if (combined.IsEmpty) combined = rotated;
                        else combined.Union(rotated);
                    }
                }
                curX += pitchX;
                curY += pitchY;
            }

            return combined;
        }

        private static Rect RotateRect(Rect r, double angleDegrees)
        {
            if (angleDegrees == 0) return r;
            double rad = angleDegrees * Math.PI / 180.0;
            double cos = Math.Cos(rad);
            double sin = Math.Sin(rad);

            Point[] pts = {
                new Point(r.Left, r.Top),
                new Point(r.Right, r.Top),
                new Point(r.Left, r.Bottom),
                new Point(r.Right, r.Bottom)
            };

            double minX = double.MaxValue, maxX = double.MinValue;
            double minY = double.MaxValue, maxY = double.MinValue;

            foreach (var p in pts)
            {
                double nx = p.X * cos - p.Y * sin;
                double ny = p.X * sin + p.Y * cos;
                minX = Math.Min(minX, nx);
                maxX = Math.Max(maxX, nx);
                minY = Math.Min(minY, ny);
                maxY = Math.Max(maxY, ny);
            }

            return new Rect(minX, minY, maxX - minX, maxY - minY);
        }
    }
}
