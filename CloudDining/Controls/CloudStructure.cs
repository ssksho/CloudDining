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
    ///     xmlns:MyNamespace="clr-namespace:SurfaceHelloWorld"
    ///
    ///
    /// 手順 1b) 異なるプロジェクトに存在する XAML ファイルでこのカスタム コントロールを使用する場合
    /// この XmlNamespace 属性を使用場所であるマークアップ ファイルのルート要素に
    /// 追加します:
    ///
    ///     xmlns:MyNamespace="clr-namespace:SurfaceHelloWorld;assembly=SurfaceHelloWorld"
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
    ///     <MyNamespace:CloudStructure/>
    ///
    /// </summary>
    public class CloudStructure : Control
    {
        static CloudStructure()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(CloudStructure), new FrameworkPropertyMetadata(typeof(CloudStructure)));
        }

        public int CloudTypeId
        {
            get { return (int)GetValue(CloudTypeIdProperty); }
            set { SetValue(CloudTypeIdProperty, value); }
        }
        public CloudStateType CloudStatus
        {
            get { return (CloudStateType)GetValue(CloudStatusProperty); }
            set { SetValue(CloudStatusProperty, value); }
        }

        public static readonly DependencyProperty CloudTypeIdProperty = DependencyProperty.Register(
            "CloudTypeId", typeof(int), typeof(CloudStructure), new UIPropertyMetadata(15));
        public static readonly DependencyProperty CloudStatusProperty = DependencyProperty.Register(
            "CloudStatus", typeof(CloudStateType), typeof(CloudStructure), new UIPropertyMetadata(CloudStateType.Sunny));
    }
    public enum CloudStateType { Sunny, Cloudy, Rainy }
    public class CloudTypeIdToUriConverter : System.Windows.Data.IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (values[0] == DependencyProperty.UnsetValue
                || values[1] == DependencyProperty.UnsetValue)
                return null;

            var url = new Uri(
                string.Format("pack://application:,,,/Resources/Clouds/cloudImage{0:00}_{1}.png",
                Math.Max(Math.Min((int)values[0], 29), 0), Math.Max(Math.Min((int)values[1], 3), 0)), UriKind.Absolute);
            return new BitmapImage(url);
        }
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        { throw new NotImplementedException(); }
    }

}