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
            detailScatter.CenterChanged += detailScatter_CenterChanged;
            pictureScatter.CenterChanged += detailScatter_CenterChanged;

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
            var zikanKaitenHiritsu = TimeSpan.FromMinutes(30);
            var kaiten = len.TotalSeconds / zikanKaitenHiritsu.TotalSeconds;
            var timelineAnime = new System.Windows.Media.Animation.DoubleAnimation(0, 360 * kaiten, (Duration)len);
            System.Windows.Media.Animation.Storyboard.SetTarget(timelineAnime, timeshiftDram);
            System.Windows.Media.Animation.Storyboard.SetTargetProperty(timelineAnime, new PropertyPath(Controls.DramControl.SubAngleOffsetProperty));
            _timelineStoryboard = new System.Windows.Media.Animation.Storyboard();
            _timelineStoryboard.Children.Add(timelineAnime);
            _timelineStoryboard.Begin(this, true);
            //timeshiftDram.BeginAnimation(Controls.DramControl.SubAngleOffsetProperty, _timelineStoryboard);
            MinutesToAngleRate = 360 * kaiten / len.TotalMinutes;
        }
        const double LATEST_OFFSET_LINE = 20;
        double MinutesToAngleRate;
        Random _rnd;
        DateTime _startAppTime;
        FieldManager _fieldManager;
        System.Windows.Media.Animation.Storyboard _timelineStoryboard;
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
                        foreach (var item in e.NewItems.OfType<PlaneNode>())
                        {
                            var grid = new Controls.PlaneControl() { PlaneStatus = Controls.PlaneStateType.Right };
                            grid.Width = 100;
                            grid.Height = 100;
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
                                Content = new SurfaceButton() { Content = grid, Background = null, Style = (Style)FindResource("surfaceTemplate"), },
                                Angle = (item.RaiseTime - _startAppTime).TotalMinutes * MinutesToAngleRate + LATEST_OFFSET_LINE,
                                Track = _rnd.Next(20) * 30,
                            };
                            ((SurfaceButton)dramItem.Content).Click += DramItem_Click;
                            ((SurfaceButton)dramItem.Content).Tag = dramItem;
                            dramItem.Tag = item;
                            item.TimeshiftElement = dramItem;
                            timeshiftDram.Items.Add(dramItem);
                        }
                        foreach (var item in e.NewItems.OfType<PlaneNode>())
                        {
                            var dramItem = new Controls.DramItem()
                            {
                                Content = new SurfaceButton()
                                {
                                    Content = new Image()
                                    {
                                        Height = 130,
                                        Source = new BitmapImage(item.Picture),
                                        Effect = (System.Windows.Media.Effects.Effect)FindResource("dropShadowEffectB"),
                                    },
                                    Background = null,
                                    Style = (Style)FindResource("surfaceTemplate"),
                                },
                                Angle = (item.RaiseTime - _startAppTime).TotalMinutes * MinutesToAngleRate + LATEST_OFFSET_LINE,
                                Track = _rnd.Next(20) * 30,
                            };
                            ((SurfaceButton)dramItem.Content).Click += DramItem_Click;
                            ((SurfaceButton)dramItem.Content).Tag = dramItem;
                            dramItem.Tag = item;
                            dramItem.BeginAnimation(
                                Control.OpacityProperty, new System.Windows.Media.Animation.DoubleAnimation(0.0, 1.0,
                                (Duration)TimeSpan.FromMilliseconds(500)));
                            item.TimeshiftElement = dramItem;
                            timeshiftDram.Items.Add(dramItem);
                        }
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        foreach (var item in e.OldItems.OfType<ComplexCloudNode>())
                            item.ChildrenChanged -= CloudComplex_ChildrenChanged;
                        foreach (var item in e.OldItems.OfType<BaseNode>())
                        {
                            var dramItem = item.TimeshiftElement;
                            timeshiftDram.Items.Remove(dramItem);
                        }
                        break;
                }
            }));
        }
        void detailScatter_CenterChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var point = (Point)e.NewValue;
            if (detailScatter.IsMouseOver == false)
            {
                var width = detailPanelContainer.ActualWidth;
                var height = detailPanelContainer.ActualHeight;
                if (width - point.X < 20 || height - point.Y < 20 || point.X < 20 || point.Y < 20)
                {
                    var bbb = TimeSpan.FromMilliseconds(50);
                    var aaa = new System.Windows.Media.Animation.ObjectAnimationUsingKeyFrames();
                    aaa.KeyFrames.Add(new System.Windows.Media.Animation.DiscreteObjectKeyFrame(Visibility.Visible, System.Windows.Media.Animation.KeyTime.FromTimeSpan(TimeSpan.Zero)));
                    aaa.KeyFrames.Add(new System.Windows.Media.Animation.DiscreteObjectKeyFrame(Visibility.Hidden, System.Windows.Media.Animation.KeyTime.FromTimeSpan(bbb)));
                    detailPanelContainer.BeginAnimation(Control.VisibilityProperty,  aaa, System.Windows.Media.Animation.HandoffBehavior.SnapshotAndReplace);
                    detailPanelContainer.BeginAnimation(Control.OpacityProperty, new System.Windows.Media.Animation.DoubleAnimation(0.0, (Duration)bbb), System.Windows.Media.Animation.HandoffBehavior.SnapshotAndReplace);
                    _timelineStoryboard.Resume(this);
                }
            }
        }
        void CloudComplex_ChildrenChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Dispatcher.Invoke((Action)(() =>
            {
                var complexCloud = (ComplexCloudNode)sender;
                var dramItem = (Controls.DramItem)complexCloud.TimeshiftElement;
                var grid = (Grid)((SurfaceButton)dramItem.Content).Content;

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
                                Effect = complexCloud.Children.Count == 1 ? null : (System.Windows.Media.Effects.DropShadowEffect)FindResource("dropShadowEffectA")
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
        void DramItem_Click(object sender, RoutedEventArgs e)
        {
            var dramItem = (Controls.DramItem)((SurfaceButton)sender).Tag;
            if (dramItem.Tag is ComplexCloudNode)
            {
                var complexCloud = (ComplexCloudNode)dramItem.Tag;
                pictureScatter.Visibility = System.Windows.Visibility.Collapsed;
                detailScatter.Visibility = System.Windows.Visibility.Visible;
                detailScatter.Center = new Point(900, 500);
                detailScatter.Orientation = 0.0;
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
            else if (dramItem.Tag is PlaneNode)
            {
                var plane = (PlaneNode)dramItem.Tag;
                detailScatter.Visibility = System.Windows.Visibility.Collapsed;
                pictureScatter.Visibility = System.Windows.Visibility.Visible;
                pictureScatter.Center = new Point(900, 500);
                pictureScatter.Orientation = 0.0;
                detailImage.Source = new BitmapImage(plane.Picture);
            }
            _timelineStoryboard.Pause(this);

            var bbb = TimeSpan.FromMilliseconds(150);
            var aaa = new System.Windows.Media.Animation.ObjectAnimationUsingKeyFrames();
            aaa.KeyFrames.Add(new System.Windows.Media.Animation.DiscreteObjectKeyFrame(Visibility.Visible, System.Windows.Media.Animation.KeyTime.FromTimeSpan(TimeSpan.Zero)));
            detailPanelContainer.BeginAnimation(Control.VisibilityProperty, aaa, System.Windows.Media.Animation.HandoffBehavior.SnapshotAndReplace);
            detailPanelContainer.BeginAnimation(Control.OpacityProperty, new System.Windows.Media.Animation.DoubleAnimation(0.0, 1.0, (Duration)bbb), System.Windows.Media.Animation.HandoffBehavior.SnapshotAndReplace);
        }
        void SurfaceButton_Click(object sender, RoutedEventArgs e)
        {
            DataCacheDictionary.DownloadUserIcon(new Uri("https://lh3.googleusercontent.com/-_EKZ1xMSe8M/UEiC5bvR5jI/AAAAAAAACzE/nuFW2QY647c/s576/06+-+1"))
                .ContinueWith(tsk =>
                    {
                        _fieldManager.PostPlane(
                            new PlaneNode(tsk.Result, _fieldManager.Users.First(), null));
                    });
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