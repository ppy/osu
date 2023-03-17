using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using M.DBus;
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
using osu.Framework.Utils;
using osu.Game;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays;
using osu.Game.Overlays.Notifications;
using osu.Game.Rulesets;
using osu.Game.Users;

#nullable disable

namespace osu.Desktop.DBus
{
    public partial class DBusManagerContainer : Component, IDBusManagerContainer<IMDBusObject>
    {
        public DBusManager<IMDBusObject> DBusManager;

        public Action<Notification> NotificationAction { get; set; }
        private readonly Bindable<bool> controlSource;
        private readonly BindableDouble mprisUpdateInterval = new BindableDouble();

        private readonly Bindable<UserActivity> bindableActivity = new Bindable<UserActivity>();
        private readonly MprisPlayerService mprisService = new MprisPlayerService();

        private KdeStatusTrayService kdeTrayService => trayManager.KdeTrayService;
        private CanonicalTrayService canonicalTrayService => trayManager.CanonicalTrayService;

        private SDL2DesktopWindow sdl2DesktopWindow => (SDL2DesktopWindow)host.Window;

        private BeatmapInfoDBusService beatmapService;
        private AudioInfoDBusService audioservice;
        private UserInfoDBusService userInfoService;

        private readonly TrayManager trayManager = new TrayManager();
        private readonly SystemNotificationManager systemNotificationManager = new SystemNotificationManager();

        #region 依赖

        [Resolved]
        private BeatmapManager beatmapManager { get; set; }

        [Resolved]
        private Bindable<WorkingBeatmap> beatmap { get; set; }

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

        #endregion

        public DBusManagerContainer(bool autoStart = false, Bindable<bool> controlSource = null)
        {
            if (autoStart && controlSource != null)
                this.controlSource = controlSource;
            else if (controlSource == null && autoStart) throw new InvalidOperationException("设置了自动启动但是控制源是null?");

            DBusManager = new DBusManager<IMDBusObject>(false, trayManager, systemNotificationManager);
            trayManager.SetDBusManager(DBusManager);
            systemNotificationManager.SetDBusManager(DBusManager);
        }

        private readonly BindableBool trackLooping = new BindableBool();

        protected override void LoadComplete()
        {
            controlSource?.BindValueChanged(onControlSourceChanged, true);
            beatmap.DisabledChanged += b => mprisService.BeatmapDisabled = b;

            base.LoadComplete();
        }

        protected override void Update()
        {
            base.Update();
            mprisService.TrackRunning = musicController.CurrentTrack.IsRunning;
            trackLooping.Value = musicController.CurrentTrack.Looping;
        }

        private Bindable<bool> enableTray;
        private Bindable<bool> enableSystemNotifications;

        private Bindable<string> iconName;

        #region IDBusManagerContainer

        public void Add(IMDBusObject obj)
        {
            if (!string.IsNullOrEmpty(obj.CustomRegisterName))
                DBusManager.RegisterNewObject(obj, obj.CustomRegisterName).ConfigureAwait(false);
            else
                DBusManager.RegisterNewObject(obj).ConfigureAwait(false);
        }

        public void AddRange(IEnumerable<IMDBusObject> objects)
        {
            foreach (var obj in objects)
                Add(obj);
        }

        public void Remove(IMDBusObject obj)
        {
            DBusManager.RemoveObject(obj);
        }

        public void RemoveRange(IEnumerable<IMDBusObject> objects)
        {
            foreach (var obj in objects)
                Remove(obj);
        }

        public void PostSystemNotification(SystemNotification notification)
        {
            systemNotificationManager.PostAsync(notification).ConfigureAwait(false);
        }

        public void AddTrayEntry(SimpleEntry entry)
        {
            trayManager.AddEntry(entry);
        }

        public void RemoveTrayEntry(SimpleEntry entry)
        {
            trayManager.RemoveEntry(entry);
        }

        #endregion

        [BackgroundDependencyLoader]
        private void load(IAPIProvider api, Storage storage)
        {
            AddRange(new IMDBusObject[]
            {
                beatmapService = new BeatmapInfoDBusService(),
                audioservice = new AudioInfoDBusService(),
                userInfoService = new UserInfoDBusService()
            });

            DBusManager.GreetService.AllowPost = config.GetBindable<bool>(MSetting.DBusAllowPost);
            DBusManager.GreetService.OnMessageRecive = onMessageRevicedFromDBus;

            iconName = config.GetBindable<string>(MSetting.TrayIconName);
            enableTray = config.GetBindable<bool>(MSetting.EnableTray);
            enableSystemNotifications = config.GetBindable<bool>(MSetting.EnableSystemNotifications);
            config.BindWith(MSetting.MprisUpdateInterval, mprisUpdateInterval);

            void postConnect()
            {
                this.Add(mprisService);

                trayManager.AddEntryRange(new[]
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

                            if (sdl2DesktopWindow.Visible)
                                raiseWindow();
                        },
                        IconName = "window-pop-out"
                    },
                    new SimpleEntry
                    {
                        Label = "退出",
                        OnActive = exitGame,
                        IconName = "application-exit"
                    },
                    new SeparatorEntry()
                });

                enableTray.BindValueChanged(onEnableTrayChanged, true);
                enableSystemNotifications.BindValueChanged(systemNotificationManager.OnEnableNotificationsChanged, true);

                //workaround: 执行到这里时一些dbus服务可能还没注册完成，因此延迟一些时间执行这些
                Scheduler.AddDelayed(() =>
                {
                    game.Audio.Volume.BindValueChanged(v =>
                        mprisService.Set("Volume", v.NewValue), true);

                    trackLooping.BindValueChanged(v =>
                    {
                        mprisService.Set("LoopStatus", v.NewValue ? "Track" : "Playlist");
                    }, true);

                    iconName.BindValueChanged(v =>
                    {
                        kdeTrayService.Set(nameof(kdeTrayService.KdeProperties.IconName), v.NewValue);
                    }, true);

                    mprisService.AllowSet = true;

                    //workaround: 让GNOME插件Mpris Indicator Button能触发Shuffle
                    mprisService.TriggerPropertyChangeFor("Shuffle");

                    updateMprisProgress();
                }, config.Get<double>(MSetting.DBusWaitOnline));

                DBusManager.OnConnected -= scheduleFirstConnected;
            }

            void scheduleFirstConnected() => Schedule(postConnect);

            DBusManager.OnConnected += scheduleFirstConnected;

            api.LocalUser.BindValueChanged(onUserChanged, true);
            beatmap.BindValueChanged(onBeatmapChanged, true);
            ruleset.BindValueChanged(v => userInfoService.SetProperty(nameof(UserMetadataProperties.CurrentRuleset), v.NewValue?.Name ?? "???"), true);
            bindableActivity.BindValueChanged(v => userInfoService.SetProperty(nameof(UserMetadataProperties.Activity), v.NewValue?.GetStatus() ?? "空闲"), true);

            mprisService.Storage = storage;
            beatmapService.Storage = storage;

            mprisService.UseAvatarLogoAsDefault = config.GetBindable<bool>(MSetting.MprisUseAvatarlogoAsCover);

            //添加Schedule确保这些调用会在Update上执行
            mprisService.Next += () => musicController.NextTrack(); //NextTrack和PreviousTrack已经有Schedule了
            mprisService.Previous += () => musicController.PreviousTrack();
            mprisService.Play += () => Schedule(() => musicController.Play(requestedByUser: true));
            mprisService.Pause += () => Schedule(() => musicController.Stop(true));
            mprisService.Quit += exitGame;
            mprisService.Stop += () => Schedule(() => musicController.Stop(true));
            mprisService.PlayPause += () => Schedule(() => musicController.TogglePause());
            mprisService.OpenUri += s => Schedule(() => game.HandleLink(s));
            mprisService.WindowRaise += raiseWindow;
            mprisService.OnVolumeSet += v => Schedule(() => game.Audio.Volume.Value = v);
            mprisService.Seek += t => Schedule(() =>
            {
                double target = musicController.CurrentTrack.CurrentTime + (t / 1000d);

                if (target <= 0)
                {
                    musicController.SeekTo(0);
                    return;
                }

                if (target >= musicController.CurrentTrack.Length)
                    musicController.NextTrack();
                else
                    musicController.SeekTo(target);

                mprisService.Progress = (long)(target * 1000);
            });

            mprisService.SetPosition += pos => Schedule(() =>
            {
                musicController.SeekTo(pos / 1000d);
            });

            mprisService.OnRandom += () =>
            {
                var usableBeatmapSets = beatmapManager.GetAllUsableBeatmapSets();
                int num = RNG.Next(0, usableBeatmapSets.Count - 1);

                var info = usableBeatmapSets[num].Beatmaps.FirstOrDefault();

                if (info != null && !beatmap.Disabled)
                {
                    Schedule(() =>
                    {
                        beatmap.Value = beatmapManager.GetWorkingBeatmap(info);
                        musicController.Play();
                    });
                }
            };

            kdeTrayService.WindowRaise += raiseWindow;
        }

        private void onEnableTrayChanged(ValueChangedEvent<bool> v)
        {
            if (v.NewValue)
            {
                Add(canonicalTrayService);
                Add(kdeTrayService);

                Task.Run(trayManager.ConnectToWatcher);
            }
            else
            {
                Remove(canonicalTrayService);
                Remove(kdeTrayService);
            }
        }

        private void raiseWindow()
        {
            Schedule(() =>
            {
                if (!sdl2DesktopWindow.Visible) sdl2DesktopWindow.Visible = true;

                sdl2DesktopWindow.Raise();
            });
        }

        private void exitGame() => Schedule(game.Exit);

        private void onMessageRevicedFromDBus(string message)
        {
            NotificationAction.Invoke(new SimpleNotification
            {
                Text = "收到一条来自DBus的消息: \n" + message
            });

            Logger.Log($"收到一条来自DBus的消息: {message}");
        }

        private void onUserChanged(ValueChangedEvent<APIUser> v)
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
            //else
            //    DBusManager.Disconnect();
        }

        private void updateMprisProgress()
        {
            mprisService.Progress = (long)(musicController.CurrentTrack.CurrentTime * 1000);
            mprisService.TrackLength = (long)(musicController.CurrentTrack.Length * 1000);

            Scheduler.AddDelayed(updateMprisProgress, mprisUpdateInterval.Value);
        }

        #region Disposal

        protected override void Dispose(bool isDisposing)
        {
            DBusManager.Dispose();
            base.Dispose(isDisposing);
        }

        #endregion
    }
}
