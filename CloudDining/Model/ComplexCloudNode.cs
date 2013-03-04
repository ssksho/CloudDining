using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace CloudDining.Model
{
    public class ComplexCloudNode : BaseNode
    {
        public ComplexCloudNode(bool isTimeshift, DateTime? raiseTime = null)
            : base(raiseTime)
        {
            _syncer_countFilledDisplaySpanNode = new object();
            _isTimeshift = isTimeshift;
            _children = new ObservableCollection<CloudNode>();
            _children.CollectionChanged += _children_CollectionChanged;
            Children = new ReadOnlyObservableCollection<CloudNode>(_children);
        }
        bool _isTimeshift;
        object _syncer_countFilledDisplaySpanNode;
        int _timerGen, countFilledDisplaySpanNode;
        DateTime _pauseTime;
        ObservableCollection<CloudNode> _children;

        public bool HasInvisibleCloud { get; private set; }
        public System.Collections.ObjectModel.ReadOnlyObservableCollection<CloudNode> Children { get; private set; }
        public void Add(CloudNode node, TimeSpan? lifeTime = null)
        {
            _children.Add(node);
            if (_isTimeshift == false)
                FieldManager.Delay(tuple =>
                {
                    if (tuple.Item1 != _timerGen)
                        return;
                    lock (_syncer_countFilledDisplaySpanNode)
                        countFilledDisplaySpanNode++;
                    if (countFilledDisplaySpanNode == Children.Count)
                        _children.Clear();
                }, new Tuple<int, CloudNode>(_timerGen, node), (long)(lifeTime ?? TimeSpan.FromSeconds(3)).TotalSeconds);
        }
        public void Pause()
        {
            if (_isTimeshift)
                return;
            _timerGen++;
            _pauseTime = DateTime.Now;
        }
        public void Resume()
        {
            if (_isTimeshift || _pauseTime == DateTime.MinValue)
                return;
            foreach(var item in Children)
            {
                var utc = DateTime.Now;
                var pauseSpan = utc - _pauseTime;
                var aaa = item.CheckinTime + item.CheckinSpan + item.DisplaySpan + pauseSpan;
                var newSpan = aaa - utc;
                newSpan = newSpan > TimeSpan.Zero ? newSpan : TimeSpan.Zero;
                FieldManager.Delay(tuple =>
                        {
                            if (tuple.Item1 != _timerGen)
                                return;
                            lock (_syncer_countFilledDisplaySpanNode)
                                countFilledDisplaySpanNode++;
                            if (countFilledDisplaySpanNode == Children.Count)
                                _children.Clear();
                        }, new Tuple<int, CloudNode>(_timerGen, item), (long)newSpan.TotalSeconds);
            }
            _pauseTime = DateTime.MinValue;
        }

        public event NotifyCollectionChangedEventHandler ChildrenChanged;
        protected virtual void OnChildrenChanged(NotifyCollectionChangedEventArgs e)
        {
            if (ChildrenChanged != null)
                ChildrenChanged(this, e);
        }
        void _children_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        { OnChildrenChanged(e); }
    }
}
