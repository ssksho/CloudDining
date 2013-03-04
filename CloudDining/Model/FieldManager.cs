using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace CloudDining.Model
{
    public class FieldManager : ViewModel.ViewModelBase
    {
        public FieldManager(System.Windows.Threading.Dispatcher uiThreadDispatcher)
            : base(uiThreadDispatcher)
        {
            _rnd = new Random();
            _lifeTimer = new Dictionary<object, System.Threading.Timer>();
            _userCheckinTime = new Dictionary<Account, DateTime>();
            _users = new ObservableCollection<Account>();
            _timelineNodes = new ObservableCollection<BaseNode>();
            _homeNodes = new ObservableCollection<BaseNode>();
            _checkinSpan = TimeSpan.FromSeconds(5);
            _cloudLifeSpan = TimeSpan.FromSeconds(30);

            Users = new ReadOnlyObservableCollection<Account>(_users);
            HomeNodes = new ReadOnlyObservableCollection<BaseNode>(_homeNodes);
            TimelineNodes = new ReadOnlyObservableCollection<BaseNode>(_timelineNodes);
            Mode = ActiveModeType.Home;

            AddUser(new Account("父", new Uri("pack://application:,,,/Resources/Profiles/user00.jpg", UriKind.Absolute), this));
            AddUser(new Account("母", new Uri("pack://application:,,,/Resources/Profiles/user01.jpg", UriKind.Absolute), this));
            AddUser(new Account("息子", new Uri("pack://application:,,,/Resources/Profiles/user02.jpg", UriKind.Absolute), this));
            AddUser(new Account("娘", new Uri("pack://application:,,,/Resources/Profiles/user03.jpg", UriKind.Absolute), this));

            HomeNodesChanged += FieldManager_TimelineNodesChanged;
            TimelineNodesChanged += FieldManager_TimelineNodesChanged;
        }
        Random _rnd;
        ActiveModeType _mode;
        BaseNode _openingNode;
        TimeSpan _checkinSpan, _cloudLifeSpan;
        Dictionary<Account, DateTime> _userCheckinTime;
        ObservableCollection<Account> _users;
        ObservableCollection<BaseNode> _timelineNodes;
        ObservableCollection<BaseNode> _homeNodes;
        static Dictionary<object, System.Threading.Timer> _lifeTimer; 

        public ReadOnlyObservableCollection<Account> Users { get; set; }
        public ReadOnlyObservableCollection<BaseNode> TimelineNodes { get; set; }
        public ReadOnlyObservableCollection<BaseNode> HomeNodes { get; set; }
        public ActiveModeType Mode
        {
            get{return _mode;}
            set
            {
                _mode = value;
                OnModeChanged(new ExEventArgs<ActiveModeType>(value));
                OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("Mode"));
            }
        }

        internal void PostPlane(PlaneNode home, PlaneNode time)
        {
            lock (_homeNodes)
            {
                _homeNodes.Add(home);
                _timelineNodes.Add(time);
            }
        }
        public void AddUser(Account target)
        {
            _users.Add(target);
            _userCheckinTime.Add(target, DateTime.MinValue);
        }
        public void RemoveUser(Account target)
        {
            _users.Remove(target);
            _userCheckinTime.Remove(target);
        }
        public bool CheckStatusOf(Account target)
        {
            DateTime currentStatus;
            if (_userCheckinTime.TryGetValue(target, out currentStatus) == false)
                throw new ArgumentException(
                    "引数targetで指定されたインスタンスはFieldManager.Usersに登録されていません。");
            return currentStatus != DateTime.MinValue;
        }
        public void CheckinUser(Account target, TimeSpan? checkinSpan = null, TimeSpan? cloudLifeSpan = null)
        {
            if (CheckStatusOf(target) == false)
            {
                _userCheckinTime[target] = DateTime.Now;
                OnCheckined(new ExEventArgs<Account>(target));
            }

            Delay<Account>(
                state => CheckoutUser((Account)state, cloudLifeSpan ?? _cloudLifeSpan),
                target, (long)(checkinSpan ?? _checkinSpan).TotalSeconds);
        }
        public bool CheckoutUser(Account target, TimeSpan lifeSpan)
        {
            if (_userCheckinTime.ContainsKey(target) == false)
                throw new ArgumentException(
                    "引数targetで指定されたインスタンスはFieldManager.Usersに登録されていません。");
            if (_userCheckinTime[target] == DateTime.MinValue)
                return false;

            var complexNodeTimeshift = new ComplexCloudNode(true);
            var complexNodeHome = new ComplexCloudNode(false);
            var checkinTime = _userCheckinTime[target];
            var checkinSpan = DateTime.Now - checkinTime;
            var cloudNode = new CloudNode(target, target.Weather, _rnd.Next(30), checkinTime, checkinSpan, lifeSpan);
            lock (_homeNodes)
            {
                foreach (var item in TimelineNodes.Reverse().OfType<ComplexCloudNode>())
                    if (item.RaiseTime > checkinTime)
                        item.Add(cloudNode);
                bool aaa = true;
                foreach (var item in HomeNodes.Reverse().OfType<ComplexCloudNode>())
                    if (item.RaiseTime > checkinTime)
                    {
                        aaa = false;
                        item.Add(cloudNode);
                    }
                if (aaa)
                {
                    _homeNodes.Add(complexNodeHome);
                    _timelineNodes.Add(complexNodeTimeshift);
                }
            }
            complexNodeTimeshift.ChildrenChanged += complexNode_ChildrenChanged;
            complexNodeHome.ChildrenChanged += complexNode_ChildrenChanged;
            OnCheckouted(new ExEventArgs<Account>(target));

            complexNodeTimeshift.Add(cloudNode);
            complexNodeHome.Add(cloudNode, lifeSpan);
            _userCheckinTime[target] = DateTime.MinValue;

            return true;
        }

        public static void Delay<T>(Action<T> handler, T param, long delaySecounds)
        {
            System.Threading.Timer timer;
            if (_lifeTimer.TryGetValue(param, out timer))
            {
                timer.Dispose();
                _lifeTimer.Remove(param);
            }

            object[] state = new object[3];
            timer = new System.Threading.Timer(lifeTimer_Fired<T>, state, System.Threading.Timeout.Infinite, 0);

            _lifeTimer.Add(param, timer);
            state[0] = timer;
            state[1] = param;
            state[2] = handler;
            timer.Change(delaySecounds * 1000, System.Threading.Timeout.Infinite);
        }
        static void lifeTimer_Fired<T>(object state)
        {
            var timer = (System.Threading.Timer)((object[])state)[0];
            var param = (T)((object[])state)[1];
            var lamda = (Action<T>)((object[])state)[2];
            lamda(param);
            timer.Dispose();
            _lifeTimer.Remove(_lifeTimer.First(pair => pair.Value == timer).Key);
        }
        void FieldManager_TimelineNodesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (BaseNode item in e.NewItems)
                    {
                        item.IsOpenedChanged += baseNode_IsOpenedChanged;
                        if (item is PlaneNode)
                            ((PlaneNode)item).IsReadedChanged += planeNode_IsReadedChanged;
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (BaseNode item in e.OldItems)
                    {
                        item.IsOpenedChanged -= baseNode_IsOpenedChanged;
                        if (item is PlaneNode)
                            ((PlaneNode)item).IsReadedChanged -= planeNode_IsReadedChanged;
                    }
                    break;
            }
        }
        void complexNode_ChildrenChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var complexNode = (Model.ComplexCloudNode)sender;
            if (e.Action == NotifyCollectionChangedAction.Remove
                || e.Action == NotifyCollectionChangedAction.Reset)
                if (complexNode.Children.Count == 0)
                    lock (_homeNodes)
                    {
                        complexNode.ChildrenChanged -= complexNode_ChildrenChanged;
                        _timelineNodes.Remove(complexNode);
                        _homeNodes.Remove(complexNode);
                    }
        }
        void baseNode_IsOpenedChanged(object sender, ExEventArgs<bool> e)
        {
            if (e.Value)
            {
                if (_openingNode != null && sender != _openingNode)
                    _openingNode.Close();
                _openingNode = (BaseNode)sender;
            }
            else
                _openingNode = null;
        }
        void planeNode_IsReadedChanged(object sender, ExEventArgs<bool> e)
        {
            if (e.Value == true)
                lock (_homeNodes)
                    _homeNodes.Remove((PlaneNode)sender);
        }

        public event EventHandler<ExEventArgs<Account>> Checkined;
        protected virtual void OnCheckined(ExEventArgs<Account> e)
        {
            if (Checkined != null)
                Checkined(this, e);
        }
        public event EventHandler<ExEventArgs<Account>> Checkouted;
        protected virtual void OnCheckouted(ExEventArgs<Account> e)
        {
            if (Checkouted != null)
                Checkouted(this, e);
        }
        public event EventHandler<ExEventArgs<ActiveModeType>> ModeChanged;
        protected virtual void OnModeChanged(ExEventArgs<ActiveModeType> e)
        {
            if (ModeChanged != null)
                ModeChanged(this, e);
        }
        public event NotifyCollectionChangedEventHandler HomeNodesChanged
        {
            add { _homeNodes.CollectionChanged += value; }
            remove { _homeNodes.CollectionChanged -= value; }
        }
        public event NotifyCollectionChangedEventHandler TimelineNodesChanged
        {
            add { _timelineNodes.CollectionChanged += value; }
            remove { _timelineNodes.CollectionChanged -= value; }
        }
    }
    public enum NodeType
    { All, Cloud, Plane }
    public enum ActiveModeType
    { Home, Timeline, Setting }

    public class ExEventArgs<T> : EventArgs
    {
        public ExEventArgs(T value) { Value = value; }
        public T Value { get; private set; }
    }
}
