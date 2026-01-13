using System.Windows;
using System.Windows.Controls.Primitives;
using Microsoft.Xaml.Behaviors;

namespace MarkingDesigner.Behaviors
{
    public class ElementResizeHeightBehavior : Behavior<Thumb>
    {
        public static readonly DependencyProperty HeightProperty =
            DependencyProperty.Register("Height", typeof(double), typeof(ElementResizeHeightBehavior),
                new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        public double Height { get => (double)GetValue(HeightProperty); set => SetValue(HeightProperty, value); }

        protected override void OnAttached() { base.OnAttached(); AssociatedObject.DragDelta += OnDragDelta; }
        protected override void OnDetaching() { AssociatedObject.DragDelta -= OnDragDelta; base.OnDetaching(); }
        private void OnDragDelta(object sender, DragDeltaEventArgs e)
        {
            double factor = 25.4 / 96.0;
            // Y軸反転のため符号に注意
            double h = Height - (e.VerticalChange * factor);
            Height = h < 1.0 ? 1.0 : h;
        }
    }
}