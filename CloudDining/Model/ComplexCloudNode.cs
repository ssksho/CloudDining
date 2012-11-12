using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace CloudDining.Model
{
    public class ComplexCloudNode : BaseNode
    {
        public ComplexCloudNode() : base() { Children = new ObservableCollection<CloudNode>(); }
        public ObservableCollection<CloudNode> Children { get; private set; }
    }
}
