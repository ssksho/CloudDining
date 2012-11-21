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
        public ComplexCloudNode(DateTime? raiseTime = null)
            : base(raiseTime)
        {
            Children = new ObservableCollection<CloudNode>();
            Children.CollectionChanged += Children_CollectionChanged;
        }
        public ObservableCollection<CloudNode> Children { get; private set; }
        void Children_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        { OnChildrenChanged(e); }

        public event NotifyCollectionChangedEventHandler ChildrenChanged;
        protected virtual void OnChildrenChanged(NotifyCollectionChangedEventArgs e)
        {
            if (ChildrenChanged != null)
                ChildrenChanged(this, e);
        }
    }
}
