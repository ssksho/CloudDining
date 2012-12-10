using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CloudDining.Model
{
    public class PlaneNode : BaseNode
    {
        public PlaneNode(Uri picture, Account owner, DateTime? raiseTime)
            : base(raiseTime)
        {
            Picture = picture;
            Owner = owner;
        }

        public Account Owner { get; private set; }
        public Uri Picture { get; private set; }
        public bool IsReaded { get; private set; }
        public override void Close()
        {
            base.Close();
            if (IsReaded)
                return;

            IsReaded = true;
            OnIsReadedChanged(new ExEventArgs<bool>(IsReaded));
        }

        public event EventHandler<ExEventArgs<bool>> IsReadedChanged;
        protected virtual void OnIsReadedChanged(ExEventArgs<bool> e)
        {
            if (IsReadedChanged != null)
                IsReadedChanged(this, e);
        }

        public static PlaneNode[] CreatePlanePair(Uri imageUrl, Account owner, DateTime? raiseTime)
        {
            var home = new PlaneNode(imageUrl, owner, raiseTime);
            var time = new PlaneNode(imageUrl, owner, raiseTime);
            home.IsReadedChanged += (sender, e) =>
            {
                if (time.IsReaded)
                    return;
                time.IsReaded = true;
                time.OnIsReadedChanged(new ExEventArgs<bool>(true));
            };
            time.IsReadedChanged += (sender, e) =>
            {
                if (home.IsReaded)
                    return;
                home.IsReaded = true;
                home.OnIsReadedChanged(new ExEventArgs<bool>(true));
            };
            return new PlaneNode[] { home, time };
        }
    }
}
