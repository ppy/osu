using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using M.DBus;
using M.DBus.Services;
using M.DBus.Services.Kde;
using M.DBus.Services.Notifications;
using M.DBus.Tray;
using M.DBus.Utils;
using osu.Desktop.DBus.Tray;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Textures;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Game;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Online.API;
using osu.Game.Overlays;
using osu.Game.Overlays.Notifications;
using osu.Game.Rulesets;
using osu.Game.Users;
using Tmds.DBus;

namespace osu.Desktop.DBus
{
    public class DBusManagerContainer : Component, IHandleTrayManagement, IHandleSystemNotifications
    {
        public DBusManager DBusManager;

        public Action<Notification> NotificationAction { get; set; }
        private readonly Bindable<bool> controlSource;

        private readonly Bindable<UserActivity> bindableActivity = new Bindable<UserActivity>();
        private readonly MprisPlayerService mprisService = new MprisPlayerService();

        private readonly KdeStatusTrayService kdeTrayService = new KdeStatusTrayService();
        private readonly CanonicalTrayService canonicalTrayService = new CanonicalTrayService();
        private SDL2DesktopWindow sdl2DesktopWindow => (SDL2DesktopWindow)host.Window;

        private BeatmapInfoDBusService beatmapService;
        private AudioInfoDBusService audioservice;
        private UserInfoDBusService userInfoService;

        [Resolved]
        private IBindable<WorkingBeatmap> beatmap { get; set; }

        [Resolved]
        private IBindable<RulesetInfo> ruleset { get; set; }

        [Resolved]
        private MusicController musicController { get; set; }

        [Resolved]
        private GameHost host { get; set; }

        [Resolved]
        private OsuGame game { get; set; }

        [Resolved]
        private LargeTextureStore textureStore { get; set; }

        [Resolved]
        private MConfigManager config { get; set; }

        public DBusManagerContainer(bool autoStart = false, Bindable<bool> controlSource = null)
        {
            if (autoStart && controlSource != null)
                this.controlSource = controlSource;
            else if (controlSource == null && autoStart) throw new InvalidOperationException("设置了自动启动但是控制源是null?");

            DBusManager = new DBusManager(false, this, this);
        }

        #region Disposal

        protected override void Dispose(bool isDisposing)
        {
            DBusManager.Dispose();
            base.Dispose(isDisposing);
        }

        #endregion

        protected override void LoadComplete()
        {
            controlSource?.BindValueChanged(onControlSourceChanged, true);
            base.LoadComplete();
        }

        protected override void Update()
        {
            base.Update();
            mprisService.TrackRunning = musicController.CurrentTrack.IsRunning;
        }

        private Bindable<bool> enableTray;
        private Bindable<bool> enableSystemNotifications;

        [BackgroundDependencyLoader]
        private void load(IAPIProvider api, Storage storage)
        {
            DBusManager.RegisterNewObjects(new IDBusObject[]
            {
                beatmapService = new BeatmapInfoDBusService(),
                audioservice = new AudioInfoDBusService(),
                userInfoService = new UserInfoDBusService()
            });

            DBusManager.GreetService.AllowPost = config.GetBindable<bool>(MSetting.DBusAllowPost);
            DBusManager.GreetService.OnMessageRecive = onMessageRevicedFromDBus;

            void onDBusConnected()
            {
                DBusManager.RegisterNewObject(mprisService,
                    "org.mpris.MediaPlayer2.mfosu");

                canonicalTrayService.AddEntryRange(new[]
                {
                    new SimpleEntry
                    {
                        Label = "mfosu",
                        Enabled = false,
                        IconData = textureStore.GetStream("avatarlogo")?.ToByteArray()
                                   ?? SimpleEntry.EmptyPngBytes
                    },
                    new SimpleEntry
                    {
                        Label = "隐藏/显示窗口",
                        OnActive = () =>
                        {
                            sdl2DesktopWindow.Visible = !sdl2DesktopWindow.Visible;
                        },
                        IconName = "window-pop-out"
                    },
                    new SimpleEntry
                    {
                        Label = "退出",
                        OnActive = exitGame,
                        IconName = "application-exit"
                    },
                    new SeparatorEntry
                    {
                        Label = "分割符"
                    }
                });

                enableTray.BindValueChanged(onEnableTrayChanged, true);
                enableSystemNotifications.BindValueChanged(onEnableNotificationsChanged, true);

                DBusManager.OnConnected -= onDBusConnected;
            }

            enableTray = config.GetBindable<bool>(MSetting.EnableTray);
            enableSystemNotifications = config.GetBindable<bool>(MSetting.EnableSystemNotifications);

            DBusManager.OnConnected += onDBusConnected;

            api.LocalUser.BindValueChanged(onUserChanged, true);
            beatmap.BindValueChanged(onBeatmapChanged, true);
            ruleset.BindValueChanged(v => userInfoService.SetProperty(nameof(UserMetadataProperties.CurrentRuleset), v.NewValue?.Name ?? "???"), true);
            bindableActivity.BindValueChanged(v => userInfoService.SetProperty(nameof(UserMetadataProperties.Activity), v.NewValue?.Status ?? "空闲"), true);

            mprisService.Storage = storage;
            beatmapService.Storage = storage;

            mprisService.UseAvatarLogoAsDefault = config.GetBindable<bool>(MSetting.MprisUseAvatarlogoAsCover);
            mprisService.Next += () => musicController.NextTrack();
            mprisService.Previous += () => musicController.PreviousTrack();
            mprisService.Play += () => musicController.Play(requestedByUser: true);
            mprisService.Pause += () => musicController.Stop(true);
            mprisService.Quit += exitGame;
            mprisService.Seek += t => musicController.SeekTo(t);
            mprisService.Stop += () => musicController.Stop(true);
            mprisService.PlayPause += () => musicController.TogglePause();
            mprisService.OpenUri += game.HandleLink;
            mprisService.WindowRaise += raiseWindow;

            kdeTrayService.WindowRaise += raiseWindow;
        }

        private void onEnableTrayChanged(ValueChangedEvent<bool> v)
        {
            if (v.NewValue)
            {
                DBusManager.RegisterNewObject(canonicalTrayService,
                    "io.matrix_feather.dbus.menu");

                DBusManager.RegisterNewObject(kdeTrayService,
                    "org.kde.StatusNotifierItem.mfosu");

                Task.Run(ConnectToWatcher);
            }
            else
            {
                DBusManager.UnRegisterObject(kdeTrayService);
                DBusManager.UnRegisterObject(canonicalTrayService);
            }
        }

        private void raiseWindow()
        {
            if (!sdl2DesktopWindow.Visible) sdl2DesktopWindow.Visible = true;

            sdl2DesktopWindow.Raise();
        }

        private void exitGame()
            => Schedule(game.GracefullyExit);

        private void onMessageRevicedFromDBus(string message)
        {
            NotificationAction.Invoke(new SimpleNotification
            {
                Text = "收到一条来自DBus的消息: \n" + message
            });

            Logger.Log($"收到一条来自DBus的消息: {message}");
        }

        private void onUserChanged(ValueChangedEvent<User> v)
        {
            bindableActivity.UnbindBindings();
            bindableActivity.BindTo(v.NewValue.Activity);
            userInfoService.User = v.NewValue;
        }

        private void onBeatmapChanged(ValueChangedEvent<WorkingBeatmap> v)
        {
            beatmapService.Beatmap = v.NewValue;
            mprisService.Beatmap = v.NewValue;
            audioservice.Beatmap = v.NewValue;
        }

        private void onControlSourceChanged(ValueChangedEvent<bool> v)
        {
            if (v.NewValue)
                DBusManager.Connect();
            else
                DBusManager.Disconnect();
        }

        #region 托盘

        private IStatusNotifierWatcher trayWatcher;

        public async Task<bool> ConnectToWatcher()
        {
            try
            {
                trayWatcher = DBusManager.GetDBusObject<IStatusNotifierWatcher>(new ObjectPath("/StatusNotifierWatcher"), "org.kde.StatusNotifierWatcher");

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
        {
            canonicalTrayService.AddEntryToMenu(entry);
        }

        public void RemoveEntry(SimpleEntry entry)
        {
            canonicalTrayService.RemoveEntryFromMenu(entry);
        }

        #endregion

        #region 通知

        private void onEnableNotificationsChanged(ValueChangedEvent<bool> v)
        {
            if (v.NewValue)
            {
                connectToNotifications();
            }
            else
            {
                systemNotification = null;
            }
        }

        private INotifications systemNotification;

        private bool notificationWatched;
        private readonly Dictionary<uint, SystemNotification> notifications = new Dictionary<uint, SystemNotification>();

        private bool connectToNotifications()
        {
            try
            {
                var path = new ObjectPath("/org/freedesktop/Notifications");
                systemNotification = DBusManager.GetDBusObject<INotifications>(path, path.ToServiceName());

                if (!notificationWatched)
                {
                    //bug: 在gnome上会导致调用两次？
                    systemNotification.WatchActionInvokedAsync(onActionInvoked);
                    systemNotification.WatchNotificationClosedAsync(onNotificationClosed);
                    notificationWatched = true;
                }
            }
            catch (Exception e)
            {
                systemNotification = null;
                notificationWatched = false;
                Logger.Error(e, "未能连接到 org.freedesktop.Notifications, 请检查相关配置");
                return false;
            }

            return true;
        }

        private void onNotificationClosed((uint id, uint reason) singal)
        {
            SystemNotification notification;

            if (notifications.TryGetValue(singal.id, out notification))
            {
                notification.OnClosed?.Invoke(singal.reason.ToCloseReason());
                notifications.Remove(singal.id);
            }
        }

        private void onActionInvoked((uint id, string actionKey) obj)
        {
            SystemNotification notification;

            if (notifications.TryGetValue(obj.id, out notification))
            {
                notification.Actions.FirstOrDefault(a => a.Id == obj.actionKey)?.OnInvoked?.Invoke();
                notification.OnClosed?.Invoke(CloseReason.ActionInvoked);

                notifications.Remove(obj.id);
                Task.Run(async () => await CloseNotificationAsync(obj.id).ConfigureAwait(false));
            }
        }

        public async Task<uint> PostAsync(SystemNotification notification)
        {
            try
            {
                if (systemNotification != null)
                {
                    var target = notification.ToDBusObject();

                    var result = await systemNotification.NotifyAsync(target.appName,
                        target.replacesID,
                        target.appIcon,
                        target.title,
                        target.description,
                        target.actions,
                        target.hints,
                        target.displayTime).ConfigureAwait(false);

                    notifications[result] = notification;
                    return result;
                }
            }
            catch (Exception e)
            {
                Logger.Error(e, "发送系统通知时出现了问题");
            }

            return 0;
        }

        public async Task<bool> CloseNotificationAsync(uint id)
        {
            if (systemNotification != null)
            {
                await systemNotification.CloseNotificationAsync(id).ConfigureAwait(false);
                return true;
            }

            return false;
        }

        public async Task<string[]> GetCapabilitiesAsync()
        {
            if (systemNotification != null)
            {
                return await systemNotification.GetCapabilitiesAsync().ConfigureAwait(false);
            }

            return Array.Empty<string>();
        }

        private readonly (string name, string vendor, string version, string specVersion) defaultServerInfo = ("mfosu", "mfosu", "0", "0");

        public async Task<(string name, string vendor, string version, string specVersion)> GetServerInformationAsync()
        {
            if (systemNotification != null)
            {
                return await systemNotification.GetServerInformationAsync().ConfigureAwait(false);
            }

            return defaultServerInfo;
        }

        #endregion
    }
}
