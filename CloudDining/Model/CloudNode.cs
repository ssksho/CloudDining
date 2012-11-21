using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace CloudDining.Model
{
    public class CloudNode : BaseNode
    {
        public CloudNode(Account owner, Controls.CloudStateType status, DateTime? raiseTime = null)
            : base(raiseTime)
        {
            Owner = owner;
            Status = status;
        }
        Controls.CloudStateType _status;

        public Account Owner { get; private set; }
        public Controls.CloudStateType Status
        {
            get { return _status; }
            set
            {
                _status = value;
                OnStatusChanged(new ExEventArgs<Controls.CloudStateType>(value));
            }
        }

        public event EventHandler<ExEventArgs<Controls.CloudStateType>> StatusChanged;
        protected virtual void OnStatusChanged(ExEventArgs<Controls.CloudStateType> e)
        {
            if (StatusChanged != null)
                StatusChanged(this, e);
        }
    }
}
