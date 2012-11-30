using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace CloudDining.Model
{
    public class CloudNode
    {
        public CloudNode(Account owner, Controls.CloudStateType status, int cloudTypeId, DateTime checkinTime, TimeSpan checkinSpan, DateTime? raiseTime = null)
        {
            Owner = owner;
            Status = status;
            CloudTypeId = cloudTypeId;
            CheckinTime = checkinTime;
            CheckinSpan = checkinSpan;
        }
        Controls.CloudStateType _status;

        public Account Owner { get; private set; }
        public DateTime CheckinTime { get; private set; }
        public TimeSpan CheckinSpan { get; private set; }
        public UIElement TimeshiftElement { get; set; }
        public UIElement HomeElement { get; set; }
        public Controls.CloudStateType Status
        {
            get { return _status; }
            set
            {
                _status = value;
                OnStatusChanged(new ExEventArgs<Controls.CloudStateType>(value));
            }
        }
        public int CloudTypeId { get; private set; }

        public event EventHandler<ExEventArgs<Controls.CloudStateType>> StatusChanged;
        protected virtual void OnStatusChanged(ExEventArgs<Controls.CloudStateType> e)
        {
            if (StatusChanged != null)
                StatusChanged(this, e);
        }
    }
}
