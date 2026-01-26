using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Text.RegularExpressions;
using System.ComponentModel;
using MarkingDesigner.Models;

namespace MarkingDesigner.Views.Controls
{
    public class MarkingObject : FrameworkElement
    {
        #region Properties
        public static readonly DependencyProperty StrokeProperty =
            DependencyProperty.Register("Stroke", typeof(Brush), typeof(MarkingObject),
            new FrameworkPropertyMetadata(Brushes.Black, FrameworkPropertyMetadataOptions.AffectsRender));
        public Brush Stroke { get { return (Brush)GetValue(StrokeProperty); } set { SetValue(StrokeProperty, value); } }

        public static readonly DependencyProperty StrokeThicknessProperty =
            DependencyProperty.Register("StrokeThickness", typeof(double), typeof(MarkingObject),
            new FrameworkPropertyMetadata(1.0, FrameworkPropertyMetadataOptions.AffectsRender));
        public double StrokeThickness { get { return (double)GetValue(StrokeThicknessProperty); } set { SetValue(StrokeThicknessProperty, value); } }
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
            new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));
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

        protected Geometry GetGeometry()
        {
            if (DesignerProperties.GetIsInDesignMode(this)) return Geometry.Empty;
            if (!MarkingFont.IsInitialized || MarkingFont.Fonts == null) return Geometry.Empty;
            if (!MarkingFont.Fonts.ContainsKey(FontType)) return Geometry.Empty;
            if (string.IsNullOrEmpty(Text)) return Geometry.Empty;

            var ggs = new GeometryGroup();
            var currentFontDict = MarkingFont.Fonts[FontType];

            // mm -> DIP (96/25.4)
            double dipScale = 96.0 / 25.4;
            double renderScale = (CharHeight / 1000.0) * dipScale;
            double currentX = 0;
            double currentY = 0;

            for (int i = 0; i < Text.Length; i++)
            {
                byte charCode = (byte)Text[i];
                if (currentFontDict.TryGetValue(charCode, out var ncData))
                {
                    if (ncData != null && !string.IsNullOrEmpty(ncData.GCode))
                    {
                        GeometryGroup charGroup = GCodeUtility.ParseToGeometry(ncData.GCode);
                        TransformGroup charTg = new TransformGroup();
                        
                        // 1. Scale to mm then to DIP
                        charTg.Children.Add(new ScaleTransform(renderScale, renderScale));
                        
                        // 2. Rotate
                        double angle = GetAngleFromType(RotationType);
                        if (angle != 0) charTg.Children.Add(new RotateTransform(angle));

                        // 3. Move by pitch (converted to DIP)
                        charTg.Children.Add(new TranslateTransform(currentX * dipScale, currentY * dipScale));
                        
                        charGroup.Transform = charTg;
                        ggs.Children.Add(charGroup);
                    }
                }
                currentX += PitchX;
                currentY += PitchY;
            }

            if (ggs.CanFreeze) ggs.Freeze();
            return ggs;
        }

        protected override void OnRender(DrawingContext dc)
        {
            var geo = GetGeometry();
            if (geo == null || geo == Geometry.Empty) return;

            Rect b = geo.Bounds;
            if (b.IsEmpty) return;

            // Normalize top-left to (0,0) for the local element spacing
            dc.PushTransform(new TranslateTransform(-b.Left, -b.Top));
            
            // Draw Text
            dc.DrawGeometry(null, new Pen(Stroke, StrokeThickness), geo);

            // Draw center dot (1mm diameter)
            double mm2dip = 96.0 / 25.4;
            double dotRadius = (1.0 * mm2dip) / 2.0; 
            dc.DrawEllipse(Brushes.Red, null, new Point(0, 0), dotRadius, dotRadius);

            dc.Pop();
        }

        protected override Size MeasureOverride(Size constraint)
        {
            var geo = GetGeometry();
            if (geo == null || geo == Geometry.Empty) return new Size(0, 0);

            Rect b = geo.Bounds;
            if (b.IsEmpty) return new Size(0, 0);

            return new Size(Math.Max(b.Width, 1.0), Math.Max(b.Height, 1.0));
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            return base.ArrangeOverride(finalSize);
        }


    }
}