using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
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
using osu.Framework.Logging;
using osu.Framework.Screens;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Input;
using osu.Game.Input.Bindings;
using osu.Game.Overlays.Settings;
using osu.Game.Overlays.Settings.Sections.Mf;
using osu.Game.Rulesets.Mods;
using osu.Game.Screens.Mvis.BottomBar;
using osu.Game.Screens.Mvis.BottomBar.Buttons;
using osu.Game.Screens.Mvis.Misc;
using osu.Game.Screens.Mvis.Plugins;
using osu.Game.Screens.Mvis.Plugins.Types;
using osu.Game.Screens.Mvis.SideBar;
using osu.Game.Screens.Mvis.Skinning;
using osu.Game.Screens.Play;
using osu.Game.Screens.Select;
using osu.Game.Skinning;
using osu.Game.Users;
using osuTK;
using osuTK.Graphics;
using osuTK.Input;
using Sidebar = osu.Game.Screens.Mvis.SideBar.Sidebar;
using SongProgressBar = osu.Game.Screens.Mvis.BottomBar.SongProgressBar;

namespace osu.Game.Screens.Mvis
{
    //todo: 重写界面?
    public class MvisScreen : ScreenWithBeatmapBackground, IKeyBindingHandler<GlobalAction>
    {
        public override bool HideOverlaysOnEnter => true;
        public override bool AllowBackButton => false;

        public override bool CursorVisible => !OverlaysHidden
                                              || sidebar.State.Value == Visibility.Visible
                                              || IsHovered == false; //隐藏界面或侧边栏可见，显示光标

        public override bool AllowRateAdjustments => true;

        private bool okForHide => IsHovered
                                  && isIdle.Value
                                  && !(bottomBar?.IsHovered ?? false)
                                  && !(lockButton?.ToggleableValue.Value ?? false)
                                  && !lockChanges.Value
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
        public Action<WorkingBeatmap> OnBeatmapChanged;

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

        private BottomBarContainer bottomBar;
        private SongProgressBar progressBar;

        //留着这些能让播放器在触发GlobalAction时会有更好的界面体验
        private BottomBarButton soloButton;
        private BottomBarButton prevButton;
        private BottomBarButton nextButton;
        private BottomBarButton sidebarToggleButton;
        private BottomBarButton pluginButton;

        private BottomBarSwitchButton loopToggleButton;
        private BottomBarOverlayLockSwitchButton lockButton;
        private BottomBarSwitchButton songProgressButton;

        #endregion

        #region 背景和前景

        private InputManager inputManager { get; set; }
        private NightcoreBeatContainer nightcoreBeatContainer;

        private Container background;
        private BgTrianglesContainer bgTriangles;
        private FullScreenSkinnableComponent skinnableBbBackground;

        private Container foreground;
        private FullScreenSkinnableComponent skinnableForeground;

        #endregion

        #region overlay

        private LoadingSpinner loadingSpinner;

        private readonly Sidebar sidebar = new Sidebar();

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
        private readonly Dictionary<Key, Action> pluginKeyBindings = new Dictionary<Key, Action>();

        public bool OverlaysHidden { get; private set; }
        private readonly BindableBool lockChanges = new BindableBool();
        private readonly IBindable<bool> isIdle = new BindableBool();

        private readonly Bindable<UserActivity> activity = new Bindable<UserActivity>();

        public DrawableTrack CurrentTrack => audioControlProvider.GetCurrentTrack();

        private readonly BindableList<MvisPlugin> loadList = new BindableList<MvisPlugin>();

        public Bindable<bool> HideTriangles = new Bindable<bool>();
        public Bindable<bool> HideScreenBackground = new Bindable<bool>();

        private IProvideAudioControlPlugin audioControlProvider;
        private readonly OsuMusicControllerWrapper musicControllerWrapper = new OsuMusicControllerWrapper();

        public float BottombarHeight => (bottomBar?.Height - bottomBar?.Y ?? 0) + 10 + 5;

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
            SidebarSettingsScrollContainer settingsScroll;
            SidebarPluginsPage pluginsPage;
            sidebar.AddRange(new Drawable[]
            {
                settingsScroll = new SidebarSettingsScrollContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Child = new FillFlowContainer
                    {
                        AutoSizeAxes = Axes.Y,
                        RelativeSizeAxes = Axes.X,
                        Spacing = new Vector2(20),
                        Padding = new MarginPadding { Top = 10, Left = 5, Right = 5 },
                        Margin = new MarginPadding { Bottom = 10 },
                        Direction = FillDirection.Vertical,
                        Children = new Drawable[]
                        {
                            new MfMvisSection
                            {
                                Margin = new MarginPadding { Top = -15 },
                                Padding = new MarginPadding(0)
                            },
                            new MfMvisPluginSection
                            {
                                Padding = new MarginPadding(0)
                            },
                            new SettingsButton
                            {
                                Text = "歌曲选择",
                                Action = () => this.Push(new MvisSongSelect())
                            }
                        }
                    },
                },
                pluginsPage = new SidebarPluginsPage()
            });

            //配置绑定/设置
            isIdle.BindTo(idleTracker.IsIdle);
            config.BindWith(MSetting.MvisBgBlur, bgBlur);
            config.BindWith(MSetting.MvisIdleBgDim, idleBgDim);
            config.BindWith(MSetting.MvisMusicSpeed, musicSpeed);
            config.BindWith(MSetting.MvisAdjustMusicWithFreq, adjustFreq);
            config.BindWith(MSetting.MvisEnableNightcoreBeat, nightcoreBeat);
            config.BindWith(MSetting.MvisStoryboardProxy, allowProxy);
            currentAudioControlProviderSetting = config.GetBindable<string>(MSetting.MvisCurrentAudioProvider);

            InternalChildren = new Drawable[]
            {
                colourProvider,
                musicControllerWrapper,
                nightcoreBeatContainer = new NightcoreBeatContainer
                {
                    Alpha = 0
                },
                new Container
                {
                    Name = "Contents",
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding { Horizontal = HORIZONTAL_OVERFLOW_PADDING },
                    Children = new Drawable[]
                    {
                        bgTriangles = new BgTrianglesContainer(),
                        background = new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Name = "Background Layer",
                        },
                        foreground = new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Name = "Foreground Layer",
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre
                        }
                    }
                },
                skinnableForeground = new FullScreenSkinnableComponent("MPlayer-foreground", confineMode: ConfineMode.ScaleToFill, defaultImplementation: _ => new PlaceHolder())
                {
                    Name = "前景图",
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Alpha = 0,
                    OverrideChildAnchor = true
                },
                new Container
                {
                    Name = "Overlay Layer",
                    RelativeSizeAxes = Axes.Both,
                    Depth = float.MinValue,
                    Padding = new MarginPadding { Horizontal = HORIZONTAL_OVERFLOW_PADDING },
                    Children = new Drawable[]
                    {
                        skinnableBbBackground = new FullScreenSkinnableComponent("MBottomBar-background",
                            confineMode: ConfineMode.ScaleToFill,
                            masking: true,
                            defaultImplementation: _ => new PlaceHolder())
                        {
                            Name = "底栏背景图",
                            Anchor = Anchor.BottomCentre,
                            Origin = Anchor.BottomCentre,
                            RelativeSizeAxes = Axes.X,
                            Height = 100,
                            ChildAnchor = Anchor.BottomCentre,
                            ChildOrigin = Anchor.BottomCentre,
                            Alpha = 0,
                            CentreComponent = false,
                            OverrideChildAnchor = true
                        },
                        sidebar,
                        loadingSpinner = new LoadingSpinner(true, true)
                        {
                            Anchor = Anchor.BottomCentre,
                            Origin = Anchor.BottomCentre,
                            Margin = new MarginPadding(115)
                        },
                        bottomBar = new BottomBarContainer
                        {
                            LeftContent = new Drawable[]
                            {
                                new BottomBarButton
                                {
                                    ButtonIcon = FontAwesome.Solid.ArrowLeft,
                                    Action = this.Exit,
                                    TooltipText = "退出",
                                },
                                new BottomBarButton
                                {
                                    ButtonIcon = FontAwesome.Regular.QuestionCircle,
                                    Action = () => game?.OpenUrlExternally("https://matrix-feather.github.io/mfosu/mfosu_mp_manual/"),
                                    TooltipText = "食用手册"
                                }
                            },
                            CentreContent = new Drawable[]
                            {
                                prevButton = new NextPrevButton
                                {
                                    Size = new Vector2(50, 30),
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    ButtonIcon = FontAwesome.Solid.StepBackward,
                                    Action = prevTrack,
                                    TooltipText = "上一首 / 重新开始",
                                },
                                songProgressButton = new SongProgressButton
                                {
                                    TooltipText = "暂停 / 播放",
                                    Action = togglePause,
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre
                                },
                                nextButton = new NextPrevButton
                                {
                                    Size = new Vector2(50, 30),
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    ButtonIcon = FontAwesome.Solid.StepForward,
                                    Action = nextTrack,
                                    TooltipText = "下一首",
                                },
                            },
                            RightContent = new Drawable[]
                            {
                                pluginButton = new BottomBarButton
                                {
                                    ButtonIcon = FontAwesome.Solid.Plug,
                                    TooltipText = "查看插件",
                                    Action = () => updateSidebarState(pluginsPage)
                                },
                                new BottomBarButton
                                {
                                    ButtonIcon = FontAwesome.Solid.Desktop,
                                    Action = () =>
                                    {
                                        //隐藏界面，锁定更改并隐藏锁定按钮
                                        lockChanges.Value = false;
                                        hideOverlays(true);

                                        updateSidebarState(null);

                                        //防止手机端无法恢复界面
                                        lockChanges.Value = RuntimeInfo.IsDesktop;
                                        lockButton.ToggleableValue.Value = !RuntimeInfo.IsDesktop;
                                    },
                                    TooltipText = "锁定变更并隐藏界面"
                                },
                                loopToggleButton = new ToggleLoopButton
                                {
                                    ButtonIcon = FontAwesome.Solid.Undo,
                                    Action = () => CurrentTrack.Looping = loopToggleButton.ToggleableValue.Value,
                                    TooltipText = "单曲循环",
                                },
                                soloButton = new BottomBarButton
                                {
                                    ButtonIcon = FontAwesome.Solid.User,
                                    Action = presentBeatmap,
                                    TooltipText = "在单人游戏选歌界面中查看",
                                },
                                sidebarToggleButton = new BottomBarButton
                                {
                                    ButtonIcon = FontAwesome.Solid.Cog,
                                    Action = () => updateSidebarState(settingsScroll),
                                    TooltipText = "播放器设置",
                                }
                            }
                        },
                        progressBar = new SongProgressBar
                        {
                            OnSeek = SeekTo
                        }
                    }
                }
            };

            //后期设置
            bottomBar.PluginEntriesFillFlow.Add(lockButton = new BottomBarOverlayLockSwitchButton
            {
                TooltipText = "锁定变更",
                Action = showPluginEntriesTemporary
            });

            //todo: 找出为啥audioControlProvider会在被赋值前访问
            audioControlProvider = musicControllerWrapper;
        }

        protected override void LoadComplete()
        {
            //各种BindValueChanged
            //这部分放load会导致当前屏幕为主界面时，播放器会在后台相应设置变动
            loadList.BindCollectionChanged(onLoadListChanged);

            bgBlur.BindValueChanged(v => updateBackground(Beatmap.Value));
            idleBgDim.BindValueChanged(_ => updateIdleVisuals());
            lockChanges.BindValueChanged(v =>
            {
                lockButton.Disabled = v.NewValue;
            });

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

            songProgressButton.ToggleableValue.BindTo(trackRunning);

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
                        var btn = pluginSidebarPage.CreateBottomBarButton();

                        //如果插件的侧边栏页面有入口按钮
                        if (btn != null)
                        {
                            btn.Action = () => updateSidebarState(pluginSidebarPage);
                            btn.TooltipText += $" ({pluginSidebarPage.ShortcutKey})";

                            bottomBar.PluginEntriesFillFlow.Add(btn);
                        }

                        //如果插件的侧边栏页面有调用快捷键
                        if (pluginSidebarPage.ShortcutKey != Key.Unknown)
                        {
                            pluginKeyBindings[pluginSidebarPage.ShortcutKey] = () =>
                            {
                                if (!pl.Disabled.Value) btn?.Click();
                            };
                        }
                    }
                }
                catch (Exception e)
                {
                    Logger.Error(e, $"在添加 {pl.Name} 时出现问题, 请联系你的插件提供方");
                }
            }

            //把lockButton放在中间
            bottomBar.CentreBotton(lockButton);

            //更新当前音乐控制插件
            currentAudioControlProviderSetting.BindValueChanged(v =>
            {
                //获取与新值匹配的控制插件
                var pl = (IProvideAudioControlPlugin)pluginManager.GetAllPlugins(false).FirstOrDefault(p => v.NewValue == $"{p.GetType().Namespace}+{p.GetType().Name}");

                //如果没找到(为null)，则解锁Beatmap.Disabled
                Beatmap.Disabled = pl != null;

                //设置当前控制插件IsCurrent为false
                audioControlProvider.IsCurrent = false;

                //切换并设置当前控制插件IsCurrent为true
                audioControlProvider = pl ?? musicControllerWrapper;
                audioControlProvider.IsCurrent = true;
            }, true);

            bottomBar.MoveToY(bottomBar.Height + 10).FadeOut();
            progressBar.MoveToY(5);

            base.LoadComplete();
        }

        private void setupKeyBindings()
        {
            keyBindings[GlobalAction.MvisMusicPrev] = () => prevButton.Click();
            keyBindings[GlobalAction.MvisMusicNext] = () => nextButton.Click();
            keyBindings[GlobalAction.MvisOpenInSongSelect] = () => soloButton.Click();
            keyBindings[GlobalAction.MvisToggleOverlayLock] = () => lockButton.Click();
            keyBindings[GlobalAction.MvisTogglePluginPage] = () => pluginButton.Click();
            keyBindings[GlobalAction.MvisTogglePause] = () => songProgressButton.Click();
            keyBindings[GlobalAction.MvisToggleTrackLoop] = () => loopToggleButton.Click();
            keyBindings[GlobalAction.MvisTogglePlayList] = () => sidebarToggleButton.Click();
            keyBindings[GlobalAction.MvisForceLockOverlayChanges] = () => lockChanges.Toggle();
            keyBindings[GlobalAction.Back] = () =>
            {
                if (sidebar.IsPresent && !sidebar.Hiding)
                {
                    sidebar.Hide();
                    return;
                }

                if (OverlaysHidden)
                {
                    lockChanges.Value = false;
                    lockButton.ToggleableValue.Value = false;
                    showOverlays(true);
                }
                else
                    this.Exit();
            };
        }

        private void onPluginUnLoad(MvisPlugin pl)
        {
            //查找与pl对应的侧边栏页面
            foreach (var sc in sidebar.Components)
            {
                //如果找到的侧边栏的Plugin与pl匹配
                if (sc is PluginSidebarPage plsp && plsp.Plugin == pl)
                {
                    sidebar.Remove(plsp); //移除这个页面
                    pluginKeyBindings.Remove(plsp.ShortcutKey); //移除快捷键

                    //查找与plsp对应的底栏入口
                    foreach (var d in bottomBar.PluginEntriesFillFlow)
                    {
                        //同上
                        if (d is PluginBottomBarButton btn && btn.Page == plsp)
                        {
                            btn.FadeTo(0.01f, 300, Easing.OutQuint).Then().Schedule(() =>
                            {
                                btn.Expire();
                                bottomBar.CentreBotton(lockButton);
                            });
                        }
                    }
                }
            }
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

            var sc = (ISidebarContent)d;

            //如果sc是上一个显示(mvis)或sc是侧边栏的当前显示并且侧边栏未隐藏
            if (sc == sidebar.CurrentDisplay.Value && !sidebar.Hiding)
            {
                sidebar.Hide();
                return;
            }

            sidebar.ShowComponent(d);
        }

        #region override事件

        protected override void Update()
        {
            base.Update();

            trackRunning.Value = CurrentTrack.IsRunning;
            progressBar.CurrentTime = CurrentTrack.CurrentTime;
            progressBar.EndTime = CurrentTrack.Length;
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
            skinnableForeground.FadeIn(duration, Easing.OutQuint);

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
            lockChanges.Value = true;

            //非背景层的动画
            foreground.ScaleTo(0, duration, Easing.OutQuint);
            bottomBar.MoveToY(bottomBar.Height + 10, duration, Easing.OutQuint).FadeOut(duration, Easing.OutExpo);
            progressBar.MoveToY(3.5f, duration, Easing.OutQuint).FadeTo(0.1f, duration, Easing.OutExpo);

            this.FadeOut(500, Easing.OutQuint);

            OnScreenExiting?.Invoke();
            pluginManager.OnPluginUnLoad -= onPluginUnLoad;

            return base.OnExiting(next);
        }

        public override void OnSuspending(IScreen next)
        {
            CurrentTrack.ResetSpeedAdjustments();
            Beatmap.Disabled = false;

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

            Beatmap.Disabled = audioControlProvider != null && audioControlProvider != musicControllerWrapper;
            this.FadeIn(duration * 0.6f)
                .ScaleTo(1, duration * 0.6f, Easing.OutQuint);

            CurrentTrack.ResetSpeedAdjustments();
            applyTrackAdjustments();
            updateBackground(Beatmap.Value);

            Beatmap.BindValueChanged(onBeatmapChanged, true);

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
            //查找插件按键绑定
            pluginKeyBindings.FirstOrDefault(b => b.Key == e.Key).Value?.Invoke();

            return base.OnKeyDown(e);
        }

        //当有弹窗或游戏失去焦点时要进行的动作
        protected override void OnHoverLost(HoverLostEvent e)
        {
            if (lockButton.ToggleableValue.Value && OverlaysHidden)
                lockButton.Toggle();

            showOverlays(false);
            base.OnHoverLost(e);
        }

        #endregion

        private void presentBeatmap() =>
            game?.PresentBeatmap(Beatmap.Value.BeatmapSetInfo);

        private void showPluginEntriesTemporary() =>
            bottomBar.PluginEntriesFillFlow.FadeIn(500, Easing.OutQuint).Then().Delay(2000).FadeOut(500, Easing.OutQuint);

        private void hideOverlays(bool force)
        {
            if (!force && !okForHide)
                return;

            skinnableBbBackground.MoveToY(bottomBar.Height, duration, Easing.OutQuint)
                                 .FadeOut(duration, Easing.OutQuint);

            progressBar.MoveToY(4f, duration, Easing.OutQuint);

            OverlaysHidden = true;
            updateIdleVisuals();
            OnIdle?.Invoke();
        }

        private void showOverlays(bool force)
        {
            //在有锁并且悬浮界面已隐藏或悬浮界面可见的情况下显示悬浮锁
            if (!force && ((lockButton.ToggleableValue.Value && OverlaysHidden) || !OverlaysHidden || lockChanges.Value))
            {
                showPluginEntriesTemporary();
                return;
            }

            foreground.FadeTo(1, duration, Easing.OutQuint);

            skinnableBbBackground.MoveToY(0, duration, Easing.OutQuint)
                                 .FadeIn(duration, Easing.OutQuint);

            progressBar.MoveToY(0, duration, Easing.OutQuint);

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
            CurrentTrack.Looping = loopToggleButton.ToggleableValue.Value;
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
            OnBeatmapChanged?.Invoke(beatmap);
        }
    }
}
