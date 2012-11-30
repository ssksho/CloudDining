using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CloudDining.Controls
{
    public class ExScatterViewItem : Microsoft.Surface.Presentation.Controls.ScatterViewItem
    {
        static ExScatterViewItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(ExScatterViewItem), new FrameworkPropertyMetadata(typeof(ExScatterViewItem)));
        }

        public event DependencyPropertyChangedEventHandler CenterChanged;
        protected override System.Windows.Size MeasureOverride(System.Windows.Size availableSize)
        {
            UIElement child;
            if ((child = GetVisualChild(0) as UIElement) == null)
                return base.MeasureOverride(availableSize);
            else
            {
                child.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                return child.DesiredSize;
            }
        }
        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (CenterChanged != null && e.Property == ExScatterViewItem.CenterProperty)
                CenterChanged(this, e);
        }
    }
}
