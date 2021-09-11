using System;
using M.DBus;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
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

        private readonly Bindable<UserActivity> bindableActivity = new Bindable<UserActivity>();
        private readonly MprisPlayerService mprisService = new MprisPlayerService();

        //private readonly KdeStatusTrayService kdeTrayService = new KdeStatusTrayService();

        [BackgroundDependencyLoader]
        private void load(IAPIProvider api, MConfigManager config, Storage storage, OsuGame game)
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

                //DBusManager.RegisterNewObject(kdeTrayService,
                //    "org.kde.StatusNotifierItem.mfosu");

                DBusManager.OnConnected -= onDBusConnected;
            }

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
            mprisService.Play += () => musicController.Play();
            mprisService.Pause += () => musicController.Stop(true);
            mprisService.Quit += game.GracefullyExit;
            mprisService.Seek += t => musicController.SeekTo(t);
            mprisService.Stop += () => musicController.Stop(true);
            mprisService.PlayPause += () => musicController.TogglePause();
            mprisService.OpenUri += game.HandleLink;
            mprisService.WindowRaise += raiseWindow;

            //kdeTrayService.WindowRaise += raiseWindow;
        }

        private void raiseWindow()
        {
            (host.Window as SDL2DesktopWindow)?.Raise();
        }

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

        public Action<Notification> NotificationAction { get; set; }

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

        protected override void Dispose(bool isDisposing)
        {
            DBusManager.Dispose();
            base.Dispose(isDisposing);
        }
    }
}
