using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace CloudDining.Model
{
    public class BaseNode
    {
        public BaseNode(DateTime? raiseTime = null)
        {
            RaiseTime = raiseTime ?? DateTime.Now;
        }
        public bool IsOpened { get; private set; }
        public virtual DateTime RaiseTime { get; protected set; }
        public FrameworkElement Element { get; set; }
        public virtual void Open()
        {
            IsOpened = true;
            OnIsOpenedChanged(new ExEventArgs<bool>(IsOpened));
        }
        public virtual void Close()
        {
            IsOpened = false;
            OnIsOpenedChanged(new ExEventArgs<bool>(IsOpened));
        }

        public event EventHandler<ExEventArgs<bool>> IsOpenedChanged;
        protected virtual void OnIsOpenedChanged(ExEventArgs<bool> e)
        {
            if (IsOpenedChanged != null)
                IsOpenedChanged(this, e);
        }
    }
}
