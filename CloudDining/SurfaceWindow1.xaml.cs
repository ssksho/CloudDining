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
using System.Windows.Threading;
using Microsoft.Surface;
using Microsoft.Surface.Presentation;
using Microsoft.Surface.Presentation.Controls;
using Microsoft.Surface.Presentation.Input;

namespace CloudDining
{
    using Model;

    public partial class SurfaceWindow1 : SurfaceWindow
    {
        public SurfaceWindow1()
        {
            InitializeComponent();
            AddWindowAvailabilityHandlers();

            _loginUserSelecterDevice = new List<Tuple<InputDevice, System.Windows.Threading.DispatcherTimer>>();
            _loginUserSelecterPoint = new Dictionary<Tuple<InputDevice, System.Windows.Threading.DispatcherTimer>, Point>();
            _loginUserSelecterMenu = new Dictionary<Tuple<InputDevice, System.Windows.Threading.DispatcherTimer>, ElementMenu>();

            _fieldManager = new FieldManager();
            DataContext = _fieldManager;
        }
        FieldManager _fieldManager;
        List<Tuple<InputDevice, DispatcherTimer>> _loginUserSelecterDevice;
        Dictionary<Tuple<InputDevice, DispatcherTimer>, ElementMenu> _loginUserSelecterMenu;
        Dictionary<Tuple<InputDevice, DispatcherTimer>, Point> _loginUserSelecterPoint;

        //ログイン時のユーザ選択の実装
        void backGrid_PreviewInputDown(object sender, InputEventArgs e)
        {
            var position = e.Device.GetPosition(backGrid);
            var timer = new System.Windows.Threading.DispatcherTimer();
            var tuple = new Tuple<InputDevice, System.Windows.Threading.DispatcherTimer>(e.Device, timer);
            _loginUserSelecterDevice.Add(tuple);
            _loginUserSelecterPoint.Add(tuple, position);
            timer.Interval = TimeSpan.FromMilliseconds(1000);
            timer.Tick += timer_Tick;
            timer.Tag = tuple;
            timer.Start();
        }
        void backGrid_PreviewInputMove(object sender, InputEventArgs e)
        {
            var position = e.Device.GetPosition(backGrid);
            var tuple = _loginUserSelecterDevice.FirstOrDefault(obj => obj.Item1 == e.Device);
            if (tuple == null)
                return;
            var point = _loginUserSelecterPoint[tuple];
            if (Math.Abs(position.X - point.X) > 5 || Math.Abs(position.Y - point.Y) > 5)
            {
                tuple.Item2.Stop();
                _loginUserSelecterMenu.Remove(tuple);
                _loginUserSelecterPoint.Remove(tuple);
                _loginUserSelecterDevice.Remove(tuple);
            }
        }
        void backGrid_PreviewInputUp(object sender, InputEventArgs e)
        {
            var tuple = _loginUserSelecterDevice.FirstOrDefault(obj => obj.Item1 == e.Device);
            if (tuple == null)
                return;
            tuple.Item2.Stop();
            _loginUserSelecterMenu.Remove(tuple);
            _loginUserSelecterPoint.Remove(tuple);
            _loginUserSelecterDevice.Remove(tuple);
        }
        void timer_Tick(object sender, EventArgs e)
        {
            var timer = (System.Windows.Threading.DispatcherTimer)sender;
            timer.Stop();
            var tuple = (Tuple<InputDevice, System.Windows.Threading.DispatcherTimer>)timer.Tag;
            if (_loginUserSelecterDevice.Remove(tuple) == false)
                return;

            _loginUserSelecterMenu.Remove(tuple);
            _loginUserSelecterPoint.Remove(tuple);

            var position = tuple.Item1.GetPosition(backGrid);
            loginUserSelecter.Visibility = System.Windows.Visibility.Visible;
            loginUserSelecterTranslater.X = position.X;
            loginUserSelecterTranslater.Y = position.Y;
            loginUserSelecter.IsSubmenuOpen = true;
            tuple.Item1.Capture(loginUserSelecter, CaptureMode.SubTree);
        }
        void ElementMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var obj = (ElementMenuItem)sender;
            loginUserDisplay.Text = string.Format("{0}がログインしました。", obj.Header);
        }
        void loginUserSelecter_SubmenuClosed(object sender, RoutedEventArgs e)
        {
            loginUserSelecter.Visibility = System.Windows.Visibility.Hidden;
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            // Remove handlers for window availability events
            RemoveWindowAvailabilityHandlers();
        }
        private void AddWindowAvailabilityHandlers()
        {
            // Subscribe to surface window availability events
            ApplicationServices.WindowInteractive += OnWindowInteractive;
            ApplicationServices.WindowNoninteractive += OnWindowNoninteractive;
            ApplicationServices.WindowUnavailable += OnWindowUnavailable;
        }
        private void RemoveWindowAvailabilityHandlers()
        {
            // Unsubscribe from surface window availability events
            ApplicationServices.WindowInteractive -= OnWindowInteractive;
            ApplicationServices.WindowNoninteractive -= OnWindowNoninteractive;
            ApplicationServices.WindowUnavailable -= OnWindowUnavailable;
        }
        private void OnWindowInteractive(object sender, EventArgs e)
        {
            //TODO: enable audio, animations here
        }
        private void OnWindowNoninteractive(object sender, EventArgs e)
        {
            //TODO: Disable audio here if it is enabled

            //TODO: optionally enable animations here
        }
        private void OnWindowUnavailable(object sender, EventArgs e)
        {
            //TODO: disable audio, animations here
        }
    }
}