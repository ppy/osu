using System;
using System.Threading.Tasks;
using M.DBus;
using M.DBus.Services.Kde;
using M.DBus.Tray;
using osu.Framework.Graphics;
using osu.Framework.Logging;
using Tmds.DBus;

namespace osu.Desktop.DBus.Tray
{
    public partial class TrayManager : Component, IHandleTrayManagement
    {
        private DBusManager<IMDBusObject> dBusManager;

        public readonly KdeStatusTrayService KdeTrayService = new KdeStatusTrayService();
        public readonly CanonicalTrayService CanonicalTrayService = new CanonicalTrayService();

        internal void SetDBusManager(DBusManager<IMDBusObject> dBusManager)
        {
            this.dBusManager = dBusManager;
        }

        private IStatusNotifierWatcher trayWatcher;

        public async Task<bool> ConnectToWatcher()
        {
            try
            {
                trayWatcher = dBusManager.GetDBusObject<IStatusNotifierWatcher>(new ObjectPath("/StatusNotifierWatcher"), "org.kde.StatusNotifierWatcher");

                await trayWatcher.RegisterStatusNotifierItemAsync("org.kde.StatusNotifierItem.mfosu").ConfigureAwait(false);
            }
            catch (Exception e)
            {
                trayWatcher = null;
                Logger.Error(e, "未能连接到 org.kde.StatusNotifierWatcher, 请检查相关配置");
                return false;
            }

            return true;
        }

        public void AddEntry(SimpleEntry entry)
            => CanonicalTrayService.AddEntryToMenu(entry);

        public void AddEntryRange(SimpleEntry[] entries)
            => CanonicalTrayService.AddEntryRange(entries);

        public void RemoveEntry(SimpleEntry entry)
            => CanonicalTrayService.RemoveEntryFromMenu(entry);

        public void RemoveEntryRange(SimpleEntry[] entries)
        {
            foreach (var entry in entries)
            {
                RemoveEntry(entry);
            }
        }
    }
}
