using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using M.DBus.Tray;
using M.Resources.Localisation.LLin;
using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Bindables;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Audio;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Framework.Logging;
using osu.Framework.Screens;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Input;
using osu.Game.Input.Bindings;
using osu.Game.Overlays;
using osu.Game.Overlays.Dialog;
using osu.Game.Overlays.Notifications;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Screens.LLin.Misc;
using osu.Game.Screens.LLin.Plugins;
using osu.Game.Screens.LLin.Plugins.Internal.FallbackFunctionBar;
using osu.Game.Screens.LLin.Plugins.Types;
using osu.Game.Screens.LLin.SideBar;
using osu.Game.Screens.LLin.SideBar.Settings;
using osu.Game.Screens.LLin.SideBar.Tabs;
using osu.Game.Screens.Play;
using osu.Game.Screens.Select;
using osu.Game.Users;
using osuTK;
using osuTK.Graphics;
using osuTK.Input;

namespace osu.Game.Screens.LLin
{
    [Cached(typeof(IImplementLLin))]
    public partial class LLinScreen : ScreenWithBeatmapBackground, IImplementLLin, IKeyBindingHandler<GlobalAction>
    {
        public Action<bool>? OnTrackRunningToggle { get; set; }
        public Action? Exiting { get; set; }
        public Action? Suspending { get; set; }
        public Action? Resuming { get; set; }
        public Action? OnIdle { get; set; }
        public Action? OnActive { get; set; }

        #region 全局依赖

        [Resolved]
        private LLinPluginManager pluginManager { get; set; } = null!;

        [Resolved]
        private IDialogOverlay dialog { get; set; } = null!;

        [Resolved]
        private INotificationOverlay notifications { get; set; } = null!;

        [Resolved(CanBeNull = true)]
        private OsuGame? game { get; set; }

        [Cached]
        private CustomColourProvider colourProvider = new CustomColourProvider();

        [Cached]
        private BeatmapHashResolver hashResolver = new BeatmapHashResolver();

        #endregion

        #region 音频

        private IProvideAudioControlPlugin? realAudioControlPlugin;

        private IProvideAudioControlPlugin audioControlPlugin
        {
            get => realAudioControlPlugin ?? pluginManager.DefaultAudioController;
            set => realAudioControlPlugin = value;
        }

        public Action<double>? OnSeek { get; set; }

        public bool SeekTo(double position)
        {
            bool success = audioControlPlugin.Seek(position);

            if (success)
                OnSeek?.Invoke(position);

            return success;
        }

        public DrawableTrack CurrentTrack => audioControlPlugin.GetCurrentTrack();

        public void RequestAudioControl(IProvideAudioControlPlugin pacp, LocalisableString message, Action? onDeny, Action? onAllow)
        {
            if (!(pacp is LLinPlugin mpl)) return;

            dialog.Push(new ConfirmDialog(
                mpl.ToString()
                + LLinBaseStrings.AudioControlRequestedMain
                + "\n"
                + LLinBaseStrings.AudioControlRequestedSub(message.ToString()),
                () =>
                {
                    changeAudioControlProvider(pacp);
                    onAllow?.Invoke();
                },
                onDeny));
        }

        public void ReleaseAudioControlFrom(IProvideAudioControlPlugin pacp)
        {
            if (audioControlPlugin == pacp)
                changeAudioControlProvider(null);
        }

        private void changeAudioControlProvider(IProvideAudioControlPlugin? pacp)
        {
            //如果没找到(为null)，则解锁Beatmap.Disabled
            Beatmap.Disabled = (pacp != null) && (pacp != pluginManager.DefaultAudioController);

            //设置当前控制插件IsCurrent为false
            audioControlPlugin.IsCurrent = false;

            //切换并设置当前控制插件IsCurrent为true
            audioControlPlugin = pacp ?? pluginManager.DefaultAudioController;
            audioControlPlugin.IsCurrent = true;
            //Logger.Log($"更改控制插件到{audioControlProvider}");
        }

        private readonly LLinModRateAdjust modRateAdjust = new LLinModRateAdjust();

        private void updateTrackAdjustments()
        {
            //modRate
            modRateAdjust.SpeedChange.Value = musicSpeed.Value;

            CurrentTrack.ResetSpeedAdjustments();
            CurrentTrack.Looping = loopToggleButton.Bindable.Value;
            CurrentTrack.RestartPoint = 0;
            CurrentTrack.AddAdjustment(adjustFreq.Value ? AdjustableProperty.Frequency : AdjustableProperty.Tempo, musicSpeed);
        }

        #endregion

        #region 谱面

        private Action<WorkingBeatmap>? onBeatmapChangedAction;

        public void OnBeatmapChanged(Action<WorkingBeatmap> action, object sender, bool runOnce = false)
        {
            bool alreadyRegistered = onBeatmapChangedAction?.GetInvocationList().Contains(action) ?? false;

            if (sender.GetType().IsSubclassOf(typeof(LLinPlugin))
                && pluginManager.GetAllPlugins(false).Contains((LLinPlugin)sender)
                && runOnce
                && alreadyRegistered)
            {
                action.Invoke(Beatmap.Value);
                return;
            }

            if (alreadyRegistered)
                throw new InvalidOperationException($"{sender}已经注册过一个相同的{action}了。");

            onBeatmapChangedAction += action;

            if (runOnce) action.Invoke(Beatmap.Value);
        }

        /// <summary>
        /// 旧方法，可能需要重做
        /// 谱面更新事件
        /// </summary>
        /// <param name="v">新谱面</param>
        private void onBeatmapChanged(ValueChangedEvent<WorkingBeatmap> v)
        {
            var beatmap = v.NewValue;

            updateTrackAdjustments();
            updateBackground(beatmap);

            activity.Value = new UserActivity.InLLin(beatmap.BeatmapInfo);
            onBeatmapChangedAction?.Invoke(beatmap);
        }

        #endregion

        #region 按键绑定、输入处理

        private readonly Dictionary<PluginKeybind, LLinPlugin> pluginKeyBindings = new Dictionary<PluginKeybind, LLinPlugin>();

        private readonly Dictionary<GlobalAction, Action> internalKeyBindings = new Dictionary<GlobalAction, Action>();

        public void RegisterPluginKeybind(LLinPlugin plugin, PluginKeybind keybind)
        {
            if (pluginKeyBindings.Any(b => (b.Value == plugin && b.Key.Key == keybind.Key)))
                throw new InvalidOperationException($"{plugin}已经注册过一个相同的{keybind}了");

            keybind.Id = pluginKeyBindings.Count + 1;

            pluginKeyBindings[keybind] = plugin;
        }

        public void UnRegisterPluginKeybind(LLinPlugin plugin, PluginKeybind? keybind = null)
        {
            //查找插件是pl的绑定
            if (keybind == null)
            {
                var bindings = pluginKeyBindings.Where(b => b.Value == plugin);

                foreach (var bind in bindings)
                    pluginKeyBindings.Remove(bind.Key);
            }
            else
                pluginKeyBindings.Remove(keybind);
        }

        private void initInternalKeyBindings()
        {
            internalKeyBindings[GlobalAction.MvisMusicPrev] = () => prevButton.Active();
            internalKeyBindings[GlobalAction.MvisMusicNext] = () => nextButton.Active();
            internalKeyBindings[GlobalAction.MvisOpenInSongSelect] = () => soloButton.Active();
            internalKeyBindings[GlobalAction.MvisToggleOverlayLock] = () => lockButton.Active();
            internalKeyBindings[GlobalAction.MvisTogglePluginPage] = () => pluginButton.Active();
            internalKeyBindings[GlobalAction.MvisTogglePause] = () => songProgressButton.Active();
            internalKeyBindings[GlobalAction.MvisToggleTrackLoop] = () => loopToggleButton.Active();
            internalKeyBindings[GlobalAction.MvisTogglePlayList] = () => sidebarToggleButton.Active();
            internalKeyBindings[GlobalAction.MvisForceLockOverlayChanges] = () => disableChangesButton.Active();
            internalKeyBindings[GlobalAction.Back] = () =>
            {
                if (sidebar.IsPresent && sidebar.State.Value == Visibility.Visible)
                {
                    sidebar.Hide();
                    return;
                }

                if (InterfacesHidden)
                {
                    lockButton.Bindable.Disabled = false;
                    lockButton.Bindable.Value = false;
                    makeActive(true);
                }
                else
                    this.Exit();
            };
        }

        public bool OnPressed(KeyBindingPressEvent<GlobalAction> action)
        {
            //查找本体按键绑定
            var target = internalKeyBindings.FirstOrDefault(b => b.Key == action.Action).Value;
            target?.Invoke();

            return target != null;
        }

        public void OnReleased(KeyBindingReleaseEvent<GlobalAction> e) { }

        protected override bool Handle(UIEvent e)
        {
            if (e is MouseMoveEvent)
                makeActive(false);

            return base.Handle(e);
        }

        protected override bool OnKeyDown(KeyDownEvent e)
        {
            //查找插件按键绑定并执行
            var target = pluginKeyBindings.FirstOrDefault(b => b.Key.Key == e.Key).Key;
            target?.Action?.Invoke();

            return target != null;
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            if (lockButton.Bindable.Value && InterfacesHidden && !lockButton.Bindable.Disabled)
                lockButton.Bindable.Toggle();

            makeActive(false);
            base.OnHoverLost(e);
        }

        #endregion

        #region 背景控制

        private readonly List<LLinPlugin> blackScreenPlugins = new List<LLinPlugin>();
        private readonly List<LLinPlugin> cleanScreenPlugins = new List<LLinPlugin>();

        private readonly BindableBool blackBackground = new BindableBool();

        private readonly BgTrianglesContainer backgroundTriangles = new BgTrianglesContainer();

        public bool RequestBlackBackground(LLinPlugin sender)
        {
            if (blackScreenPlugins.Contains(sender)) return true;

            blackScreenPlugins.Add(sender);
            blackBackground.Value = true;
            return true;
        }

        public bool RequestNonBlackBackground(LLinPlugin sender)
        {
            if (!blackScreenPlugins.Contains(sender)) return false;

            blackScreenPlugins.Remove(sender);
            blackBackground.Value = blackScreenPlugins.Count > 0;
            return true;
        }

        public bool RequestCleanBackground(LLinPlugin sender)
        {
            if (cleanScreenPlugins.Contains(sender)) return true;

            cleanScreenPlugins.Add(sender);
            backgroundTriangles.Hide();
            return true;
        }

        public bool RequestNonCleanBackground(LLinPlugin sender)
        {
            if (!cleanScreenPlugins.Contains(sender)) return false;

            cleanScreenPlugins.Remove(sender);
            if (cleanScreenPlugins.Count == 0) backgroundTriangles.Show();
            return true;
        }

        /// <summary>
        /// 旧方法，可能需要重做
        /// 根据指定的谱面更新背景
        /// </summary>
        /// <param name="beatmap">目标谱面</param>
        /// <param name="applyBgBrightness">是否同样应用亮度</param>
        private void updateBackground(WorkingBeatmap beatmap, bool applyBgBrightness = true)
        {
            ApplyToBackground(bsb =>
            {
                bsb.BlurAmount.Value = bgBlur.Value * 100;
                bsb.Beatmap = beatmap;
            });

            if (applyBgBrightness)
                applyBackgroundBrightness();
        }

        /// <summary>
        /// 旧方法，可能需要重做
        /// 将屏幕暗化应用到背景层
        /// </summary>
        /// <param name="auto">是否根据情况自动调整.</param>
        /// <param name="brightness">要调整的亮度.</param>
        private void applyBackgroundBrightness(bool auto = true, float brightness = 0)
        {
            if (!this.IsCurrentScreen()) return;

            ApplyToBackground(b =>
            {
                Color4 targetColor = auto
                    ? OsuColour.Gray(InterfacesHidden ? idleBgDim.Value : 0.6f)
                    : OsuColour.Gray(brightness);

                b.FadeColour(blackBackground.Value ? Color4.Black : targetColor, 300, Easing.OutQuint);
                backgroundLayer.FadeColour(targetColor, 300, Easing.OutQuint);
            });
        }

        #endregion

        #region Proxy

        private readonly Container proxyLayer;

        public void AddProxy(Drawable d)
        {
            if (!d.IsProxy) throw new InvalidOperationException("试图向Proxy层添加非Proxy Drawable");

            if (!proxyLayer.Contains(d))
                proxyLayer.Add(d);
        }

        public bool RemoveProxy(Drawable d)
        {
            if (proxyLayer.Contains(d))
            {
                proxyLayer.Remove(d, false);
                return true;
            }

            return false;
        }

        #endregion

        #region 插件加载、卸载

        private readonly List<LLinPlugin> loadingList = new List<LLinPlugin>();

        public bool UnmarkFromLoading(LLinPlugin pl)
        {
            if (!loadingList.Contains(pl) || !pluginManager.GetAllPlugins(false).Contains(pl)) return false;

            loadingList.Remove(pl);
            if (loadingList.Count == 0) loadingSpinner.Hide();
            return true;
        }

        public bool MarkAsLoading(LLinPlugin pl)
        {
            if (loadingList.Contains(pl) || !pluginManager.GetAllPlugins(false).Contains(pl)) return false;

            loadingList.Add(pl);
            loadingSpinner.Show();
            return true;
        }

        private void onPluginUnLoad(LLinPlugin pl)
        {
            UnRegisterPluginKeybind(pl); //移除快捷键

            //查找与pl对应的侧边栏页面
            foreach (var sc in sidebar.Components)
            {
                //如果找到的侧边栏的Plugin与pl匹配
                if (sc is PluginSidebarPage plsp && plsp.Plugin == pl)
                {
                    sidebar.Remove(plsp, true); //移除这个页面

                    //查找与plsp对应的底栏入口
                    foreach (var d in currentFunctionBar.GetAllPluginFunctionButton())
                    {
                        //同上
                        if (d is IPluginFunctionProvider btn && btn.SourcePage == plsp)
                        {
                            functionProviders.Remove(d);
                            currentFunctionBar.Remove(d);
                            break;
                        }
                    }
                }
            }

            if ((LLinPlugin)currentFunctionBar == pl)
                changeFunctionBarProvider(null);
        }

        #endregion

        private void checkIfPluginIllegal(LLinPlugin pl)
        {
            if (!pluginManager.GetAllPlugins(false).Contains(pl))
                throw new InvalidOperationException($"{pl} 不是正在运行中的插件, 请将此问题报告给插件开发者({pl.Author})。");
        }

        #region 对话框

        public void PushDialog(LLinPlugin sender, IconUsage icon, LocalisableString title, LocalisableString text, DialogOption[] options)
        {
            checkIfPluginIllegal(sender);

            dialog.Push(new LLinDialog(icon, title, text, options));
        }

        #endregion

        #region 通知

        public void PostNotification(LLinPlugin sender, IconUsage icon, LocalisableString message)
        {
            checkIfPluginIllegal(sender);
            notifications.Post(new SimpleNotification
            {
                Icon = icon,
                Text = message
            });
        }

        private uint lastID;
        private readonly IDictionary<uint, ProgressNotification> notificationDictionary = new ConcurrentDictionary<uint, ProgressNotification>();

        public (uint id, CancellationToken cancellationToken) PostProgressNotification(LLinPlugin sender, IconUsage icon, LocalisableString message, LocalisableString completionMessage)
        {
            checkIfPluginIllegal(sender);

            lastID++;
            uint id = lastID;

            var notification = new PluginProgressNotification
            {
                Text = message,
                CompletionText = completionMessage.ToString(),
                OnComplete = () => notificationDictionary.Remove(id)
            };

            notifications.Post(notification);

            if (!notificationDictionary.TryAdd(id, notification))
                throw new InvalidOperationException();

            return (id, notification.CancellationToken);
        }

        public bool UpdateProgressNotification(LLinPlugin sender, uint targetID, float progress)
        {
            checkIfPluginIllegal(sender);

            ProgressNotification? target;
            if (!notificationDictionary.TryGetValue(targetID, out target)) return false;

            if (target.State is ProgressNotificationState.Completed or ProgressNotificationState.Cancelled)
                return false;

            target.Progress = Math.Min(1, progress);

            return true;
        }

        public bool UpdateProgressNotification(LLinPlugin sender, uint targetID, ProgressState state)
        {
            checkIfPluginIllegal(sender);

            ProgressNotification? target;
            if (!notificationDictionary.TryGetValue(targetID, out target)) return false;

            if (target.State == ProgressNotificationState.Cancelled)
            {
                Logger.Log($"{sender} 的一项任务已经被您下达取消命令, 但似乎他们并没有这么做", level: LogLevel.Important);
                notificationDictionary.Remove(targetID);
            }

            switch (state)
            {
                case ProgressState.Failed:
                    target.State = ProgressNotificationState.Cancelled;
                    break;

                case ProgressState.Success:
                    target.State = ProgressNotificationState.Completed;
                    break;

                default:
                    target.State = ProgressNotificationState.Queued;
                    break;
            }

            return true;
        }

        #endregion

        #region 界面属性

        public override bool HideOverlaysOnEnter => true;
        public override bool AllowBackButton => false;

        public override bool CursorVisible => !InterfacesHidden
                                              || sidebar.State.Value == Visibility.Visible
                                              || tabControl.IsVisible.Value //TabControl可见
                                              || IsHovered == false; //隐藏界面或侧边栏可见，显示光标

        public override bool? AllowTrackAdjustments => true;

        #endregion

        private InputManager inputManager = null!;

        private readonly PlayerInfo info = new PlayerInfo
        {
            Name = "LLin",
            Version = 2,
            VendorName = "MATRIX-夜翎",
            SupportedFlags = PlayerFlags.All
        };

        public PlayerInfo GetInfo() => info;

        public bool InterfacesHidden { get; set; }

        public float BottomBarHeight => currentFunctionBar.GetSafeAreaPadding();

        private readonly Bindable<UserActivity> activity = new Bindable<UserActivity>();

        private readonly Container backgroundLayer;
        private readonly Container foregroundLayer;
        private readonly Container overlayLayer;

        private LoadingSpinner loadingSpinner = null!;

        private readonly ModNightcore<HitObject>.NightcoreBeatContainer nightcoreBeatContainer = new ModNightcore<HitObject>.NightcoreBeatContainer();

        #region 设置

        private readonly BindableFloat bgBlur = new BindableFloat();
        private readonly BindableFloat idleBgDim = new BindableFloat();
        private readonly BindableDouble musicSpeed = new BindableDouble();
        private readonly BindableBool adjustFreq = new BindableBool();
        private readonly BindableBool nightcoreBeat = new BindableBool();
        private readonly BindableBool allowProxy = new BindableBool();
        private readonly BindableBool autoVsync = new BindableBool();
        private Bindable<string> currentAudioControlProviderSetting = null!;
        private Bindable<string> currentFunctionbarSetting = null!;

        private FrameSync previousFrameSync;
        private readonly Bindable<FrameSync> frameSyncMode = new Bindable<FrameSync>();

        #endregion

        public LLinScreen()
        {
            Activity.BindTo(activity);

            InternalChildren = new Drawable[]
            {
                hashResolver,
                colourProvider,
                backgroundLayer = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Name = "背景层",
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre
                },
                foregroundLayer = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Name = "前景层",
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre
                },
                overlayLayer = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Depth = float.MinValue,
                    Name = "覆盖层"
                }
            };

            //对proxyLayer的处理交由allowProxy，因此不在这里添加
            proxyLayer = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Name = "Proxy 层"
            };
        }

        [BackgroundDependencyLoader]
        private void load(MConfigManager config, IdleTracker idleTracker, FrameworkConfigManager fcm)
        {
            inputManager = GetContainingInputManager();

            sidebar.Header = tabControl;
            var settingsPage = new PlayerSettings();
            var pluginsPage = new SidebarPluginsPage();

            sidebar.AddRange(new Drawable[] { settingsPage, pluginsPage });

            functionProviders.AddRange(new IFunctionProvider[]
            {
                new FakeButton
                {
                    Icon = FontAwesome.Solid.ArrowLeft,
                    Action = () =>
                    {
                        this.Exit();
                        return true;
                    },
                    Description = LLinBaseStrings.Exit,
                    Type = FunctionType.Base
                },
                prevButton = new FakeButton
                {
                    Size = new Vector2(50, 30),
                    Icon = FontAwesome.Solid.StepBackward,
                    Action = () => audioControlPlugin.PrevTrack(),
                    Description = LLinBaseStrings.PrevOrRestart,
                    Type = FunctionType.Audio
                },
                songProgressButton = new ToggleableFakeButton
                {
                    Description = LLinBaseStrings.TogglePause,
                    Action = () =>
                    {
                        audioControlPlugin.TogglePause();
                        OnTrackRunningToggle?.Invoke(CurrentTrack.IsRunning);

                        return true;
                    },
                    Type = FunctionType.ProgressDisplay
                },
                nextButton = new FakeButton
                {
                    Size = new Vector2(50, 30),
                    Icon = FontAwesome.Solid.StepForward,
                    Action = () => audioControlPlugin.NextTrack(),
                    Description = LLinBaseStrings.Next,
                    Type = FunctionType.Audio
                },
                pluginButton = new FakeButton
                {
                    Icon = FontAwesome.Solid.Plug,
                    Description = LLinBaseStrings.ViewPlugins,
                    Action = () =>
                    {
                        sidebar.ShowComponent(pluginsPage, true);
                        return true;
                    },
                    Type = FunctionType.Misc
                },
                disableChangesButton = new FakeButton
                {
                    Icon = FontAwesome.Solid.Desktop,
                    Action = () =>
                    {
                        //隐藏界面，锁定更改并隐藏锁定按钮
                        makeIdle(true);

                        //隐藏侧边栏
                        sidebar.ShowComponent(null);

                        lockButton.Bindable.Disabled = false;
                        lockButton.Bindable.Value = true;

                        //防止手机端无法恢复界面
                        lockButton.Bindable.Disabled = RuntimeInfo.IsDesktop;

                        currentFunctionBar.ShowFunctionControlTemporary();

                        return true;
                    },
                    Description = LLinBaseStrings.HideAndLockInterface,
                    Type = FunctionType.Misc
                },
                loopToggleButton = new ToggleableFakeButton
                {
                    Icon = FontAwesome.Solid.Undo,
                    Action = () =>
                    {
                        CurrentTrack.Looping = loopToggleButton.Bindable.Value;

                        return true;
                    },
                    Description = LLinBaseStrings.ToggleLoop,
                    Type = FunctionType.Misc
                },
                soloButton = new FakeButton
                {
                    Icon = FontAwesome.Solid.User,
                    Action = () =>
                    {
                        game?.PresentBeatmap(Beatmap.Value.BeatmapSetInfo);
                        return true;
                    },
                    Description = LLinBaseStrings.ViewInSongSelect,
                    Type = FunctionType.Misc
                },
                sidebarToggleButton = new FakeButton
                {
                    Icon = FontAwesome.Solid.List,
                    Action = () =>
                    {
                        sidebar.ShowComponent(settingsPage, true);
                        return true;
                    },
                    Description = LLinBaseStrings.OpenSidebar,
                    Type = FunctionType.Misc
                },
                lockButton = new ToggleableFakeButton
                {
                    Description = LLinBaseStrings.LockInterface,
                    Action = () =>
                    {
                        currentFunctionBar.ShowFunctionControlTemporary();

                        return true;
                    },
                    Type = FunctionType.Plugin,
                    Icon = FontAwesome.Solid.Lock
                }
            });

            overlayLayer.AddRange(new Drawable[]
            {
                nightcoreBeatContainer,
                sidebar,
                tabControl,
                loadingSpinner = new LoadingSpinner(true, true)
                {
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    Margin = new MarginPadding(115)
                }
            });

            backgroundLayer.Add(backgroundTriangles);

            //配置绑定/设置
            inputIdle.BindTo(idleTracker.IsIdle);
            config.BindWith(MSetting.MvisBgBlur, bgBlur);
            config.BindWith(MSetting.MvisIdleBgDim, idleBgDim);
            config.BindWith(MSetting.MvisMusicSpeed, musicSpeed);
            config.BindWith(MSetting.MvisAdjustMusicWithFreq, adjustFreq);
            config.BindWith(MSetting.MvisEnableNightcoreBeat, nightcoreBeat);
            config.BindWith(MSetting.MvisStoryboardProxy, allowProxy);
            config.BindWith(MSetting.MvisAutoVSync, autoVsync);
            currentAudioControlProviderSetting = config.GetBindable<string>(MSetting.MvisCurrentAudioProvider);
            currentFunctionbarSetting = config.GetBindable<string>(MSetting.MvisCurrentFunctionBar);

            fcm.BindWith(FrameworkSetting.FrameSync, frameSyncMode);

            //加载插件
            foreach (var pl in pluginManager.GetAllPlugins(true))
            {
                try
                {
                    //决定要把插件放在何处
                    switch (pl.Target)
                    {
                        case LLinPlugin.TargetLayer.Background:
                            backgroundLayer.Add(pl);
                            break;

                        case LLinPlugin.TargetLayer.Foreground:
                            foregroundLayer.Add(pl);
                            break;
                    }

                    var pluginSidebarPage = pl.CreateSidebarPage();

                    //如果插件有侧边栏页面
                    if (pluginSidebarPage != null)
                    {
                        sidebar.Add(pluginSidebarPage);
                        var btn = pluginSidebarPage.GetFunctionEntry();

                        //如果插件的侧边栏页面有入口按钮
                        if (btn != null)
                        {
                            btn.Action = () =>
                            {
                                sidebar.ShowComponent(pluginSidebarPage, true);
                                return true;
                            };
                            btn.Description += $" ({pluginSidebarPage.ShortcutKey})";

                            functionProviders.Add(btn);
                        }

                        //如果插件的侧边栏页面有调用快捷键
                        if (pluginSidebarPage.ShortcutKey != Key.Unknown)
                        {
                            RegisterPluginKeybind(pl, new PluginKeybind(pluginSidebarPage.ShortcutKey, () =>
                            {
                                if (!pl.Disabled.Value) btn?.Active();
                            }));
                        }
                    }
                }
                catch (Exception e)
                {
                    Logger.Error(e, $"在添加 {pl.Name} 时出现问题, 请联系你的插件提供方: {e.Message}");
                }
            }
        }

        private readonly SimpleEntry dbusEntry = new SimpleEntry
        {
            Label = "LLin - 插件",
            Enabled = false
        };

        protected override void LoadComplete()
        {
            bgBlur.BindValueChanged(v => updateBackground(Beatmap.Value));
            idleBgDim.BindValueChanged(v => applyBackgroundBrightness(true, v.NewValue));
            musicSpeed.BindValueChanged(_ => updateTrackAdjustments());
            adjustFreq.BindValueChanged(_ => updateTrackAdjustments());
            nightcoreBeat.BindValueChanged(v =>
            {
                if (v.NewValue)
                    nightcoreBeatContainer.Show();
                else
                    nightcoreBeatContainer.Hide();
            }, true);

            inputIdle.BindValueChanged(v =>
            {
                if (v.NewValue) makeIdle(false);
            });

            allowProxy.BindValueChanged(v =>
            {
                //如果允许proxy显示
                if (v.NewValue)
                {
                    backgroundLayer.Remove(proxyLayer, false);
                    AddInternal(proxyLayer);
                }
                else
                {
                    RemoveInternal(proxyLayer, false);
                    backgroundLayer.Add(proxyLayer);
                }
            }, true);

            //VSync
            previousFrameSync = frameSyncMode.Value;

            if (autoVsync.Value)
                frameSyncMode.Value = FrameSync.VSync;

            autoVsync.BindValueChanged(v =>
            {
                frameSyncMode.Value = v.NewValue ? FrameSync.VSync : previousFrameSync;
            });

            //设置键位
            initInternalKeyBindings();

            //添加DBusEntry
            pluginManager.AddDBusMenuEntry(dbusEntry);

            //当插件卸载时调用onPluginUnload
            pluginManager.OnPluginUnLoad += onPluginUnLoad;

            //添加选歌入口
            sidebar.Add(new SongSelectPage
            {
                Action = () => this.Push(new LLinSongSelect())
            });

            //更新当前音乐控制插件
            currentAudioControlProviderSetting.BindValueChanged(v =>
            {
                //获取与新值匹配的控制插件
                changeAudioControlProvider(pluginManager.GetAudioControlByPath(v.NewValue));
            }, true);

            //更新当前功能条
            currentFunctionbarSetting.BindValueChanged(v =>
            {
                //获取与新值匹配的控制插件
                changeFunctionBarProvider(pluginManager.GetFunctionBarProviderByPath(v.NewValue));
            }, true);

            blackBackground.BindValueChanged(_ => applyBackgroundBrightness());

            base.LoadComplete();
        }

        protected override void Update()
        {
            songProgressButton.Bindable.Value = CurrentTrack.IsRunning;
            base.Update();
        }

        private IReadOnlyList<Mod>? lastScreenMods;

        public override void OnEntering(ScreenTransitionEvent e)
        {
            base.OnEntering(e);

            //保存上个屏幕的Mods
            lastScreenMods = Mods.Value;

            //覆盖mod列表
            Mods.Value = new List<Mod> { modRateAdjust };

            //动画
            backgroundLayer.FadeOut().Then().Delay(250).FadeIn(500);
            foregroundLayer.ScaleTo(0f).Then().ScaleTo(1f, 300, Easing.OutQuint);

            //触发一次onBeatmapChanged和onTrackRunningToggle
            Beatmap.BindValueChanged(onBeatmapChanged, true);
            OnTrackRunningToggle?.Invoke(CurrentTrack.IsRunning);
        }

        public override bool OnExiting(ScreenExitEvent e)
        {
            //重置Track
            CurrentTrack.ResetSpeedAdjustments();
            CurrentTrack.Looping = false;
            Beatmap.Disabled = false;

            //恢复mods
            Mods.Value = lastScreenMods;

            //锁定变更
            lockButton.Bindable.Disabled = true;

            //非背景层的动画
            foregroundLayer.ScaleTo(0, 300, Easing.OutQuint);
            currentFunctionBar.Hide();

            this.FadeOut(500, Easing.OutQuint);

            Exiting?.Invoke();

            pluginManager.OnPluginUnLoad -= onPluginUnLoad;

            pluginManager.RemoveDBusMenuEntry(dbusEntry);

            if (autoVsync.Value)
            {
                frameSyncMode.Value = previousFrameSync;
            }

            return base.OnExiting(e);
        }

        private WorkingBeatmap? suspendBeatmap;

        public override void OnSuspending(ScreenTransitionEvent e)
        {
            CurrentTrack.ResetSpeedAdjustments();
            Beatmap.Disabled = false;
            suspendBeatmap = Beatmap.Value;

            //恢复mods
            Mods.Value = lastScreenMods;

            //背景层的动画
            applyBackgroundBrightness(false, 1);

            this.FadeOut(300 * 0.6f, Easing.OutQuint)
                .ScaleTo(1.2f, 300 * 0.6f, Easing.OutQuint);

            Beatmap.UnbindEvents();
            Suspending?.Invoke();

            base.OnSuspending(e);
        }

        public override void OnResuming(ScreenTransitionEvent e)
        {
            base.OnResuming(e);

            //更新Mod
            lastScreenMods = ((OsuScreen)e.Last).Mods.Value;

            Mods.Value = new List<Mod> { modRateAdjust };

            Beatmap.Disabled = audioControlPlugin != null && audioControlPlugin != pluginManager.DefaultAudioController;
            this.FadeIn(300 * 0.6f)
                .ScaleTo(1, 300 * 0.6f, Easing.OutQuint);

            updateTrackAdjustments();

            Beatmap.BindValueChanged(onBeatmapChanged);
            if (Beatmap.Value != suspendBeatmap) Beatmap.TriggerChange();
            else updateBackground(Beatmap.Value);

            //背景层的动画
            backgroundLayer.FadeOut().Then().Delay(300 * 0.6f).FadeIn(150);
            Resuming?.Invoke();
        }

        #region 激活、闲置界面

        private readonly IBindable<bool> inputIdle = new Bindable<bool>();

        private bool okForHide => IsHovered
                                  && inputIdle.Value
                                  && currentFunctionBar.OkForHide()
                                  && !lockButton.Bindable.Value
                                  && !lockButton.Bindable.Disabled
                                  && inputManager?.DraggedDrawable == null
                                  && inputManager?.FocusedDrawable == null;

        private void makeIdle(bool forceIdle)
        {
            if (!forceIdle && !okForHide)
                return;

            applyBackgroundBrightness(true, idleBgDim.Value);
            InterfacesHidden = true;
            OnIdle?.Invoke();
        }

        private void makeActive(bool forceActive)
        {
            currentFunctionBar.ShowFunctionControlTemporary();

            //如果界面已隐藏、不是强制显示并且已经锁定变更
            if (!forceActive && lockButton.Bindable.Value && InterfacesHidden) return;

            currentFunctionBar.Show();
            applyBackgroundBrightness();

            InterfacesHidden = false;
            OnActive?.Invoke();
        }

        #endregion

        #region 侧边栏

        private readonly Sidebar sidebar = new Sidebar();

        private readonly TabControl tabControl = new TabControl();

        #endregion

        #region 功能条

        private readonly List<IFunctionProvider> functionProviders = new List<IFunctionProvider>();

        private FakeButton soloButton = null!;
        private FakeButton prevButton = null!;
        private FakeButton nextButton = null!;
        private FakeButton pluginButton = null!;
        private FakeButton disableChangesButton = null!;
        private FakeButton sidebarToggleButton = null!;

        private ToggleableFakeButton loopToggleButton = null!;
        private ToggleableFakeButton lockButton = null!;
        private ToggleableFakeButton songProgressButton = null!;

        private readonly FunctionBar fallbackFunctionbar = new FunctionBar();

        private IFunctionBarProvider? userFunctionBar;

        private IFunctionBarProvider currentFunctionBar
        {
            get => userFunctionBar ?? fallbackFunctionbar;
            set => userFunctionBar = value;
        }

        private void changeFunctionBarProvider(IFunctionBarProvider? target)
        {
            //找到旧的Functionbar
            var targetDrawable = overlayLayer.FirstOrDefault(d => d == currentFunctionBar);

            //移除
            if (targetDrawable != null)
                overlayLayer.Remove(targetDrawable, false);

            //不要在此功能条禁用时再调用onFunctionBarPluginDisable
            currentFunctionBar.OnDisable -= onFunctionBarDisable;

            //如果新的目标是null，则使用后备功能条
            var newProvider = target ?? fallbackFunctionbar;

            //更新控制按钮
            newProvider.SetFunctionControls(functionProviders);
            newProvider.OnDisable += onFunctionBarDisable;

            //更新currentFunctionBarProvider
            currentFunctionBar = newProvider;

            //添加新的功能条
            overlayLayer.Add((Drawable)newProvider);
            //Logger.Log($"更改底栏到{newProvider}");
        }

        private void onFunctionBarDisable() => changeFunctionBarProvider(null);

        #endregion
    }
}
