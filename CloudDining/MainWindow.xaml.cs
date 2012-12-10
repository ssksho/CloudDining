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
        Storyboard createStoryBoardCLD(FrameworkElement animationTarget, Action<Point,Point,TimeSpan,Storyboard> extendFunc)
        {
            var storyboard = new Storyboard();
            var spanSum = TimeSpan.Zero;
            //端から端まで移動するサイクルを5回繰り返すアニメーションを
            //共通的に適用する
            for (var i = 0; i < 5; i++)
            {
                var moveSpan = TimeSpan.FromMilliseconds(_rnd.Next(20000, 50000));
                double xA, yA, xB, yB;
                //画面の上、右のサイズ合計を上限に乱数を取得。これを使用して上か右かに
                //張り付いた座標を生成する。生成したらその位置と対角になる座標を生成。
                //対角座標算出のブレ幅として80上限の乱数を加算する
                var pointSeed = _rnd.Next((int)backGrid.ActualWidth + (int)backGrid.ActualHeight);
                var pointASeed = pointSeed;
                var pointBSeed = pointSeed - _rnd.Next(100);
                xA = Math.Min(pointASeed, backGrid.ActualWidth);
                yA = Math.Max(pointASeed - backGrid.ActualWidth, 0);
                //座標が0の場合は左か上に張り付いている事になる。250引く事で見えない部分を
                //指定させ、コントロールが完全に画面の枠外に出るようにする
                xA -= xA == 0 ? 250 : 0;
                yA -= yA == 0 ? 250 : 0;
                xB = Math.Max(backGrid.ActualWidth - pointBSeed, 0);
                yB = Math.Max(backGrid.ActualHeight - Math.Max(pointBSeed - backGrid.ActualWidth, 0), 0);
                xB -= xB == 0 ? 250 : 0;
                yB -= yB == 0 ? 250 : 0;
                Point startPoint, endPoint;
                if (_rnd.Next(2) == 0)
                {
                    startPoint = new Point(xA, yA);
                    endPoint = new Point(xB, yB);
                }
                else
                {
                    startPoint = new Point(xB, yB);
                    endPoint = new Point(xA, yA);
                }

                var animation = new DoubleAnimationUsingKeyFrames();
                Storyboard.SetTarget(animation, animationTarget);
                Storyboard.SetTargetProperty(animation, new PropertyPath("(Canvas.Left)"));
                animation.BeginTime = spanSum;
                animation.KeyFrames.Add(new EasingDoubleKeyFrame(startPoint.X, KeyTime.FromTimeSpan(TimeSpan.Zero)));
                animation.KeyFrames.Add(new EasingDoubleKeyFrame(endPoint.X, KeyTime.FromTimeSpan(moveSpan)));
                storyboard.Children.Add(animation);

                animation = new DoubleAnimationUsingKeyFrames();
                Storyboard.SetTarget(animation, animationTarget);
                Storyboard.SetTargetProperty(animation, new PropertyPath("(Canvas.Top)"));
                animation.BeginTime = spanSum;
                animation.KeyFrames.Add(new EasingDoubleKeyFrame(startPoint.Y, KeyTime.FromTimeSpan(TimeSpan.Zero)));
                animation.KeyFrames.Add(new EasingDoubleKeyFrame(endPoint.Y, KeyTime.FromTimeSpan(moveSpan)));
                storyboard.Children.Add(animation);

                if (extendFunc != null)
                    extendFunc(startPoint, endPoint, spanSum, storyboard);
                spanSum += moveSpan;
            }
            storyboard.RepeatBehavior = RepeatBehavior.Forever;
            storyboard.Begin(this, true);
            return storyboard;
        }
        Storyboard createStoryBoardPLN(FrameworkElement animationTarget, PlaneControl plane)
        {
            return createStoryBoardCLD(animationTarget, (beginPoint, endPoint, beginTime, storyboard) =>
                {
                    var animation = new ObjectAnimationUsingKeyFrames();
                    Storyboard.SetTarget(animation, plane);
                    Storyboard.SetTargetProperty(animation, new PropertyPath(PlaneControl.PlaneStatusProperty));
                    animation.BeginTime = beginTime;
                    animation.KeyFrames.Add(new DiscreteObjectKeyFrame(
                        endPoint.X - beginPoint.X >= 0 ? PlaneStateType.Right : PlaneStateType.Left,
                        KeyTime.FromTimeSpan(TimeSpan.Zero)));
                    storyboard.Children.Add(animation);
                });
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
                            item.ChildrenChanged += CloudComplex_Home_ChildrenChanged;
                            var grid = new Grid();
                            var container = new SurfaceButton() { Content = grid, Background = null, Style = (Style)FindResource("surfaceTemplate"), };
                            container.Click += container_Click;                            
                            item.Element = container;
                            homeGrid.Children.Add(container);
                            container.Tag = new object[] { item, createStoryBoardCLD(container, null) };
                        }
                        foreach (var item in e.NewItems.OfType<PlaneNode>())
                        {
                            var planeControl = new PlaneControl();
                            var container = new SurfaceButton() { Content = planeControl, Background = null, Style = (Style)FindResource("surfaceTemplate"), };
                            planeControl.Height = 150;
                            planeControl.Width = 200;
                            container.Tag = new object[] { item, createStoryBoardPLN(container, planeControl) };
                            container.Click += container_Click;
                            item.Element = container;
                            homeGrid.Children.Add(container);
                        }
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        foreach (var item in e.OldItems.OfType<BaseNode>())
                        {
                            var container = (SurfaceButton)item.Element;
                            var storyboard = (Storyboard)((object[])container.Tag)[1];
                            if (item is ComplexCloudNode)
                                ((ComplexCloudNode)item).ChildrenChanged -= CloudComplex_Home_ChildrenChanged;

                            var animeSpan = TimeSpan.FromMilliseconds(1000);
                            var feedOutAnime = new DoubleAnimation(0.0, (Duration)animeSpan);
                            Storyboard.SetTarget(feedOutAnime, container);
                            Storyboard.SetTargetProperty(feedOutAnime, new PropertyPath(Control.OpacityProperty));
                            var visibilityAnime = new ObjectAnimationUsingKeyFrames();
                            visibilityAnime.KeyFrames.Add(new DiscreteObjectKeyFrame(Visibility.Visible, (KeyTime)TimeSpan.Zero));
                            visibilityAnime.KeyFrames.Add(new DiscreteObjectKeyFrame(Visibility.Collapsed, (KeyTime)animeSpan));
                            Storyboard.SetTarget(visibilityAnime, container);
                            Storyboard.SetTargetProperty(visibilityAnime, new PropertyPath(Control.VisibilityProperty));
                            var feedOutStoryboard = new Storyboard();
                            feedOutStoryboard.Children.Add(feedOutAnime);
                            feedOutStoryboard.Children.Add(visibilityAnime);
                            feedOutStoryboard.Begin(this);
                            FieldManager.Delay(ctrl =>
                                {
                                    Dispatcher.Invoke((Action<Control>)(aaa => homeGrid.Children.Remove(aaa)), ctrl);
                                    storyboard.Stop(this);
                                }, container, (long)animeSpan.TotalSeconds + 1);
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
                var container = (SurfaceButton)complexCloud.Element;
                var grid = (Grid)container.Content;

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
                    //case NotifyCollectionChangedAction.Remove:
                    //    foreach (var item in e.OldItems.Cast<CloudNode>())
                    //        grid.Children.Remove(item.Element);
                    //    break;
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
        void container_Click(object sender, RoutedEventArgs e)
        {
            var container = (SurfaceButton)sender;
            var node = (BaseNode)((object[])container.Tag)[0];
            var storyboard = (Storyboard)((object[])container.Tag)[1];
            if (node is ComplexCloudNode)
                ShowPopupInfo((ComplexCloudNode)node);
            else if (node is PlaneNode)
                ShowPopupInfo((PlaneNode)node);
            storyboard.Pause(this);
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
                    if (((FrameworkElement)_openingNode.Element).Tag is object[])
                    {
                        var board = (Storyboard)((object[])((FrameworkElement)_openingNode.Element).Tag)[1];
                        board.Resume(this);
                    }
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
                    _fieldManager.Users.First().PostPlane(tsk.Result, null));
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