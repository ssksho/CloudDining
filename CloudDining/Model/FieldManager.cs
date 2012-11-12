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
            _lifeTimer = new Dictionary<object, System.Threading.Timer>();
            _userStatus = new Dictionary<Account, bool>();
            _users = new ObservableCollection<Account>();
            _timelineNodes = new ObservableCollection<BaseNode>();
            _homeNodes = new ObservableCollection<BaseNode>();

            Users = new ReadOnlyObservableCollection<Account>(_users);
            HomeNodes = new ReadOnlyObservableCollection<BaseNode>(_homeNodes);
            TimelineNodes = new ReadOnlyObservableCollection<BaseNode>(_timelineNodes);
            Mode = ActiveModeType.Home;

            AddUser(new Account("父", new Uri("pack://application:,,,/Resources/Profiles/user00.jpg", UriKind.Absolute), this));
            AddUser(new Account("母", new Uri("pack://application:,,,/Resources/Profiles/user01.jpg", UriKind.Absolute), this));
            AddUser(new Account("息子", new Uri("pack://application:,,,/Resources/Profiles/user02.jpg", UriKind.Absolute), this));
            AddUser(new Account("娘", new Uri("pack://application:,,,/Resources/Profiles/user03.jpg", UriKind.Absolute), this));

            HomeNodesChanged += FieldManager_HomeNodesChanged;
        }
        ActiveModeType _mode;
        Dictionary<Account, bool> _userStatus;
        Dictionary<object, System.Threading.Timer> _lifeTimer;
        ObservableCollection<Account> _users;
        ObservableCollection<BaseNode> _timelineNodes;
        ObservableCollection<BaseNode> _homeNodes;

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

        public void PostPlane(PlaneNode data)
        {
            _homeNodes.Add(data);
            _timelineNodes.Add(data);
        }
        public void AddUser(Account target)
        {
            _users.Add(target);
            _userStatus.Add(target, false);
        }
        public void RemoveUser(Account target)
        {
            _users.Remove(target);
            _userStatus.Remove(target);
        }
        public bool CheckStatusOf(Account target)
        { return _userStatus[target]; }
        public void CheckinUser(Account target, TimeSpan? checkinSpan = null, TimeSpan? cloudLifeSpan = null)
        {
            bool currentStatus;
            if (_userStatus.TryGetValue(target, out currentStatus) == false)
                throw new ArgumentException(
                    "引数targetで指定されたインスタンスはFieldManager.Usersに登録されていません。");
            if (currentStatus == false)
            {
                _userStatus[target] = true;
                System.Diagnostics.Debug.WriteLine("DebugWriteLine: Checkin", target.Name);
                OnCheckined(new ExEventArgs<Account>(target));
            }

            Delay<Account>(state =>
                CheckoutUser((Account)state, cloudLifeSpan),
                target, checkinSpan.HasValue ? (long)checkinSpan.Value.TotalSeconds : 60 * 30);
        }
        public bool CheckoutUser(Account target, TimeSpan? lifeSpan = null)
        {
            if (_userStatus.ContainsKey(target) == false)
                throw new ArgumentException(
                    "引数targetで指定されたインスタンスはFieldManager.Usersに登録されていません。");
            if (_userStatus[target] == false)
                return false;

            var complexNode = new ComplexCloudNode();
            foreach (var item in _userStatus.Where(pair => pair.Value))
                complexNode.Children.Add(new CloudNode(item.Key));
            _homeNodes.Add(complexNode);
            _timelineNodes.Add(complexNode);
            _userStatus[target] = false;

            System.Diagnostics.Debug.WriteLine("DebugWriteLine: Checkout", target.Name);
            OnCheckouted(new ExEventArgs<Account>(target));

            //3時間で雲は消える
            Delay<ComplexCloudNode>(
                node => _homeNodes.Remove(node), complexNode,
                lifeSpan.HasValue ? (long)lifeSpan.Value.TotalSeconds : 60 * 60 * 3);

            return true;
        }

        void Delay<T>(Action<T> handler, T param, long delaySecounds)
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
        void lifeTimer_Fired<T>(object state)
        {
            var timer = (System.Threading.Timer)((object[])state)[0];
            var param = (T)((object[])state)[1];
            var lamda = (Action<T>)((object[])state)[2];
            lamda(param);
            timer.Dispose();
            _lifeTimer.Remove(_lifeTimer.First(pair => pair.Value == timer).Key);
        }
        void FieldManager_HomeNodesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach(var item in e.NewItems)
                        if(item is PlaneNode)
                            ((PlaneNode)item).IsReadedChanged += PlaneNode_IsReadedChanged;
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach(var item in e.OldItems)
                        if (item is PlaneNode)
                            ((PlaneNode)item).IsReadedChanged += PlaneNode_IsReadedChanged;
                    break;
            }
        }
        void PlaneNode_IsReadedChanged(object sender, ExEventArgs<bool> e)
        {
            if (e.Value == true)
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
    { Home, Timeline }

    public class ExEventArgs<T> : EventArgs
    {
        public ExEventArgs(T value) { Value = value; }
        public T Value { get; private set; }
    }
}
