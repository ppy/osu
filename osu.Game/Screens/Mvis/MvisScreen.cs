using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using JetBrains.Annotations;
using M.Resources.Localisation.Mvis;
using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Bindables;
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
using osu.Game.Overlays.Settings;
using osu.Game.Rulesets.Mods;
using osu.Game.Screens.Mvis.Misc;
using osu.Game.Screens.Mvis.Plugins;
using osu.Game.Screens.Mvis.Plugins.Internal.FallbackFunctionBar;
using osu.Game.Screens.Mvis.Plugins.Types;
using osu.Game.Screens.Mvis.SideBar;
using osu.Game.Screens.Mvis.SideBar.Settings;
using osu.Game.Screens.Mvis.SideBar.Tabs;
using osu.Game.Screens.Play;
using osu.Game.Screens.Select;
using osu.Game.Users;
using osuTK;
using osuTK.Graphics;
using osuTK.Input;
using Sidebar = osu.Game.Screens.Mvis.SideBar.Sidebar;

namespace osu.Game.Screens.Mvis
{
    //todo: 重写界面?
    public class MvisScreen : ScreenWithBeatmapBackground, IKeyBindingHandler<GlobalAction>
    {
        public override bool HideOverlaysOnEnter => true;
        public override bool AllowBackButton => false;

        public override bool CursorVisible => !OverlaysHidden
                                              || sidebar.State.Value == Visibility.Visible
                                              || tabHeader.IsVisible.Value //TabHeader可见
                                              || IsHovered == false; //隐藏界面或侧边栏可见，显示光标

        public override bool AllowTrackAdjustments => true;

        private bool okForHide => IsHovered
                                  && isIdle.Value
                                  && !currentFunctionBarProvider.OkForHide()
                                  && !(lockButton?.Bindable.Value ?? false)
                                  && !(lockButton?.Bindable.Disabled ?? false)
                                  && inputManager?.DraggedDrawable == null
                                  && inputManager?.FocusedDrawable == null;

        #region 外部事件

        /// <summary>
        /// 切换暂停时调用。<br/><br/>
        /// 传递: 当前音乐是否暂停<br/>
        /// true: 暂停<br/>
        /// false: 播放<br/>
        /// </summary>
        public Action<bool> OnTrackRunningToggle;

        /// <summary>
        /// 播放器屏幕退出时调用
        /// </summary>
        public Action OnScreenExiting;

        /// <summary>
        /// 播放器屏幕进入后台时调用
        /// </summary>
        public Action OnScreenSuspending;

        /// <summary>
        /// 播放器屏幕进入前台时调用
        /// </summary>
        public Action OnScreenResuming;

        /// <summary>
        /// 进入空闲状态(长时间没有输入)时调用
        /// </summary>
        public Action OnIdle;

        /// <summary>
        /// 从空闲状态退出时调用
        /// </summary>
        public Action OnResumeFromIdle;

        /// <summary>
        /// 谱面变更时调用<br/><br/>
        /// 传递: 当前谱面(WorkingBeatmap)<br/>
        /// </summary>
        private Action<WorkingBeatmap> onBeatmapChangedAction;

        public void OnBeatmapChanged(Action<WorkingBeatmap> action, Drawable sender, bool runOnce = false)
        {
            bool alreadyRegistered = onBeatmapChangedAction?.GetInvocationList().ToList().Contains(action) ?? false;

            if (sender.GetType().IsSubclassOf(typeof(MvisPlugin))
                && pluginManager.GetAllPlugins(false).Contains((MvisPlugin)sender)
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

        private readonly Dictionary<PluginKeybind, MvisPlugin> pluginKeyBindings = new Dictionary<PluginKeybind, MvisPlugin>();

        public void RegisterKeybind(MvisPlugin plugin, PluginKeybind keybind)
        {
            if (pluginKeyBindings.Any(b => (b.Value == plugin && b.Key.Key == keybind.Key)))
                throw new InvalidOperationException($"{plugin}已经注册过一个相同的{keybind}了");

            keybind.Id = pluginKeyBindings.Count + 1;

            pluginKeyBindings[keybind] = plugin;
        }

        private void unBindFor(MvisPlugin pl)
        {
            //查找插件是pl的绑定
            var bindings = pluginKeyBindings.Where(b => b.Value == pl);

            foreach (var bind in bindings)
                pluginKeyBindings.Remove(bind.Key);
        }

        /// <summary>
        /// 拖动下方进度条时调用<br/><br/>
        /// 传递: 拖动的目标时间
        /// </summary>
        public Action<double> OnSeek;

        #endregion

        #region 依赖

        [Resolved(CanBeNull = true)]
        private OsuGame game { get; set; }

        [Resolved]
        private MvisPluginManager pluginManager { get; set; }

        private CustomColourProvider colourProvider;
        private DependencyContainer dependencies;

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent) =>
            dependencies = new DependencyContainer(base.CreateChildDependencies(parent));

        #endregion

        #region 底栏

        [CanBeNull]
        private IFunctionBarProvider realFunctionBarProvider;

        [NotNull]
        private IFunctionBarProvider currentFunctionBarProvider
        {
            get => realFunctionBarProvider ?? fallbackFunctionBar;
            set => realFunctionBarProvider = value;
        }

        private readonly FunctionBar fallbackFunctionBar = new FunctionBar();

        private readonly List<IFunctionProvider> functionProviders = new List<IFunctionProvider>();

        //留着这些能让播放器在触发GlobalAction时会有更好的界面体验
        private FakeButton soloButton;
        private FakeButton prevButton;
        private FakeButton nextButton;
        private FakeButton pluginButton;
        private FakeButton disableChangesButton;
        private FakeButton sidebarToggleButton;

        private ToggleableFakeButton loopToggleButton;
        private ToggleableFakeButton lockButton;
        private ToggleableFakeButton songProgressButton;

        #endregion

        #region 背景和前景

        private InputManager inputManager { get; set; }
        private NightcoreBeatContainer nightcoreBeatContainer;

        private Container background;
        private BgTrianglesContainer bgTriangles;

        private Container foreground;

        private Container overlay;

        #endregion

        #region overlay

        private LoadingSpinner loadingSpinner;

        private readonly Sidebar sidebar = new Sidebar();

        private readonly TabControl tabHeader = new TabControl();

        #endregion

        #region 设置

        private readonly BindableBool trackRunning = new BindableBool();
        private readonly BindableFloat bgBlur = new BindableFloat();
        private readonly BindableFloat idleBgDim = new BindableFloat();
        private readonly BindableDouble musicSpeed = new BindableDouble();
        private readonly BindableBool adjustFreq = new BindableBool();
        private readonly BindableBool nightcoreBeat = new BindableBool();
        private readonly BindableBool allowProxy = new BindableBool();
        private Bindable<string> currentAudioControlProviderSetting;
        private Bindable<string> currentFunctionbarSetting;

        #endregion

        #region proxy

        private readonly Container proxyLayer = new Container
        {
            RelativeSizeAxes = Axes.Both,
            Name = "Proxy Layer",
            Depth = -1
        };

        /// <summary>
        /// 添加一个Drawable到Proxy层
        /// </summary>
        /// <param name="d">要添加的Drawable</param>
        public void AddDrawableToProxy(Drawable d) => proxyLayer.Add(d);

        /// <summary>
        /// 从Proxy层移除一个Drawablw
        /// </summary>
        /// <param name="d">要移除的Drawable</param>
        /// <returns>
        /// true: 移除成功<br/>
        /// false: 移除出现异常</returns>
        public bool RemoveDrawableFromProxy(Drawable d)
        {
            try
            {
                proxyLayer.Remove(d);
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        #endregion

        #region 杂项

        private const float duration = 500;

        private readonly MvisModRateAdjust modRateAdjust = new MvisModRateAdjust();
        private IReadOnlyList<Mod> originalMods;
        private List<Mod> timeRateMod;

        private readonly Dictionary<GlobalAction, Action> keyBindings = new Dictionary<GlobalAction, Action>();

        public bool OverlaysHidden { get; private set; }
        private readonly IBindable<bool> isIdle = new BindableBool();

        private readonly Bindable<UserActivity> activity = new Bindable<UserActivity>();

        public DrawableTrack CurrentTrack => audioControlProvider.GetCurrentTrack();

        private readonly BindableList<MvisPlugin> loadList = new BindableList<MvisPlugin>();

        public Bindable<bool> HideTriangles = new Bindable<bool>();
        public Bindable<bool> HideScreenBackground = new Bindable<bool>();

        private IProvideAudioControlPlugin audioControlProvider;
        private SettingsButton songSelectButton;
        private PlayerSettings settingsScroll;

        public float BottombarHeight => currentFunctionBarProvider.GetSafeAreaPadding();

        #endregion

        public MvisScreen()
        {
            Padding = new MarginPadding { Horizontal = -HORIZONTAL_OVERFLOW_PADDING };

            Activity.BindTo(activity);
        }

        [BackgroundDependencyLoader]
        private void load(MConfigManager config, IdleTracker idleTracker)
        {
            //早期设置
            var iR = config.Get<float>(MSetting.MvisInterfaceRed);
            var iG = config.Get<float>(MSetting.MvisInterfaceGreen);
            var iB = config.Get<float>(MSetting.MvisInterfaceBlue);
            dependencies.Cache(colourProvider = new CustomColourProvider(iR, iG, iB));
            dependencies.Cache(this);

            //向侧边栏添加内容
            SidebarPluginsPage pluginsPage;
            sidebar.Header = tabHeader;

            sidebar.AddRange(new Drawable[]
            {
                settingsScroll = new PlayerSettings(),
                pluginsPage = new SidebarPluginsPage()
            });

            songSelectButton = new SettingsButton
            {
                Text = "歌曲选择",
                Action = () => this.Push(new MvisSongSelect())
            };

            //配置绑定/设置
            isIdle.BindTo(idleTracker.IsIdle);
            config.BindWith(MSetting.MvisBgBlur, bgBlur);
            config.BindWith(MSetting.MvisIdleBgDim, idleBgDim);
            config.BindWith(MSetting.MvisMusicSpeed, musicSpeed);
            config.BindWith(MSetting.MvisAdjustMusicWithFreq, adjustFreq);
            config.BindWith(MSetting.MvisEnableNightcoreBeat, nightcoreBeat);
            config.BindWith(MSetting.MvisStoryboardProxy, allowProxy);
            currentAudioControlProviderSetting = config.GetBindable<string>(MSetting.MvisCurrentAudioProvider);
            currentFunctionbarSetting = config.GetBindable<string>(MSetting.MvisCurrentFunctionBar);

            InternalChildren = new Drawable[]
            {
                colourProvider,
                nightcoreBeatContainer = new NightcoreBeatContainer
                {
                    Alpha = 0
                },
                bgTriangles = new BgTrianglesContainer(),
                background = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding { Horizontal = HORIZONTAL_OVERFLOW_PADDING },
                    Name = "Background Layer"
                },
                foreground = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding { Horizontal = HORIZONTAL_OVERFLOW_PADDING },
                    Name = "Foreground Layer",
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre
                },
                overlay = new Container
                {
                    Name = "Overlay Layer",
                    RelativeSizeAxes = Axes.Both,
                    Depth = float.MinValue,
                    Padding = new MarginPadding { Horizontal = HORIZONTAL_OVERFLOW_PADDING },
                    Children = new Drawable[]
                    {
                        sidebar,
                        tabHeader,
                        loadingSpinner = new LoadingSpinner(true, true)
                        {
                            Anchor = Anchor.BottomCentre,
                            Origin = Anchor.BottomCentre,
                            Margin = new MarginPadding(115)
                        }
                    }
                }
            };

            //todo: 找出为啥audioControlProvider会在被赋值前访问
            audioControlProvider = pluginManager.DefaultAudioController;

            functionProviders.AddRange(new IFunctionProvider[]
            {
                new FakeButton
                {
                    Icon = FontAwesome.Solid.ArrowLeft,
                    Action = this.Exit,
                    Description = MvisBaseStrings.Exit,
                    Type = FunctionType.Base
                },
                new FakeButton
                {
                    Icon = FontAwesome.Regular.QuestionCircle,
                    Action = () => game?.OpenUrlExternally("https://matrix-feather.github.io/mfosu/mfosu_mp_manual/"),
                    Description = MvisBaseStrings.Manual,
                    Type = FunctionType.Base
                },
                prevButton = new FakeButton
                {
                    Size = new Vector2(50, 30),
                    Icon = FontAwesome.Solid.StepBackward,
                    Action = prevTrack,
                    Description = MvisBaseStrings.PrevOrRestart,
                    Type = FunctionType.Audio
                },
                songProgressButton = new ToggleableFakeButton
                {
                    Description = MvisBaseStrings.TogglePause,
                    Action = togglePause,
                    Type = FunctionType.ProgressDisplay
                },
                nextButton = new FakeButton
                {
                    Size = new Vector2(50, 30),
                    Icon = FontAwesome.Solid.StepForward,
                    Action = nextTrack,
                    Description = MvisBaseStrings.Next,
                    Type = FunctionType.Audio,
                },
                pluginButton = new FakeButton
                {
                    Icon = FontAwesome.Solid.Plug,
                    Description = MvisBaseStrings.ViewPlugins,
                    Action = () => updateSidebarState(pluginsPage),
                    Type = FunctionType.Misc
                },
                disableChangesButton = new FakeButton
                {
                    Icon = FontAwesome.Solid.Desktop,
                    Action = () =>
                    {
                        bool disabledBefore = lockButton.Bindable.Disabled;
                        lockButton.Bindable.Disabled = false;

                        //隐藏界面，锁定更改并隐藏锁定按钮
                        hideOverlays(true);

                        updateSidebarState(null);

                        //防止手机端无法恢复界面
                        lockButton.Bindable.Value = RuntimeInfo.IsDesktop;
                        lockButton.Bindable.Disabled = RuntimeInfo.IsDesktop && !disabledBefore;
                    },
                    Description = MvisBaseStrings.HideAndLockInterface,
                    Type = FunctionType.Misc
                },
                loopToggleButton = new ToggleableFakeButton
                {
                    Icon = FontAwesome.Solid.Undo,
                    Action = () => CurrentTrack.Looping = loopToggleButton.Bindable.Value,
                    Description = MvisBaseStrings.ToggleLoop,
                    Type = FunctionType.Misc
                },
                soloButton = new FakeButton
                {
                    Icon = FontAwesome.Solid.User,
                    Action = presentBeatmap,
                    Description = MvisBaseStrings.ViewInSongSelect,
                    Type = FunctionType.Misc
                },
                sidebarToggleButton = new FakeButton
                {
                    Icon = FontAwesome.Solid.List,
                    Action = () => updateSidebarState(settingsScroll),
                    Description = MvisBaseStrings.OpenSidebar,
                    Type = FunctionType.Misc
                },
                lockButton = new ToggleableFakeButton
                {
                    Description = MvisBaseStrings.LockInterface,
                    Action = () =>
                    {
                        showPluginEntriesTemporary();
                        lockButton.Active();
                    },
                    Type = FunctionType.Plugin,
                    Icon = FontAwesome.Solid.Lock
                }
            });
        }

        protected override void LoadComplete()
        {
            //各种BindValueChanged
            //这部分放load会导致当前屏幕为主界面时，播放器会在后台相应设置变动
            loadList.BindCollectionChanged(onLoadListChanged);

            bgBlur.BindValueChanged(v => updateBackground(Beatmap.Value));
            idleBgDim.BindValueChanged(_ => updateIdleVisuals());
            musicSpeed.BindValueChanged(_ => applyTrackAdjustments());
            adjustFreq.BindValueChanged(_ => applyTrackAdjustments());
            nightcoreBeat.BindValueChanged(v =>
            {
                if (v.NewValue)
                    nightcoreBeatContainer.Show();
                else
                    nightcoreBeatContainer.Hide();
            });

            isIdle.BindValueChanged(v =>
            {
                if (v.NewValue) hideOverlays(false);
            });

            inputManager = GetContainingInputManager();

            songProgressButton.Bindable.BindTo(trackRunning);

            allowProxy.BindValueChanged(v =>
            {
                //如果允许proxy显示
                if (v.NewValue)
                {
                    background.Remove(proxyLayer);
                    AddInternal(proxyLayer);
                }
                else
                {
                    RemoveInternal(proxyLayer);
                    background.Add(proxyLayer);
                }
            }, true);

            HideTriangles.BindValueChanged(v =>
            {
                switch (v.NewValue)
                {
                    case true:
                        bgTriangles.Hide();
                        break;

                    case false:
                        bgTriangles.Show();
                        break;
                }
            }, true);

            HideScreenBackground.BindValueChanged(_ => applyBackgroundBrightness());

            //设置键位
            setupKeyBindings();

            //当插件卸载时调用onPluginUnload
            pluginManager.OnPluginUnLoad += onPluginUnLoad;

            //添加插件
            foreach (var pl in pluginManager.GetAllPlugins(true))
            {
                try
                {
                    //决定要把插件放在何处
                    switch (pl.Target)
                    {
                        case MvisPlugin.TargetLayer.Background:
                            background.Add(pl);
                            break;

                        case MvisPlugin.TargetLayer.Foreground:
                            foreground.Add(pl);
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
                            btn.Action = () => updateSidebarState(pluginSidebarPage);
                            btn.Description += $" ({pluginSidebarPage.ShortcutKey})";

                            functionProviders.Add(btn);
                        }

                        //如果插件的侧边栏页面有调用快捷键
                        if (pluginSidebarPage.ShortcutKey != Key.Unknown)
                        {
                            RegisterKeybind(pl, new PluginKeybind(pluginSidebarPage.ShortcutKey, () =>
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

            //添加选歌入口
            sidebar.Add(new SongSelectPage
            {
                Action = () => this.Push(new MvisSongSelect())
            });

            //更新当前音乐控制插件
            currentAudioControlProviderSetting.BindValueChanged(v =>
            {
                //获取与新值匹配的控制插件
                var pl = pluginManager.GetAudioControlByPath(v.NewValue);
                changeAudioControlProvider(pl);
            }, true);

            //更新当前功能条
            currentFunctionbarSetting.BindValueChanged(v =>
            {
                //获取与新值匹配的控制插件
                var pl = pluginManager.GetFunctionBarProviderByPath(v.NewValue);
                changeFunctionBarProvider(pl);
            }, true);

            currentFunctionBarProvider.Hide();
            base.LoadComplete();
        }

        private void changeAudioControlProvider(IProvideAudioControlPlugin pacp)
        {
            //如果没找到(为null)，则解锁Beatmap.Disabled
            Beatmap.Disabled = (pacp != null) && (pacp != pluginManager.DefaultAudioController);

            //设置当前控制插件IsCurrent为false
            audioControlProvider.IsCurrent = false;

            //切换并设置当前控制插件IsCurrent为true
            audioControlProvider = pacp ?? pluginManager.DefaultAudioController;
            audioControlProvider.IsCurrent = true;

            songSelectButton.Enabled.Value = audioControlProvider == pluginManager.DefaultAudioController;
            //Logger.Log($"更改控制插件到{audioControlProvider}");
        }

        private void onFunctionBarPluginDisable() => changeFunctionBarProvider(null);

        private void changeFunctionBarProvider(IFunctionBarProvider target)
        {
            //找到旧的Functionbar
            var targetDrawable = overlay.FirstOrDefault(d => d is IFunctionBarProvider);

            //移除
            if (targetDrawable != null)
                overlay.Remove(targetDrawable);

            //不要在此功能条禁用时再调用onFunctionBarPluginDisable
            currentFunctionBarProvider.OnDisable -= onFunctionBarPluginDisable;

            //如果新的目标是null，则使用后备功能条
            var newProvider = target ?? fallbackFunctionBar;

            //更新控制按钮
            newProvider.SetFunctionControls(functionProviders);
            newProvider.OnDisable += onFunctionBarPluginDisable;

            //更新currentFunctionBarProvider
            currentFunctionBarProvider = newProvider;

            //添加新的功能条
            overlay.Add((Drawable)currentFunctionBarProvider);
            //Logger.Log($"更改底栏到{currentFunctionBarProvider}");
        }

        private void setupKeyBindings()
        {
            keyBindings[GlobalAction.MvisMusicPrev] = () => prevButton.Active();
            keyBindings[GlobalAction.MvisMusicNext] = () => nextButton.Active();
            keyBindings[GlobalAction.MvisOpenInSongSelect] = () => soloButton.Active();
            keyBindings[GlobalAction.MvisToggleOverlayLock] = () => lockButton.Active(true);
            keyBindings[GlobalAction.MvisTogglePluginPage] = () => pluginButton.Active();
            keyBindings[GlobalAction.MvisTogglePause] = () => songProgressButton.Active(true);
            keyBindings[GlobalAction.MvisToggleTrackLoop] = () => loopToggleButton.Active();
            keyBindings[GlobalAction.MvisTogglePlayList] = () => sidebarToggleButton.Active();
            keyBindings[GlobalAction.MvisForceLockOverlayChanges] = () => disableChangesButton.Active();
            keyBindings[GlobalAction.Back] = () =>
            {
                if (sidebar.IsPresent && sidebar.State.Value == Visibility.Visible)
                {
                    sidebar.Hide();
                    return;
                }

                if (OverlaysHidden)
                {
                    lockButton.Bindable.Disabled = false;
                    lockButton.Bindable.Value = false;
                    showOverlays(true);
                }
                else
                    this.Exit();
            };
        }

        private void onPluginUnLoad(MvisPlugin pl)
        {
            unBindFor(pl); //移除快捷键

            //查找与pl对应的侧边栏页面
            foreach (var sc in sidebar.Components)
            {
                //如果找到的侧边栏的Plugin与pl匹配
                if (sc is PluginSidebarPage plsp && plsp.Plugin == pl)
                {
                    sidebar.Remove(plsp); //移除这个页面

                    //查找与plsp对应的底栏入口
                    foreach (var d in currentFunctionBarProvider.GetAllPluginFunctionButton())
                    {
                        //同上
                        if (d is IPluginFunctionProvider btn && btn.SourcePage == plsp)
                        {
                            functionProviders.Remove(d);
                            currentFunctionBarProvider.Remove(d);
                            break;
                        }
                    }
                }
            }

            if ((MvisPlugin)currentFunctionBarProvider == pl)
                changeFunctionBarProvider(null);
        }

        internal bool RemovePluginFromLoadList(MvisPlugin pl)
        {
            if (!loadList.Contains(pl)) return false;

            loadList.Remove(pl);
            return true;
        }

        internal bool AddPluginToLoadList(MvisPlugin pl)
        {
            if (loadList.Contains(pl)) return false;

            loadList.Add(pl);
            return true;
        }

        [Resolved]
        private DialogOverlay dialog { get; set; }

        public void RequestAudioControl(IProvideAudioControlPlugin pacp, LocalisableString message, Action onDeny, Action onAllow)
        {
            if (!(pacp is MvisPlugin mpl)) return;

            dialog.Push(new ConfirmDialog(
                mpl.ToString()
                + MvisBaseStrings.AudioControlRequestedMain
                + "\n"
                + MvisBaseStrings.AudioControlRequestedSub(message.ToString()),
                () =>
                {
                    changeAudioControlProvider(pacp);
                    onAllow?.Invoke();
                },
                onDeny));
        }

        public void ReleaseAudioControlFrom(IProvideAudioControlPlugin pacp)
        {
            if (audioControlProvider == pacp)
                changeAudioControlProvider(null);
        }

        private void onLoadListChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (sender is BindableList<MvisPlugin> list)
            {
                if (list.Count > 0)
                    Schedule(loadingSpinner.Show);
                else
                    Schedule(loadingSpinner.Hide);
            }
        }

        private void updateSidebarState(Drawable d)
        {
            if (d == null) sidebar.Hide(); //如果d是null, 则隐藏侧边栏
            if (!(d is ISidebarContent)) return; //如果d不是ISidebarContent, 则忽略这次调用

            sidebar.ShowComponent(d, true);
        }

        #region override事件

        protected override void Update()
        {
            base.Update();

            trackRunning.Value = CurrentTrack.IsRunning;
        }

        public override void OnEntering(IScreen last)
        {
            base.OnEntering(last);

            originalMods = Mods.Value;

            //override mod列表
            timeRateMod = new List<Mod> { modRateAdjust };
            Mods.Value = timeRateMod;

            //各种背景层的动画
            background.FadeOut().Then().Delay(250).FadeIn(500);

            //非背景层的动画
            foreground.ScaleTo(0f).Then().ScaleTo(1f, duration, Easing.OutQuint);

            //触发一次onBeatmapChanged和onTrackRunningToggle
            Beatmap.BindValueChanged(onBeatmapChanged, true);
            OnTrackRunningToggle?.Invoke(CurrentTrack.IsRunning);
            showOverlays(true);
        }

        public override bool OnExiting(IScreen next)
        {
            //重置Track
            CurrentTrack.ResetSpeedAdjustments();
            CurrentTrack.Looping = false;
            Beatmap.Disabled = false;

            //恢复mods
            Mods.Value = originalMods;

            //锁定变更
            lockButton.Bindable.Disabled = true;

            //非背景层的动画
            foreground.ScaleTo(0, duration, Easing.OutQuint);
            currentFunctionBarProvider.Hide();

            this.FadeOut(500, Easing.OutQuint);

            OnScreenExiting?.Invoke();
            pluginManager.OnPluginUnLoad -= onPluginUnLoad;

            return base.OnExiting(next);
        }

        private WorkingBeatmap suspendBeatmap;

        public override void OnSuspending(IScreen next)
        {
            CurrentTrack.ResetSpeedAdjustments();
            Beatmap.Disabled = false;
            suspendBeatmap = Beatmap.Value;

            //恢复mods
            Mods.Value = originalMods;

            //背景层的动画
            applyBackgroundBrightness(false, 1);

            this.FadeOut(duration * 0.6f, Easing.OutQuint)
                .ScaleTo(1.2f, duration * 0.6f, Easing.OutQuint);

            Beatmap.UnbindEvents();
            OnScreenSuspending?.Invoke();

            base.OnSuspending(next);
        }

        public override void OnResuming(IScreen last)
        {
            base.OnResuming(last);

            Mods.Value = timeRateMod;

            Beatmap.Disabled = audioControlProvider != null && audioControlProvider != pluginManager.DefaultAudioController;
            this.FadeIn(duration * 0.6f)
                .ScaleTo(1, duration * 0.6f, Easing.OutQuint);

            CurrentTrack.ResetSpeedAdjustments();
            applyTrackAdjustments();

            Beatmap.BindValueChanged(onBeatmapChanged);
            if (Beatmap.Value != suspendBeatmap) Beatmap.TriggerChange();
            else updateBackground(Beatmap.Value);

            //背景层的动画
            background.FadeOut().Then().Delay(duration * 0.6f).FadeIn(duration / 2);
            OnScreenResuming?.Invoke();
        }

        public bool OnPressed(GlobalAction action)
        {
            //查找本体按键绑定
            keyBindings.FirstOrDefault(b => b.Key == action).Value?.Invoke();

            return false;
        }

        public void OnReleased(GlobalAction action) { }

        protected override bool Handle(UIEvent e)
        {
            if (e is MouseMoveEvent)
                showOverlays(false);

            return base.Handle(e);
        }

        protected override bool OnKeyDown(KeyDownEvent e)
        {
            //查找插件按键绑定并执行
            pluginKeyBindings.FirstOrDefault(b => b.Key.Key == e.Key).Key?.Action?.Invoke();

            return base.OnKeyDown(e);
        }

        //当有弹窗或游戏失去焦点时要进行的动作
        protected override void OnHoverLost(HoverLostEvent e)
        {
            if (lockButton.Bindable.Value && OverlaysHidden && !lockButton.Bindable.Disabled)
                lockButton.Bindable.Toggle();

            showOverlays(false);
            base.OnHoverLost(e);
        }

        #endregion

        private void presentBeatmap() =>
            game?.PresentBeatmap(Beatmap.Value.BeatmapSetInfo);

        private void showPluginEntriesTemporary() =>
            currentFunctionBarProvider.ShowFunctionControlTemporary();

        private void hideOverlays(bool force)
        {
            if (!force && !okForHide)
                return;

            OverlaysHidden = true;
            updateIdleVisuals();
            OnIdle?.Invoke();
        }

        private void showOverlays(bool force)
        {
            //在有锁并且悬浮界面已隐藏或悬浮界面可见的情况下显示悬浮锁
            if (!force && ((lockButton.Bindable.Value && OverlaysHidden) || !OverlaysHidden || lockButton.Bindable.Disabled))
            {
                showPluginEntriesTemporary();
                return;
            }

            foreground.FadeTo(1, duration, Easing.OutQuint);

            currentFunctionBarProvider.Show();

            OverlaysHidden = false;

            applyBackgroundBrightness();
            OnResumeFromIdle?.Invoke();
        }

        private void togglePause()
        {
            audioControlProvider?.TogglePause();
            OnTrackRunningToggle?.Invoke(CurrentTrack.IsRunning);
        }

        private void prevTrack() =>
            audioControlProvider?.PrevTrack();

        private void nextTrack() =>
            audioControlProvider?.NextTrack();

        public void SeekTo(double position)
        {
            if (position > CurrentTrack.Length)
                position = CurrentTrack.Length - 10000;

            audioControlProvider?.Seek(position);
            OnSeek?.Invoke(position);
        }

        private void updateIdleVisuals()
        {
            if (!OverlaysHidden)
                return;

            applyBackgroundBrightness(true, idleBgDim.Value);
        }

        private void applyTrackAdjustments()
        {
            CurrentTrack.ResetSpeedAdjustments();
            CurrentTrack.Looping = loopToggleButton.Bindable.Value;
            CurrentTrack.RestartPoint = 0;
            CurrentTrack.AddAdjustment(adjustFreq.Value ? AdjustableProperty.Frequency : AdjustableProperty.Tempo, musicSpeed);

            modRateAdjust.SpeedChange.Value = musicSpeed.Value;
        }

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
                    ? OsuColour.Gray(OverlaysHidden ? idleBgDim.Value : 0.6f)
                    : OsuColour.Gray(brightness);

                b.FadeColour(HideScreenBackground.Value ? Color4.Black : targetColor, duration, Easing.OutQuint);
                background.FadeColour(targetColor, duration, Easing.OutQuint);
            });
        }

        private void onBeatmapChanged(ValueChangedEvent<WorkingBeatmap> v)
        {
            var beatmap = v.NewValue;

            applyTrackAdjustments();
            updateBackground(beatmap);

            activity.Value = new UserActivity.InMvis(beatmap.BeatmapInfo);
            onBeatmapChangedAction?.Invoke(beatmap);
        }
    }
}
