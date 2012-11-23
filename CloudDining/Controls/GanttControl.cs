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
    /// <summary>
    /// このカスタム コントロールを XAML ファイルで使用するには、手順 1a または 1b の後、手順 2 に従います。
    ///
    /// 手順 1a) 現在のプロジェクトに存在する XAML ファイルでこのカスタム コントロールを使用する場合
    /// この XmlNamespace 属性を使用場所であるマークアップ ファイルのルート要素に
    /// 追加します:
    ///
    ///     xmlns:MyNamespace="clr-namespace:SurfaceHelloWorld.Controls"
    ///
    ///
    /// 手順 1b) 異なるプロジェクトに存在する XAML ファイルでこのカスタム コントロールを使用する場合
    /// この XmlNamespace 属性を使用場所であるマークアップ ファイルのルート要素に
    /// 追加します:
    ///
    ///     xmlns:MyNamespace="clr-namespace:SurfaceHelloWorld.Controls;assembly=SurfaceHelloWorld.Controls"
    ///
    /// また、XAML ファイルのあるプロジェクトからこのプロジェクトへのプロジェクト参照を追加し、
    /// リビルドして、コンパイル エラーを防ぐ必要があります:
    ///
    ///     ソリューション エクスプローラーで対象のプロジェクトを右クリックし、
    ///     [参照の追加] の [プロジェクト] を選択してから、このプロジェクトを参照し、選択します。
    ///
    ///
    /// 手順 2)
    /// コントロールを XAML ファイルで使用します。
    ///
    ///     <MyNamespace:GanttControl/>
    ///
    /// </summary>
    public class GanttControl : ItemsControl
    {
        public GanttControl()
        {
            
        }
        static GanttControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(GanttControl), new FrameworkPropertyMetadata(typeof(GanttControl)));
        }
        Panel _wallpaperElement;

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _wallpaperElement = (Panel)Template.FindName("Wallpaper", this);
            UpdateRowPattern();
        }
        protected override void OnItemsChanged(System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            base.OnItemsChanged(e);

            var tmp = DateTime.MaxValue;
            foreach (GanttItem item in Items)
                tmp = item.StartTime < tmp
                    ? new DateTime(item.StartTime.Year, item.StartTime.Month, item.StartTime.Day, item.StartTime.Hour, item.StartTime.Minute < 30 ? 0 : 30, 0)
                    : tmp;
            SetDateTimeOffset(this, tmp);
            UpdateRowPattern();
        }
        void UpdateRowPattern()
        {
            if (_wallpaperElement == null)
                return;

            var minIndex = DateTime.MaxValue;
            var maxIndex = DateTime.MinValue;
            foreach (GanttItem item in Items)
            {
                DateTime tmp;
                if (minIndex > (tmp = new DateTime(item.StartTime.Year, item.StartTime.Month, item.StartTime.Day, item.StartTime.Hour, item.StartTime.Minute < 30 ? 0 : 30, 0)))
                    minIndex = tmp;
                if (maxIndex < (tmp = new DateTime(item.EndTime.Year, item.EndTime.Month, item.EndTime.Day, item.EndTime.Minute < 30 ? item.EndTime.Hour : item.EndTime.Hour + 1, item.EndTime.Minute < 30 ? 30 : 00, 0)))
                    maxIndex = tmp;
            }
            var len = Math.Max((int)(maxIndex - minIndex).TotalMinutes / 30 + 1, 0);
            var dateGrid = GetDateTimeOffset(this);
            _wallpaperElement.Height = len * GetItemInterval(this);
            _wallpaperElement.Children.Clear();
            for (var i = 0; i < len; i++)
                _wallpaperElement.Children.Add(new Border()
                {
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch,
                    VerticalAlignment = System.Windows.VerticalAlignment.Center,
                    Height = GanttControl.GetItemInterval(this),
                    Child = new TextBlock()
                    {
                        Foreground = Brushes.Black,
                        HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
                        VerticalAlignment = System.Windows.VerticalAlignment.Center,
                        TextAlignment = TextAlignment.Center,
                        Text = dateGrid.AddMinutes(30 * i).ToString("HH:mm"),
                        Margin = new Thickness(10, 0, 0, 0),
                    },
                });
        }

        public static double GetItemInterval(DependencyObject obj)
        {
            return (double)obj.GetValue(ItemIntervalProperty);
        }
        public static void SetItemInterval(DependencyObject obj, double value)
        {
            obj.SetValue(ItemIntervalProperty, value);
        }
        public static readonly DependencyProperty ItemIntervalProperty = DependencyProperty.RegisterAttached(
            "ItemInterval", typeof(double), typeof(GanttControl),
            new FrameworkPropertyMetadata(30.0, FrameworkPropertyMetadataOptions.Inherits));
        public static DateTime GetDateTimeOffset(DependencyObject obj)
        { return (DateTime)obj.GetValue(DateTimeOffsetProperty); }
        public static void SetDateTimeOffset(DependencyObject obj, DateTime value)
        { obj.SetValue(DateTimeOffsetProperty, value); }
        public static readonly DependencyProperty DateTimeOffsetProperty = DependencyProperty.RegisterAttached(
            "DateTimeOffset", typeof(DateTime), typeof(GanttControl),
            new FrameworkPropertyMetadata(DateTime.MinValue, FrameworkPropertyMetadataOptions.Inherits));
    }
    public class GanttItem : ContentControl
    {
        static GanttItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(GanttItem), new FrameworkPropertyMetadata(typeof(GanttItem)));
        }

        public DateTime StartTime
        {
            get { return (DateTime)GetValue(StartTimeProperty); }
            set { SetValue(StartTimeProperty, value); }
        }
        public DateTime EndTime
        {
            get { return (DateTime)GetValue(EndTimeProperty); }
            set { SetValue(EndTimeProperty, value); }
        }
        public Uri HeadIcon
        {
            get { return (Uri)GetValue(HeadIconProperty); }
            set { SetValue(HeadIconProperty, value); }
        }

        public static readonly DependencyProperty StartTimeProperty = DependencyProperty.Register(
            "StartTime", typeof(DateTime), typeof(GanttItem), new FrameworkPropertyMetadata(DateTime.MinValue, FrameworkPropertyMetadataOptions.AffectsMeasure));
        public static readonly DependencyProperty EndTimeProperty = DependencyProperty.Register(
            "EndTime", typeof(DateTime), typeof(GanttItem), new FrameworkPropertyMetadata(DateTime.MinValue, FrameworkPropertyMetadataOptions.AffectsMeasure));
        public static readonly DependencyProperty HeadIconProperty = DependencyProperty.Register(
            "HeadIcon", typeof(Uri), typeof(GanttItem), new UIPropertyMetadata(null));
    }
}
