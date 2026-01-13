using System.Windows;
using System.Windows.Controls.Primitives;
using Microsoft.Xaml.Behaviors;

namespace MarkingDesigner.Behaviors
{
    public class ElementMoveBehavior : Behavior<Thumb>
    {
        public static readonly DependencyProperty PositionXProperty =
            DependencyProperty.Register("PositionX", typeof(double), typeof(ElementMoveBehavior),
                new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty PositionYProperty =
            DependencyProperty.Register("PositionY", typeof(double), typeof(ElementMoveBehavior),
                new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public double PositionX { get { return (double)GetValue(PositionXProperty); } set { SetValue(PositionXProperty, value); } }
        public double PositionY { get { return (double)GetValue(PositionYProperty); } set { SetValue(PositionYProperty, value); } }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.DragDelta += OnDragDelta;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.DragDelta -= OnDragDelta;
            base.OnDetaching();
        }

        private void OnDragDelta(object sender, DragDeltaEventArgs e)
        {
            // 画面DIP単位の移動量をmmに変換
            double factor = 25.4 / 96.0;
            PositionX += e.HorizontalChange * factor;
            // CanvasがY軸反転(ScaleY=-1)しているため、マウスの上移動(マイナス)がY座標のプラス
            PositionY -= e.VerticalChange * factor;
        }
    }
}