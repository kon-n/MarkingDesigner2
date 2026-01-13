using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Text.RegularExpressions;
using System.ComponentModel;
using MarkingDesigner.Models;

namespace MarkingDesigner.Views.Controls
{
    public class MarkingObject : Shape
    {
        #region Properties
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(MarkingObject),
            new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.AffectsRender));
        public string Text { get { return (string)GetValue(TextProperty); } set { SetValue(TextProperty, value); } }

        public static readonly DependencyProperty DisplayScaleProperty =
            DependencyProperty.Register("DisplayScale", typeof(double), typeof(MarkingObject),
            new FrameworkPropertyMetadata(10.0));
        public double DisplayScale { get { return (double)GetValue(DisplayScaleProperty); } set { SetValue(DisplayScaleProperty, value); } }

        public static readonly DependencyProperty CharHeightProperty =
            DependencyProperty.Register("CharHeight", typeof(double), typeof(MarkingObject),
            new FrameworkPropertyMetadata(5.0, FrameworkPropertyMetadataOptions.AffectsRender));
        public double CharHeight { get { return (double)GetValue(CharHeightProperty); } set { SetValue(CharHeightProperty, value); } }

        public static readonly DependencyProperty PitchXProperty =
            DependencyProperty.Register("PitchX", typeof(double), typeof(MarkingObject),
            new FrameworkPropertyMetadata(5.0, FrameworkPropertyMetadataOptions.AffectsRender));
        public double PitchX { get { return (double)GetValue(PitchXProperty); } set { SetValue(PitchXProperty, value); } }

        public static readonly DependencyProperty PitchYProperty =
           DependencyProperty.Register("PitchY", typeof(double), typeof(MarkingObject),
           new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender));
        public double PitchY { get { return (double)GetValue(PitchYProperty); } set { SetValue(PitchYProperty, value); } }

        public static readonly DependencyProperty FontTypeProperty =
            DependencyProperty.Register("FontType", typeof(MarkingFonts), typeof(MarkingObject),
            new FrameworkPropertyMetadata(MarkingFonts.FontA, FrameworkPropertyMetadataOptions.AffectsRender));
        public MarkingFonts FontType { get { return (MarkingFonts)GetValue(FontTypeProperty); } set { SetValue(FontTypeProperty, value); } }

        public static readonly DependencyProperty RotationTypeProperty =
            DependencyProperty.Register("RotationType", typeof(int), typeof(MarkingObject),
            new FrameworkPropertyMetadata(1, FrameworkPropertyMetadataOptions.AffectsRender));
        public int RotationType { get { return (int)GetValue(RotationTypeProperty); } set { SetValue(RotationTypeProperty, value); } }
        #endregion

        private double GetAngleFromType(int type)
        {
            return type switch
            {
                0 => 0.0,
                1 => -90.0,
                2 => 180.0,
                3 => 90.0,
                4 => -45.0,
                5 => -135.0,
                6 => 135.0,
                7 => 45.0,
                _ => 0.0
            };
        }

        protected override Geometry DefiningGeometry
        {
            get
            {
                if (DesignerProperties.GetIsInDesignMode(this)) return Geometry.Empty;
                if (!MarkingFont.IsInitialized || MarkingFont.Fonts == null) return Geometry.Empty;
                if (!MarkingFont.Fonts.ContainsKey(FontType)) return Geometry.Empty;
                if (string.IsNullOrEmpty(Text)) return Geometry.Empty;

                var ggs = new GeometryGroup();
                var currentFontDict = MarkingFont.Fonts[FontType];

                double renderScale = CharHeight / 1000.0;
                double angle = GetAngleFromType(RotationType); // 先に角度を取得

                double currentX = 0;
                double currentY = 0;

                for (int i = 0; i < Text.Length; i++)
                {
                    byte charCode = (byte)Text[i];
                    if (currentFontDict.ContainsKey(charCode))
                    {
                        var ncData = currentFontDict[charCode];
                        if (ncData != null && !string.IsNullOrEmpty(ncData.GCode))
                        {
                            GeometryGroup charGroup = ParseGCodeToGeometry(ncData.GCode);

                            TransformGroup charTg = new TransformGroup();

                            // 1. スケール (サイズ合わせ)
                            charTg.Children.Add(new ScaleTransform(renderScale, renderScale));

                            // 2. 回転 (文字自身の原点を中心に回転)
                            // ★修正点: 配置(Translate)の前に回転を行うことで、ピッチ方向に影響を与えない
                            if (angle != 0.0)
                            {
                                charTg.Children.Add(new RotateTransform(angle));
                            }

                            // 3. 配置 (ピッチ計算済みの位置へ移動)
                            charTg.Children.Add(new TranslateTransform(currentX, currentY));

                            charGroup.Transform = charTg;
                            ggs.Children.Add(charGroup);
                        }
                    }
                    // ピッチの加算は回転の影響を受けず、そのまま行う
                    currentX += PitchX;
                    currentY += PitchY;
                }

                if (ggs.CanFreeze) ggs.Freeze();
                return ggs;
            }
        }

        private GeometryGroup ParseGCodeToGeometry(string gcodeStr)
        {
            var gg = new GeometryGroup();
            Point start = new Point(0, 0);

            var tokenRegex = new Regex(@"([GMXYIJ])([\-0-9\.]+)");
            var matches = tokenRegex.Matches(gcodeStr);

            int mode = 0;
            Point end = start;
            double I = 0, J = 0;

            for (int k = 0; k < matches.Count; k++)
            {
                var m = matches[k];
                string type = m.Groups[1].Value;
                if (!double.TryParse(m.Groups[2].Value, out double val)) continue;

                if (type == "G")
                {
                    if (val == 0) mode = 0;
                    else if (val == 1) mode = 1;
                    else if (val == 2) mode = 2;
                    else if (val == 3) mode = 3;
                }
                else if (type == "X") end.X = val;
                else if (type == "Y") end.Y = val;
                else if (type == "I") I = val;
                else if (type == "J") J = val;

                bool execute = false;
                if (k == matches.Count - 1) execute = true;
                else
                {
                    string nextType = matches[k + 1].Groups[1].Value;
                    if (type == "Y" || (type == "X" && matches[k + 1].Groups[1].Value != "Y"))
                    {
                        execute = true;
                    }
                }

                if (execute)
                {
                    if (mode == 0)
                    {
                        start = end;
                    }
                    else if (mode == 1)
                    {
                        if (start != end)
                        {
                            var pf = new PathFigure { StartPoint = start };
                            pf.Segments.Add(new LineSegment(end, true));
                            gg.Children.Add(new PathGeometry(new[] { pf }));
                            start = end;
                        }
                    }
                    else if (mode == 2 || mode == 3)
                    {
                        double r = Math.Sqrt(I * I + J * J);
                        if (r > 0)
                        {
                            var pf = new PathFigure { StartPoint = start };
                            pf.Segments.Add(new ArcSegment(end, new Size(r, r), 0, false,
                                mode == 2 ? SweepDirection.Clockwise : SweepDirection.Counterclockwise, true));
                            gg.Children.Add(new PathGeometry(new[] { pf }));
                        }
                        start = end;
                        I = 0; J = 0;
                    }
                }
            }
            return gg;
        }
    }
}