using System.Windows;
using System.Windows.Controls.Primitives;
using Microsoft.Xaml.Behaviors;

namespace MarkingDesigner.Behaviors
{
    public class ElementMoveBehavior : Behavior<FrameworkElement>
    {
        public static readonly DependencyProperty PositionXProperty =
            DependencyProperty.Register("PositionX", typeof(double), typeof(ElementMoveBehavior),
                new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty PositionYProperty =
            DependencyProperty.Register("PositionY", typeof(double), typeof(ElementMoveBehavior),
                new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty ZoomScaleProperty =
            DependencyProperty.Register("ZoomScale", typeof(double), typeof(ElementMoveBehavior),
                new FrameworkPropertyMetadata(1.0));

        public double PositionX { get { return (double)GetValue(PositionXProperty); } set { SetValue(PositionXProperty, value); } }
        public double PositionY { get { return (double)GetValue(PositionYProperty); } set { SetValue(PositionYProperty, value); } }
        public double ZoomScale { get { return (double)GetValue(ZoomScaleProperty); } set { SetValue(ZoomScaleProperty, value); } }

        protected override void OnAttached()
        {
            base.OnAttached();
            if (AssociatedObject is Thumb thumb) thumb.DragDelta += OnThumbDragDelta;
            else
            {
                AssociatedObject.MouseLeftButtonDown += OnMouseDown;
                AssociatedObject.MouseMove += OnMouseMove;
                AssociatedObject.MouseLeftButtonUp += OnMouseUp;
            }
        }

        protected override void OnDetaching()
        {
            if (AssociatedObject is Thumb thumb) thumb.DragDelta -= OnThumbDragDelta;
            else
            {
                AssociatedObject.MouseLeftButtonDown -= OnMouseDown;
                AssociatedObject.MouseMove -= OnMouseMove;
                AssociatedObject.MouseLeftButtonUp -= OnMouseUp;
            }
            base.OnDetaching();
        }

        private Point _startPoint;
        private double _startPosX, _startPosY;
        private bool _isDragging;

        private void OnMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            _isDragging = true;
            _startPoint = e.GetPosition(null);
            _startPosX = PositionX;
            _startPosY = PositionY;
            AssociatedObject.CaptureMouse();
            e.Handled = true;
        }

        private void OnMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (!_isDragging) return;
            Point current = e.GetPosition(null);
            Vector diff = current - _startPoint;

            double factor = (25.4 / 96.0) / ZoomScale;
            PositionX = _startPosX + diff.X * factor;
            // 座標系(Canvas.Bottom)にあわせてY方向を反転 (WPF Y+ Down -> Industrial Y+ Up)
            PositionY = _startPosY - diff.Y * factor;
        }

        private void OnMouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            _isDragging = false;
            AssociatedObject.ReleaseMouseCapture();
        }

        private void OnThumbDragDelta(object sender, DragDeltaEventArgs e)
        {
            double factor = (25.4 / 96.0) / ZoomScale;
            PositionX += e.HorizontalChange * factor;
            PositionY -= e.VerticalChange * factor;
        }
    }
}