using System;
using System.Threading.Tasks;
using M.DBus;
using M.DBus.Services.Kde;
using M.DBus.Tray;
using osu.Framework.Graphics;
using osu.Framework.Logging;
using Tmds.DBus;

#nullable disable

namespace osu.Desktop.DBus.Tray
{
    public partial class TrayManager : Component, IHandleTrayManagement
    {
        private DBusMgrNew dBusManager;

        public TrayIconService KdeTrayService { get; private set; } = new TrayIconService();
        public CanonicalTrayService CanonicalTrayService { get; private set; } = new CanonicalTrayService();

        public void ReloadTray()
        {
            var prevCanonicalSrv = this.CanonicalTrayService;

            this.KdeTrayService = new TrayIconService();
            this.CanonicalTrayService = new CanonicalTrayService();

            this.CanonicalTrayService.AddEntryRange(prevCanonicalSrv.GetEntries());
        }

        internal void SetDBusManager(DBusMgrNew dBusManager)
        {
            if (this.dBusManager == dBusManager) return;

            this.dBusManager = dBusManager;

            ReloadTray();

            dBusManager.OnObjectRegisteredToConnection += o =>
            {
                if (o is TrayIconService)
                    this.Scheduler.AddDelayed(() =>
                        Task.Run(ConnectToWatcher), 300);
            };
        }

        private IStatusNotifierWatcher trayWatcher;

        public async Task<bool> ConnectToWatcher()
        {
            try
            {
                trayWatcher = dBusManager.GetProxyObject<IStatusNotifierWatcher>(new ObjectPath("/StatusNotifierWatcher"), "org.kde.StatusNotifierWatcher");

                await trayWatcher.RegisterStatusNotifierItemAsync("org.kde.StatusNotifierItem.mfosu").ConfigureAwait(false);
            }
            catch (Exception e)
            {
                trayWatcher = null;
                Logger.Error(e, "未能连接到 org.kde.StatusNotifierWatcher");
                return false;
            }

            return true;
        }

        public SimpleEntry[] GetEntries()
        {
            return CanonicalTrayService.GetEntries();
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
