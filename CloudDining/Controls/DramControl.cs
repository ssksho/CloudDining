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
    ///     <MyNamespace:DramControl/>
    ///
    /// </summary>
    public class DramControl : ItemsControl
    {
        public DramControl()
        {
            ManipulationDelta += DramControl_ManipulationDelta;
            ManipulationInertiaStarting += DramControl_ManipulationInertiaStarting;
        }
        static DramControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(DramControl), new FrameworkPropertyMetadata(typeof(DramControl)));
        }
        const double radian2Angle = 180 / Math.PI;
        const double angle2Radian = Math.PI / 180;
        public double AngleOffset
        {
            get { return (double)GetValue(AngleOffsetProperty); }
            set { SetValue(AngleOffsetProperty, value); }
        }
        public double SubAngleOffset
        {
            get { return (double)GetValue(SubAngleOffsetProperty); }
            set { SetValue(SubAngleOffsetProperty, value); }
        }
        public double ArcWidth
        {
            get { return (double)GetValue(ArcWidthProperty); }
            set { SetValue(ArcWidthProperty, value); }
        }
        public double ArcHeight
        {
            get { return (double)GetValue(ArcHeightProperty); }
            set { SetValue(ArcHeightProperty, value); }
        }
        public double Maximum
        {
            get { return (double)GetValue(MaximumProperty); }
            set { SetValue(MaximumProperty, value); }
        }
        public double Minimum
        {
            get { return (double)GetValue(MinimumProperty); }
            set { SetValue(MinimumProperty, value); }
        }
        
	    protected override void PrepareContainerForItemOverride(DependencyObject element, object item) {  
	       var cont = element as DramItem;  
	       if(element != item){
	           if(cont != null){  
	               cont.Content = item;  
	               cont.DataContext = item;  
	           }  
	       }  
	    }
        protected override DependencyObject GetContainerForItemOverride() { return new DramItem(); }
        void UpdateItemPosition()
        {
            var aaa = 180 - Math.Atan(ArcHeight / (ArcWidth / 2)) * radian2Angle - 90;
            var bbb = 180 - aaa * 2;
            var ccc = Math.Tan((180 - bbb - 90) * angle2Radian) * (ArcWidth / 2);
            SetArcRadius(this, ArcHeight + ccc);
        }
        void DramControl_ManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            AngleOffset += e.DeltaManipulation.Translation.X * 0.05;
        }
        void DramControl_ManipulationInertiaStarting(object sender, ManipulationInertiaStartingEventArgs e)
        {
            e.TranslationBehavior.DesiredDeceleration = 0.002;
        }

        public static readonly DependencyProperty AngleOffsetProperty = DependencyProperty.RegisterAttached(
            "AngleOffset", typeof(double), typeof(DramControl), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.Inherits, Changed_AngleOffset, CoerceValue_AngleOffset));
        public static readonly DependencyProperty SubAngleOffsetProperty = DependencyProperty.RegisterAttached(
            "SubAngleOffset", typeof(double), typeof(DramControl), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.Inherits, Changed_AngleOffset));
        public static readonly DependencyProperty ArcWidthProperty = DependencyProperty.Register(
            "ArcWidth", typeof(double), typeof(DramControl), new UIPropertyMetadata(0.0, Changed_ArcWidthHeight));
        public static readonly DependencyProperty ArcHeightProperty = DependencyProperty.Register(
            "ArcHeight", typeof(double), typeof(DramControl), new UIPropertyMetadata(100.0, Changed_ArcWidthHeight));
        public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register(
            "Maximum", typeof(double), typeof(DramControl), new UIPropertyMetadata(180.0));
        public static readonly DependencyProperty MinimumProperty = DependencyProperty.Register(
            "Minimum", typeof(double), typeof(DramControl), new UIPropertyMetadata(-180.0));
        static readonly DependencyPropertyKey ArcRadiusPropertyKey = DependencyProperty.RegisterAttachedReadOnly(
            "ArcRadius", typeof(double), typeof(DramControl), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.Inherits));
        public static readonly DependencyProperty ArcRadiusProperty = ArcRadiusPropertyKey.DependencyProperty;

        public static double GetAngleOffset(DependencyObject target)
        { return (double)target.GetValue(AngleOffsetProperty); }
        public static void SetAngleOffset(DependencyObject target, double value)
        { target.SetValue(AngleOffsetProperty, value); }
        public static double GetSubAngleOffset(DependencyObject target)
        { return (double)target.GetValue(SubAngleOffsetProperty); }
        public static void SetSubAngleOffset(DependencyObject target, double value)
        { target.SetValue(SubAngleOffsetProperty, value); }
        public static double GetArcRadius(DependencyObject target)
        { return (double)target.GetValue(ArcRadiusProperty); }
        static void SetArcRadius(DependencyObject target, double value)
        { target.SetValue(ArcRadiusPropertyKey, value); }
        static void Changed_AngleOffset(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (!(sender is DramControl))
                return;

            var obj = (DramControl)sender;
            DramItem child;
            for (int i = 0; (child = (DramItem)obj.ItemContainerGenerator.ContainerFromIndex(i)) != null; i++)
            {
                child.UpdateVisibility();
                var aaa = LogicalTreeHelper.GetParent(child);
            }
        }
        static void Changed_ArcWidthHeight(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var obj = (DramControl)sender;
            obj.UpdateItemPosition();
            DramItem child;
            for (int i = 0; (child = (DramItem)obj.ItemContainerGenerator.ContainerFromIndex(i)) != null; i++)
			{
                child.UpdateArcRadius();
			}
        }
        static object CoerceValue_AngleOffset(DependencyObject sender, object value)
        {
            if (sender is DramControl == false)
                return value;

            var dramCtrl = sender as DramControl;
            var val = (double)value;
            var res = Math.Min(dramCtrl.Maximum, Math.Max(val, dramCtrl.Minimum));
            return res;
        }
    }
    [System.Windows.Markup.ContentProperty("Content")]
    public class DramItem : ContentPresenter
    {
        public DramItem()
        {
            Loaded += DramItem_Loaded;
        }
        static DramItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(DramItem), new FrameworkPropertyMetadata(typeof(DramItem)));
        }

        public double Angle
        {
            get { return (double)GetValue(AngleProperty); }
            set { SetValue(AngleProperty, value); }
        }
        public double Track
        {
            get { return (double)GetValue(TrackProperty); }
            set { SetValue(TrackProperty, value); }
        }
        public double ArcRadius
        {
            get { return (double)GetValue(ArcRadiusProperty); }
            set { SetValue(ArcRadiusPropertyKey, value); }
        }
        public void UpdateArcRadius()
        {
            var aaa = VisualTreeHelper.GetParent(this);
            if (Parent == null)
                return;
            ArcRadius = DramControl.GetArcRadius(this) + Track + ActualHeight;
        }
        public void UpdateVisibility()
        {
            Visibility = Math.Abs(Angle - DramControl.GetAngleOffset(this) - DramControl.GetSubAngleOffset(this)) < 180
                ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
        }
        void DramItem_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateArcRadius();
            UpdateVisibility();
        }

        public static readonly DependencyProperty AngleProperty = DependencyProperty.Register(
            "Angle", typeof(double), typeof(DramItem), new UIPropertyMetadata(0.0, Changed_Angle));
        public static readonly DependencyProperty TrackProperty = DependencyProperty.Register(
            "Track", typeof(double), typeof(DramItem), new UIPropertyMetadata(0.0, Changed_Track));
        static readonly DependencyPropertyKey ArcRadiusPropertyKey = DependencyProperty.RegisterReadOnly(
            "ArcRadius", typeof(double), typeof(DramItem), new UIPropertyMetadata(0.0));
        public static readonly DependencyProperty ArcRadiusProperty = ArcRadiusPropertyKey.DependencyProperty;

        static void Changed_Angle(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender == null)
                return;

            var obj = (DramItem)sender;
            obj.UpdateVisibility();
        }
        static void Changed_Track(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender == null)
                return;
            
            var obj = (DramItem)sender;
            obj.UpdateArcRadius();
        }
    }
    public class CalcConverter : System.Windows.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is double)
                switch ((string)parameter)
                {
                    case "half":
                        return (double)value / 2;
                    case "signInversion":
                        return -(double)value;
                    default:
                        throw new ArgumentException("未対応のパラメータです。");
                }
            else
                throw new ArgumentException("浮動小数点のみ対応です。");
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        { throw new NotImplementedException(); }
    }
    public class MultiCalcConverter : System.Windows.Data.IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (values.Any(obj => obj == DependencyProperty.UnsetValue))
                return DependencyProperty.UnsetValue;

            if ((string)parameter == "ganttLine")
            {
                var length = ((DateTime)values[0] - (DateTime)values[1]).TotalMinutes / 30;
                var interval = (double)values[2];
                return length * interval;
            }
            else
            {
                var vals = new List<double>();
                foreach (object val in values)
                    //box化されたintがdoubleへ直接キャストできないのでいったんunbox化した後にdoubleにする
                    vals.Add(val is int ? (double)(int)val : (double)val);
                switch ((string)parameter)
                {
                    case "sum":
                        return vals.Cast<double>().Sum();
                    case "product":
                        return vals.Aggregate((val, tmp) => val * tmp);
                    default:
                        throw new NotImplementedException();
                }
            }
        }
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        { throw new NotImplementedException(); }
    }
    public class SizeConverter : System.Windows.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double tmp;
            double width = 0;
            double height = 0;
            if (double.TryParse(((string)parameter).Substring(2), out tmp))
            {
                width = (double)value + tmp;
                height = (double)value + tmp;
            }
            switch (((string)parameter)[1])
            {
                case 'a':
                    break;
                case 'w':
                    height = 0;
                    break;
                case 'h':
                    width = 0;
                    break;
                default:
                    throw new NotImplementedException();
            }
            switch (((string)parameter)[0])
            {
                case 'p':
                    return new Point(width, height);
                case 's':
                    return new Size(width, height);
                default:
                    throw new NotImplementedException();
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        { throw new NotImplementedException(); }
    }
}
