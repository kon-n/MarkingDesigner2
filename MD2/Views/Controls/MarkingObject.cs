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
            new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));
        public string Text { get { return (string)GetValue(TextProperty); } set { SetValue(TextProperty, value); } }

        public static readonly DependencyProperty DisplayScaleProperty =
            DependencyProperty.Register("DisplayScale", typeof(double), typeof(MarkingObject),
            new FrameworkPropertyMetadata(10.0));
        public double DisplayScale { get { return (double)GetValue(DisplayScaleProperty); } set { SetValue(DisplayScaleProperty, value); } }

        public static readonly DependencyProperty CharHeightProperty =
            DependencyProperty.Register("CharHeight", typeof(double), typeof(MarkingObject),
            new FrameworkPropertyMetadata(5.0, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));
        public double CharHeight { get { return (double)GetValue(CharHeightProperty); } set { SetValue(CharHeightProperty, value); } }

        public static readonly DependencyProperty PitchXProperty =
            DependencyProperty.Register("PitchX", typeof(double), typeof(MarkingObject),
            new FrameworkPropertyMetadata(5.0, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));
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
            new FrameworkPropertyMetadata(1, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));
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

                // フォントの基準高さを取得 ( 'A' または 'S' または最初の文字から推測)
                double nativeHeight = GetNativeHeight(currentFontDict);
                double renderScale = CharHeight / nativeHeight;

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
                            
                            // 1. スケール
                            charTg.Children.Add(new ScaleTransform(renderScale, renderScale));
                            
                            // 2. 回転 (個別の文字を回転)
                            double angle = GetAngleFromType(RotationType);
                            if (angle != 0.0)
                            {
                                charTg.Children.Add(new RotateTransform(angle));
                            }

                            // 3. 配置 (ピッチに基づく位置へ)
                            charTg.Children.Add(new TranslateTransform(currentX, currentY));
                            
                            charGroup.Transform = charTg;
                            ggs.Children.Add(charGroup);
                        }
                    }
                    currentX += PitchX;
                    currentY += PitchY;
                }

                // 【追加】描画内容を(0,0)基準に揃えることで Border サイズと同期させる
                Rect bounds = ggs.Bounds;
                if (!bounds.IsEmpty)
                {
                    var finalTg = new TransformGroup();
                    // 左上を(0,0)に持ってくる平行移動
                    finalTg.Children.Add(new TranslateTransform(-bounds.Left, -bounds.Top));
                    ggs.Transform = finalTg;
                }

                if (ggs.CanFreeze) ggs.Freeze();
                return ggs;
            }
        }

        private double GetNativeHeight(Dictionary<byte, NCData> fontDict)
        {
            // 'A'(65), 'S'(83), 'H'(72) あたりから高さを探る
            byte[] probes = { 65, 83, 72, 69 };
            foreach (var code in probes)
            {
                if (fontDict.TryGetValue(code, out var nc))
                {
                    var geo = ParseGCodeToGeometry(nc.GCode);
                    if (geo.Bounds.Height > 0) return geo.Bounds.Height;
                }
            }
            // 見つからなければ辞書の最初の文字から
            foreach (var nc in fontDict.Values)
            {
                var geo = ParseGCodeToGeometry(nc.GCode);
                if (geo.Bounds.Height > 0) return geo.Bounds.Height;
            }
            return 1000.0; // デフォルト
        }

        protected override Size MeasureOverride(Size constraint)
        {
            var geo = DefiningGeometry;
            if (geo == null || geo == Geometry.Empty) return new Size(0, 0);

            Rect b = geo.Bounds;
            // 原点(0,0)からの広がりをサイズとして返す
            double w = Math.Max(b.Right, 0) - Math.Min(b.Left, 0);
            double h = Math.Max(b.Bottom, 0) - Math.Min(b.Top, 0);

            // 負方向に描画されている場合、その分をマージン等で吸収するか、
            // ここで描画原点をずらして(0,0)から始まるようにする設計が必要だが、
            // まずは正確な Bounds を返す。
            return new Size(Math.Max(w, 1.0), Math.Max(h, 1.0));
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            return base.ArrangeOverride(finalSize);
        }

        private GeometryGroup ParseGCodeToGeometry(string gcodeStr)
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
                // 何らかのパラメータまたはコマンドがセットされている場合のみ実行
                if (!xSet && !ySet && !iSet && !jSet) return;

                if (mode == 0)
                {
                    // G00: 高速移動 (描画なし)
                    currentPos = nextPos;
                }
                else if (mode == 1)
                {
                    // G01: 直線補間
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
                    // G02/G03: 円弧補間
                    double r = Math.Sqrt(curI * curI + curJ * curJ);
                    // 半径が極端に小さい場合は無視
                    if (r > 1e-6)
                    {
                        double cx = currentPos.X + curI;
                        double cy = currentPos.Y + curJ;

                        // 開始・終了角度の計算
                        double startAngle = Math.Atan2(currentPos.Y - cy, currentPos.X - cx);
                        double endAngle = Math.Atan2(nextPos.Y - cy, nextPos.X - cx);

                        double sweep = endAngle - startAngle;
                        bool isCW = (mode == 2);

                        // スイープ角の正規化
                        if (isCW)
                        {
                            if (sweep > 0) sweep -= 2 * Math.PI;
                        }
                        else
                        {
                            if (sweep < 0) sweep += 2 * Math.PI;
                        }

                        // WPFの座標反転(ScaleY=-1)を考慮した方向
                        // G02(CW) は通常の座標系では Clockwise だが、
                        // レイアウト全体の ScaleY=-1 空間では、逆方向の Counterclockwise を
                        // 指定することで画面上で時計回りに見えるようになる。
                        SweepDirection dir = (mode == 2) ? SweepDirection.Counterclockwise : SweepDirection.Clockwise;
                        
                        // 180度を超える場合は LargeArc を true にする
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

                // ブロック終了につきフラグとI/Jをリセット
                xSet = ySet = iSet = jSet = false;
                curI = 0; curJ = 0;
            }

            foreach (Match m in matches)
            {
                string type = m.Groups[1].Value;
                if (!double.TryParse(m.Groups[2].Value, out double val)) continue;

                if (type == "G")
                {
                    ExecuteBlock();
                    mode = (int)val;
                }
                else if (type == "X")
                {
                    if (xSet) ExecuteBlock();
                    nextPos.X = val;
                    xSet = true;
                }
                else if (type == "Y")
                {
                    if (ySet) ExecuteBlock();
                    nextPos.Y = val;
                    ySet = true;
                }
                else if (type == "I")
                {
                    if (iSet) ExecuteBlock();
                    curI = val;
                    iSet = true;
                }
                else if (type == "J")
                {
                    if (jSet) ExecuteBlock();
                    curJ = val;
                    jSet = true;
                }
            }
            // 最後に残ったブロックを実行
            ExecuteBlock();

            return gg;
        }
    }
}