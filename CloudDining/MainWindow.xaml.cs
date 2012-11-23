using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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

    public partial class MainWindow : SurfaceWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            AddWindowAvailabilityHandlers();

            _loginUserSelecterDevice = new List<Tuple<InputDevice, System.Windows.Threading.DispatcherTimer>>();
            _loginUserSelecterPoint = new Dictionary<Tuple<InputDevice, System.Windows.Threading.DispatcherTimer>, Point>();
            _loginUserSelecterMenu = new Dictionary<Tuple<InputDevice, System.Windows.Threading.DispatcherTimer>, ElementMenu>();

            _rnd = new Random();
            _startAppTime = DateTime.Now;
            _fieldManager = new FieldManager(Dispatcher);
            _fieldManager.HomeNodesChanged += _fieldManager_HomeNodesChanged;
            _fieldManager.TimelineNodesChanged += _fieldManager_TimelineNodesChanged;
            DataContext = _fieldManager;

            var len = TimeSpan.FromHours(8);
            var aaa = TimeSpan.FromMinutes(10);
            var kaiten = len.TotalSeconds / aaa.TotalSeconds;
            timeshiftDram.BeginAnimation(
                Controls.DramControl.SubAngleOffsetProperty,
                new System.Windows.Media.Animation.DoubleAnimation(0, 360 * kaiten, (Duration)len));
            MinutesToAngleRate = 360 * kaiten / len.TotalMinutes;
        }
        const double LATEST_OFFSET_LINE = -20;
        double MinutesToAngleRate;
        Random _rnd;
        DateTime _startAppTime;
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
        void loginUserSelecter_SubmenuClosed(object sender, RoutedEventArgs e)
        {
            loginUserSelecter.Visibility = System.Windows.Visibility.Hidden;
        }

        void _fieldManager_HomeNodesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Dispatcher.Invoke((Action)(() =>
            {
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        foreach (var item in e.NewItems.OfType<ComplexCloudNode>())
                        {
                            var grid = new Grid();
                            grid.Width = 100;
                            grid.Height = 100;
                            grid.Background = Brushes.Aqua;
                            item.ChildrenChanged += CloudComplex_ChildrenChanged;
                            item.HomeElement = grid;
                            homeGrid.Children.Add(grid);
                        }
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        foreach (var item in e.OldItems.OfType<ComplexCloudNode>())
                        {
                            var dramItem = item.HomeElement;
                            homeGrid.Children.Remove(dramItem);
                            item.ChildrenChanged -= CloudComplex_ChildrenChanged;
                        }
                        break;
                }
            }));
        }
        void _fieldManager_TimelineNodesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Dispatcher.Invoke((Action)(() =>
            {
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        foreach (var item in e.NewItems.OfType<ComplexCloudNode>())
                        {
                            item.ChildrenChanged += CloudComplex_ChildrenChanged;
                            var grid = new Grid();
                            var dramItem = new Controls.DramItem()
                            {
                                Content = grid,
                                Angle = (_startAppTime - item.RaiseTime).TotalMinutes * MinutesToAngleRate + LATEST_OFFSET_LINE,
                                Track = _rnd.Next(20) * 30,
                            };
                            dramItem.MouseUp += dramItem_MouseUp;
                            dramItem.Tag = item;
                            item.TimeshiftElement = dramItem;
                            timeshiftDram.Items.Add(dramItem);
                        }
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        foreach (var item in e.OldItems.OfType<ComplexCloudNode>())
                        {
                            var dramItem = item.TimeshiftElement;
                            timeshiftDram.Items.Remove(dramItem);
                            item.ChildrenChanged -= CloudComplex_ChildrenChanged;
                        }
                        break;
                }
            }));
        }

        void dramItem_MouseUp(object sender, MouseButtonEventArgs e)
        {
            var dramItem = (Controls.DramItem)sender;
            var complexCloud = (ComplexCloudNode)dramItem.Tag;

            detailPanel.Items.Clear();
            foreach (var item in complexCloud.Children)
                detailPanel.Items.Add(new Controls.GanttItem()
                {
                    StartTime = item.CheckinTime,
                    //EndTime = item.CheckinTime.Add(item.CheckinSpan),
                    EndTime = item.CheckinTime.AddMinutes(30),
                    HeadIcon = item.Owner.Icon,
                });
        }
        void CloudComplex_ChildrenChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Dispatcher.Invoke((Action)(() =>
            {
                var complexCloud = (ComplexCloudNode)sender;
                var dramItem = (Controls.DramItem)complexCloud.TimeshiftElement;
                var grid = (Grid)dramItem.Content;

                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        foreach (var item in e.NewItems.Cast<CloudNode>())
                        {
                            var cloudStructure = new Controls.CloudStructure()
                            {
                                Width = 200,
                                Height = 150,
                                CloudTypeId = item.CloudTypeId,
                                CloudStatus = item.Status,
                                RenderTransform = new TranslateTransform(
                                    complexCloud.Children.Count * 15,
                                    complexCloud.Children.Count * 15),
                                Tag = item,
                            };
                            item.TimeshiftElement = cloudStructure;
                            grid.Children.Add(cloudStructure);
                            cloudStructure.BeginAnimation(
                                Control.OpacityProperty,
                                new System.Windows.Media.Animation.DoubleAnimation(0.0, 1.0, new Duration(TimeSpan.FromMilliseconds(1000))));
                        }
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        foreach (var item in e.OldItems.Cast<CloudNode>())
                            grid.Children.Remove(item.TimeshiftElement);
                        break;
                }
            }));
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            // Remove handlers for window availability events
            RemoveWindowAvailabilityHandlers();
        }
        void AddWindowAvailabilityHandlers()
        {
            // Subscribe to surface window availability events
            ApplicationServices.WindowInteractive += OnWindowInteractive;
            ApplicationServices.WindowNoninteractive += OnWindowNoninteractive;
            ApplicationServices.WindowUnavailable += OnWindowUnavailable;
        }
        void RemoveWindowAvailabilityHandlers()
        {
            // Unsubscribe from surface window availability events
            ApplicationServices.WindowInteractive -= OnWindowInteractive;
            ApplicationServices.WindowNoninteractive -= OnWindowNoninteractive;
            ApplicationServices.WindowUnavailable -= OnWindowUnavailable;
        }
        void OnWindowInteractive(object sender, EventArgs e)
        {
            //TODO: enable audio, animations here
        }
        void OnWindowNoninteractive(object sender, EventArgs e)
        {
            //TODO: Disable audio here if it is enabled

            //TODO: optionally enable animations here
        }
        void OnWindowUnavailable(object sender, EventArgs e)
        {
            //TODO: disable audio, animations here
        }
    }
}