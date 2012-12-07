﻿using System;
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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using CloudDining.Controls;
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
            _fieldManager.TimelineNodesChanged += _fieldManager_TimeshiftNodesChanged;
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
            MinutesToAngleRate = 360 * kaiten / len.TotalMinutes;
        }
        const double LATEST_OFFSET_LINE = 20;
        double MinutesToAngleRate;
        Random _rnd;
        BaseNode _openingNode;
        DateTime _startAppTime;
        FieldManager _fieldManager;
        Storyboard _timelineStoryboard;
        List<Tuple<InputDevice, DispatcherTimer>> _loginUserSelecterDevice;
        Dictionary<Tuple<InputDevice, DispatcherTimer>, ElementMenu> _loginUserSelecterMenu;
        Dictionary<Tuple<InputDevice, DispatcherTimer>, Point> _loginUserSelecterPoint;
        //佐々木追加
        List<Storyboard> storyboards = new List<Storyboard>();
        Storyboard storyboard;
        CloudStructure cloud;
        PlaneControl plane;
        int count;
        int[] hw = new int[4] { 0, 0, 0, 0 };

        public void ShowPopupInfo(ComplexCloudNode node)
        {
            node.Pause();
            node.Open();
            _openingNode = node;
            pictureScatter.Visibility = System.Windows.Visibility.Collapsed;
            detailScatter.Visibility = System.Windows.Visibility.Visible;
            detailScatter.Center = new Point(900, 500);
            detailScatter.Orientation = 0.0;
            detailPanel.Items.Clear();
            foreach (var item in node.Children)
                detailPanel.Items.Add(new Controls.GanttItem()
                {
                    StartTime = item.CheckinTime,
                    //EndTime = item.CheckinTime.Add(item.CheckinSpan),
                    EndTime = item.CheckinTime.AddMinutes(30),
                    HeadIcon = item.Owner.Icon,
                });

            var bbb = TimeSpan.FromMilliseconds(150);
            var aaa = new System.Windows.Media.Animation.ObjectAnimationUsingKeyFrames();
            aaa.KeyFrames.Add(new System.Windows.Media.Animation.DiscreteObjectKeyFrame(Visibility.Visible, System.Windows.Media.Animation.KeyTime.FromTimeSpan(TimeSpan.Zero)));
            detailPanelContainer.BeginAnimation(Control.VisibilityProperty, aaa, System.Windows.Media.Animation.HandoffBehavior.SnapshotAndReplace);
            detailPanelContainer.BeginAnimation(Control.OpacityProperty, new System.Windows.Media.Animation.DoubleAnimation(0.0, 1.0, (Duration)bbb), System.Windows.Media.Animation.HandoffBehavior.SnapshotAndReplace);
        }
        public void ShowPopupInfo(PlaneNode node)
        {
            node.Open();
            _openingNode = node;
            detailScatter.Visibility = System.Windows.Visibility.Collapsed;
            pictureScatter.Visibility = System.Windows.Visibility.Visible;
            pictureScatter.Center = new Point(900, 500);
            pictureScatter.Orientation = 0.0;
            detailImage.Source = new BitmapImage(node.Picture);

            var bbb = TimeSpan.FromMilliseconds(150);
            var aaa = new System.Windows.Media.Animation.ObjectAnimationUsingKeyFrames();
            aaa.KeyFrames.Add(new System.Windows.Media.Animation.DiscreteObjectKeyFrame(Visibility.Visible, System.Windows.Media.Animation.KeyTime.FromTimeSpan(TimeSpan.Zero)));
            detailPanelContainer.BeginAnimation(Control.VisibilityProperty, aaa, System.Windows.Media.Animation.HandoffBehavior.SnapshotAndReplace);
            detailPanelContainer.BeginAnimation(Control.OpacityProperty, new System.Windows.Media.Animation.DoubleAnimation(0.0, 1.0, (Duration)bbb), System.Windows.Media.Animation.HandoffBehavior.SnapshotAndReplace);
        }
        public void ClosePopupInfo()
        {
            if (_openingNode != null)
            {
                _openingNode.Close();
                if (_openingNode is ComplexCloudNode)
                    ((ComplexCloudNode)_openingNode).Resume();
            }
            var bbb = TimeSpan.FromMilliseconds(50);
            var aaa = new System.Windows.Media.Animation.ObjectAnimationUsingKeyFrames();
            aaa.KeyFrames.Add(new System.Windows.Media.Animation.DiscreteObjectKeyFrame(Visibility.Visible, System.Windows.Media.Animation.KeyTime.FromTimeSpan(TimeSpan.Zero)));
            aaa.KeyFrames.Add(new System.Windows.Media.Animation.DiscreteObjectKeyFrame(Visibility.Hidden, System.Windows.Media.Animation.KeyTime.FromTimeSpan(bbb)));
            detailPanelContainer.BeginAnimation(Control.VisibilityProperty, aaa, System.Windows.Media.Animation.HandoffBehavior.SnapshotAndReplace);
            detailPanelContainer.BeginAnimation(Control.OpacityProperty, new System.Windows.Media.Animation.DoubleAnimation(0.0, (Duration)bbb), System.Windows.Media.Animation.HandoffBehavior.SnapshotAndReplace);
        }
        CloudStructure createStoryBoardCLD()
        {
            var xtime = _rnd.Next(20000, 50000);
            var ytime = _rnd.Next(20000, 50000);
            var typeID = _rnd.Next(30);
            
            cloud = new CloudStructure();
            count = storyboards.Count;
            var x = ActualWidth;
            var y = ActualHeight;
            cloud.CloudTypeId = typeID;
            //cloud.CloudStatus = (CloudStateType)_rnd.Next(2);
            //cloud.MouseDown += detailDisplayCld;
            cloud.Tag = count;

            hw[2] = (int)x;
            hw[3] = (int)y;
            var i = _rnd.Next(2);
            hw[i] = _rnd.Next(hw[(i + 2)]);

            storyboard = new Storyboard();
            var animation = new DoubleAnimationUsingKeyFrames
            {
                RepeatBehavior = RepeatBehavior.Forever,
                AutoReverse = true
            };
            Storyboard.SetTarget(animation, cloud);
            var frame = new EasingDoubleKeyFrame(x, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(xtime)));
            if (_rnd.Next(2) > 0)
            {
                Canvas.SetRight(cloud, hw[0]);
                Storyboard.SetTargetProperty(animation, new PropertyPath("(Canvas.Right)"));
            }
            else
            {
                Canvas.SetLeft(cloud, hw[0]);
                Storyboard.SetTargetProperty(animation, new PropertyPath("(Canvas.Left)"));
            }
            animation.KeyFrames.Add(frame);
            storyboard.Children.Add(animation);

            animation = new DoubleAnimationUsingKeyFrames
            {
                RepeatBehavior = RepeatBehavior.Forever,
                AutoReverse = true
            };
            Storyboard.SetTarget(animation, cloud);
            frame = new EasingDoubleKeyFrame(y, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(ytime)));
            if (_rnd.Next(2) > 0)
            {
                Canvas.SetTop(cloud, hw[1]);
                
                Storyboard.SetTargetProperty(animation, new PropertyPath("(Canvas.Top)"));
            }
            else
            {
                Canvas.SetBottom(cloud, hw[1]);
                Storyboard.SetTarget(animation, cloud);
                Storyboard.SetTargetProperty(animation, new PropertyPath("(Canvas.Bottom)"));
            }

            animation.KeyFrames.Add(frame);
            storyboard.Children.Add(animation);

            storyboards.Add(storyboard);
            storyboards[count].Begin(this, true);
            return cloud;
        }
        PlaneControl createStoryBoardPlane()
        {
            var xtime = _rnd.Next(4000, 6000);
            var ytime = _rnd.Next(4000, 6000);

            plane = new PlaneControl();
            count = storyboards.Count;
            var x = ActualWidth;
            var y = ActualHeight;
            //plane.MouseDown += detailDisplayPlane;
            plane.Tag = count;

            hw[1] = _rnd.Next((int)y);

            storyboard = new Storyboard();
            var animation = new DoubleAnimationUsingKeyFrames
            {
                RepeatBehavior = new RepeatBehavior((double)1.0),
                AutoReverse = false
            };
            var frame = new EasingDoubleKeyFrame(x, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(xtime)));
            if (_rnd.Next(2) > 0)
            {
                plane.PlaneStatus = PlaneStateType.Left;
                Canvas.SetRight(plane, hw[0]);
                Storyboard.SetTargetProperty(animation, new PropertyPath("(Canvas.Right)"));
            }
            else
            {
                plane.PlaneStatus = PlaneStateType.Right;
                Canvas.SetLeft(plane, hw[0]);
                Storyboard.SetTargetProperty(animation, new PropertyPath("(Canvas.Left)"));
            }
            animation.KeyFrames.Add(frame);
            storyboard.Children.Add(animation);

            animation = new DoubleAnimationUsingKeyFrames
            {
                RepeatBehavior = new RepeatBehavior((double)1.0),
                AutoReverse = false
            };
            frame = new EasingDoubleKeyFrame(y, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(ytime)));
            if (_rnd.Next(2) > 0)
            {
                Canvas.SetTop(plane, hw[1]);
                Storyboard.SetTargetProperty(animation, new PropertyPath("(Canvas.Top)"));
            }
            else
            {
                Canvas.SetBottom(plane, hw[1]);
                Storyboard.SetTargetProperty(animation, new PropertyPath("(Canvas.Bottom)"));
            }

            animation.KeyFrames.Add(frame);
            storyboard.Children.Add(animation);

            storyboards.Add(storyboard);
            storyboards[count].Begin(plane, true);
            return plane;
        }

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

        //雲追加関係
        void _fieldManager_HomeNodesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Dispatcher.Invoke((Action)(() =>
            {
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        foreach (var item in e.NewItems.OfType<ComplexCloudNode>())
                        {
                            var cld = createStoryBoardCLD();
                            cld.Height = 200;
                            cld.Width = 300;
                            //item.ChildrenChanged += CloudComplex_Home_ChildrenChanged;
                            item.Element = cld;
                            cld.MouseDown += Home_Click;
                            var items = new Tuple<int, ComplexCloudNode>(storyboards.Count-1, item);
                            cld.Tag = items;
                            homeGrid.Children.Add(cld);
                            
                        }
                        foreach (var item in e.NewItems.OfType<PlaneNode>())
                        {
                            var pln = createStoryBoardPlane();
                            pln.Height = 150;
                            pln.Width = 200;
                            item.Element = pln;
                            pln.MouseDown += Home_Click;
                            var items = new Tuple<int, PlaneNode>(storyboards.Count - 1, item);
                            pln.Tag = items;
                            homeGrid.Children.Add(pln);
                        }
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        foreach (var item in e.OldItems.OfType<ComplexCloudNode>())
                        {
                            var dramItem = item.Element;
                            homeGrid.Children.Remove(dramItem);
                            item.ChildrenChanged -= CloudComplex_Home_ChildrenChanged;
                        }
                        foreach (var item in e.OldItems.OfType<PlaneNode>())
                        {
                            var dramItem = item.Element;
                            homeGrid.Children.Remove(dramItem);
                        }
                        break;
                }
            }));
        }
        void _fieldManager_TimeshiftNodesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Dispatcher.Invoke((Action)(() =>
            {
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        foreach (var item in e.NewItems.OfType<ComplexCloudNode>())
                        {
                            item.ChildrenChanged += CloudComplex_Timeshift_ChildrenChanged;
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
                            item.Element = dramItem;
                            timeshiftDram.Items.Add(dramItem);
                        }
                        foreach (var item in e.NewItems.OfType<PlaneNode>())
                        {
                            var dramItem = new Controls.DramItem()
                            {
                                Content = new SurfaceButton()
                                {
                                    Content = new Controls.PlaneControl()
                                    {
                                        Height = item.IsReaded ? 130 : 50,
                                        PlaneStatus = Controls.PlaneStateType.Right,
                                        Effect = (System.Windows.Media.Effects.Effect)FindResource("dropShadowEffectB"),
                                    },
                                    Background = null,
                                    Style = (Style)FindResource("surfaceTemplate"),
                                },
                                Angle = (item.RaiseTime - _startAppTime).TotalMinutes * MinutesToAngleRate + LATEST_OFFSET_LINE,
                                Track = _rnd.Next(20) * 30,
                            };
                            item.IsReadedChanged += item_IsReadedChanged;
                            ((SurfaceButton)dramItem.Content).Click += DramItem_Click;
                            ((SurfaceButton)dramItem.Content).Tag = dramItem;
                            dramItem.Tag = item;
                            dramItem.BeginAnimation(
                                Control.OpacityProperty, new System.Windows.Media.Animation.DoubleAnimation(0.0, 1.0,
                                (Duration)TimeSpan.FromMilliseconds(500)));
                            item.Element = dramItem;
                            timeshiftDram.Items.Add(dramItem);
                        }
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        foreach (var item in e.OldItems.OfType<ComplexCloudNode>())
                            item.ChildrenChanged -= CloudComplex_Timeshift_ChildrenChanged;
                        foreach (var item in e.OldItems.OfType<BaseNode>())
                        {
                            var dramItem = item.Element;
                            timeshiftDram.Items.Remove(dramItem);
                        }
                        break;
                }
            }));
        }
        void CloudComplex_Timeshift_ChildrenChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Dispatcher.Invoke((Action)(() =>
            {
                var complexCloud = (ComplexCloudNode)sender;
                var dramItem = (Controls.DramItem)complexCloud.Element;
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
                            item.Element = cloudStructure;
                            grid.Children.Add(cloudStructure);
                            cloudStructure.BeginAnimation(
                                Control.OpacityProperty,
                                new System.Windows.Media.Animation.DoubleAnimation(0.0, 1.0, new Duration(TimeSpan.FromMilliseconds(1000))));
                        }
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        foreach (var item in e.OldItems.Cast<CloudNode>())
                            grid.Children.Remove(item.Element);
                        break;
                }
            }));
        }
        void CloudComplex_Home_ChildrenChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Dispatcher.Invoke((Action)(() =>
            {
                var complexCloud = (ComplexCloudNode)sender;
                var dramItem = (Controls.DramItem)complexCloud.Element;
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
                            item.Element = cloudStructure;
                            grid.Children.Add(cloudStructure);
                            cloudStructure.BeginAnimation(
                                Control.OpacityProperty,
                                new System.Windows.Media.Animation.DoubleAnimation(0.0, 1.0, new Duration(TimeSpan.FromMilliseconds(1000))));
                        }
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        foreach (var item in e.OldItems.Cast<CloudNode>())
                            grid.Children.Remove(item.Element);
                        break;
                }
            }));
        }
        void item_IsReadedChanged(object sender, ExEventArgs<bool> e)
        {
            var item = (PlaneNode)sender;
            var container = (ContentControl)((Controls.DramItem)item.Element).Content;
            item.IsReadedChanged -= item_IsReadedChanged;
            container.Content = new Image()
            {
                Source = new BitmapImage(item.Picture),
                Effect = (System.Windows.Media.Effects.Effect)FindResource("dropShadowEffectB"),
            };
            container.Height = item.IsReaded ? 130 : 50;
        }
        //詳細表示関係
        void DramItem_Click(object sender, RoutedEventArgs e)
        {
            var dramItem = (Controls.DramItem)((SurfaceButton)sender).Tag;
            if (dramItem.Tag is ComplexCloudNode)
                ShowPopupInfo((ComplexCloudNode)dramItem.Tag);
            else if (dramItem.Tag is PlaneNode)
                ShowPopupInfo((PlaneNode)dramItem.Tag);
            _timelineStoryboard.Pause(this);
        }
        void Home_Click(object sender, RoutedEventArgs e)
        {
            if (sender is CloudStructure)
            {
                var cld = (CloudStructure)sender;
                var items = (Tuple<int,ComplexCloudNode>)cld.Tag;
                var cldNd = (ComplexCloudNode)items.Item2;
                cldNd.Pause();
                ShowPopupInfo(cldNd);
                storyboards[(int)items.Item1].Pause(this);
            }
            else if (sender is PlaneControl)
            {
                var pln = (PlaneControl)sender;
                var items = (Tuple<int, PlaneNode>)pln.Tag;
                var plnNd = (PlaneNode)items.Item2;
                ShowPopupInfo(plnNd);
                storyboards[(int)items.Item1].Pause(this);
            }
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
                    ClosePopupInfo();
                    _timelineStoryboard.Resume(this);
                    storyboards[0].Resume(this);
                }
            }
        }
        //コントローラー関係
        void btn_ChangeMode_Click(object sender, RoutedEventArgs e)
        {
            _fieldManager.Mode = (ActiveModeType)(((int)_fieldManager.Mode + 1) % 2);
        }
        void btn_PostPlane_Click(object sender, RoutedEventArgs e)
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