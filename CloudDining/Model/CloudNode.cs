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
        public CloudNode(Account owner, DateTime? raiseTime = null)
            : base(raiseTime) { Owner = owner; }
        WeatherType _status;

        public Account Owner { get; private set; }
        public WeatherType Status
        {
            get { return _status; }
            set
            {
                _status = value;
                OnStatusChanged(new ExEventArgs<WeatherType>(value));
            }
        }

        public event EventHandler<ExEventArgs<WeatherType>> StatusChanged;
        protected virtual void OnStatusChanged(ExEventArgs<WeatherType> e)
        {
            if (StatusChanged != null)
                StatusChanged(this, e);
        }
    }
    public enum WeatherType
    { Sunny, Rainny, Cloudy }
}
