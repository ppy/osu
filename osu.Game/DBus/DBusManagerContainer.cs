using System;
using M.DBus;
using osu.Framework.Bindables;
using osu.Framework.Graphics;

namespace osu.Game.DBus
{
    public class DBusManagerContainer : Component
    {
        public readonly DBusManager DBusManager = new DBusManager(false);
        private readonly Bindable<bool> controlSource;

        public DBusManagerContainer(bool autoStart = false, Bindable<bool> controlSource = null)
        {
            if (autoStart && controlSource != null)
            {
                this.controlSource = controlSource;
            }
            else if (controlSource == null && autoStart)
            {
                throw new InvalidOperationException("设置了自动启动但是控制源是null?");
            }
        }

        protected override void LoadComplete()
        {
            controlSource?.BindValueChanged(onControlSourceChanged, true);
            base.LoadComplete();
        }

        private void onControlSourceChanged(ValueChangedEvent<bool> v)
        {
            if (v.NewValue)
                DBusManager.Connect();
            else
                DBusManager.Disconnect();
        }

        protected override void Dispose(bool isDisposing)
        {
            DBusManager.Dispose();
            base.Dispose(isDisposing);
        }
    }
}
