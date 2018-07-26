﻿// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using osu.Framework.Configuration;
using osu.Framework.Screens;
using osu.Game.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Overlays;
using osu.Framework.Logging;
using osu.Framework.Allocation;
using osu.Game.Overlays.Toolbar;
using osu.Game.Screens;
using osu.Game.Screens.Menu;
using OpenTK;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework.Audio;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Input;
using osu.Framework.Input.Bindings;
using osu.Framework.Platform;
using osu.Framework.Threading;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Rulesets.Scoring;
using osu.Game.Overlays.Notifications;
using osu.Game.Rulesets;
using osu.Game.Screens.Play;
using osu.Game.Input.Bindings;
using osu.Game.Rulesets.Mods;
using osu.Game.Skinning;
using OpenTK.Graphics;
using osu.Game.Overlays.Volume;
using osu.Game.Screens.Select;

namespace osu.Game
{
    /// <summary>
    /// The full osu! experience. Builds on top of <see cref="OsuGameBase"/> to add menus and binding logic
    /// for initial components that are generally retrieved via DI.
    /// </summary>
    public class OsuGame : OsuGameBase, IKeyBindingHandler<GlobalAction>
    {
        public Toolbar Toolbar;

        private ChatOverlay chat;

        private MusicController musicController;

        private NotificationOverlay notifications;

        private DialogOverlay dialogOverlay;

        private DirectOverlay direct;

        private SocialOverlay social;

        private UserProfileOverlay userProfile;

        private BeatmapSetOverlay beatmapSetOverlay;

        private ScreenshotManager screenshotManager;

        public virtual Storage GetStorageForStableInstall() => null;

        private Intro intro
        {
            get
            {
                Screen s = screenStack;
                while (s != null && !(s is Intro))
                    s = s.ChildScreen;
                return s as Intro;
            }
        }

        public float ToolbarOffset => Toolbar.Position.Y + Toolbar.DrawHeight;

        public readonly Bindable<OverlayActivation> OverlayActivationMode = new Bindable<OverlayActivation>();

        private OsuScreen screenStack;

        private VolumeOverlay volume;
        private OnScreenDisplay onscreenDisplay;

        private Bindable<int> configRuleset;
        private readonly Bindable<RulesetInfo> ruleset = new Bindable<RulesetInfo>();

        private Bindable<int> configSkin;

        private readonly string[] args;

        private SettingsOverlay settings;

        private readonly List<OverlayContainer> overlays = new List<OverlayContainer>();

        // todo: move this to SongSelect once Screen has the ability to unsuspend.
        public readonly Bindable<IEnumerable<Mod>> SelectedMods = new Bindable<IEnumerable<Mod>>(new List<Mod>());

        public OsuGame(string[] args = null)
        {
            this.args = args;

            forwardLoggedErrorsToNotifications();
        }

        public void ToggleSettings() => settings.ToggleVisibility();

        public void ToggleDirect() => direct.ToggleVisibility();

        /// <summary>
        /// Close all game-wide overlays.
        /// </summary>
        /// <param name="toolbar">Whether the toolbar should also be hidden.</param>
        public void CloseAllOverlays(bool toolbar = true)
        {
            foreach (var o in overlays)
                o.State = Visibility.Hidden;
            if (toolbar) Toolbar.State = Visibility.Hidden;
        }

        private DependencyContainer dependencies;

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent) =>
            dependencies = new DependencyContainer(base.CreateChildDependencies(parent));

        [BackgroundDependencyLoader]
        private void load(FrameworkConfigManager frameworkConfig)
        {
            this.frameworkConfig = frameworkConfig;

            ScoreStore.ScoreImported += score => Schedule(() => LoadScore(score));

            if (!Host.IsPrimaryInstance)
            {
                Logger.Log(@"osu! does not support multiple running instances.", LoggingTarget.Runtime, LogLevel.Error);
                Environment.Exit(0);
            }

            if (args?.Length > 0)
            {
                var paths = args.Where(a => !a.StartsWith(@"-"));

                Task.Run(() => Import(paths.ToArray()));
            }

            dependencies.CacheAs(this);

            dependencies.CacheAs(ruleset);
            dependencies.CacheAs<IBindable<RulesetInfo>>(ruleset);

            // bind config int to database RulesetInfo
            configRuleset = LocalConfig.GetBindable<int>(OsuSetting.Ruleset);
            ruleset.Value = RulesetStore.GetRuleset(configRuleset.Value) ?? RulesetStore.AvailableRulesets.First();
            ruleset.ValueChanged += r => configRuleset.Value = r.ID ?? 0;

            // bind config int to database SkinInfo
            configSkin = LocalConfig.GetBindable<int>(OsuSetting.Skin);
            SkinManager.CurrentSkinInfo.ValueChanged += s => configSkin.Value = s.ID;
            configSkin.ValueChanged += id => SkinManager.CurrentSkinInfo.Value = SkinManager.Query(s => s.ID == id) ?? SkinInfo.Default;
            configSkin.TriggerChange();

            LocalConfig.BindWith(OsuSetting.VolumeInactive, inactiveVolumeAdjust);
        }

        private ScheduledDelegate scoreLoad;

        /// <summary>
        /// Open chat to a channel matching the provided name, if present.
        /// </summary>
        /// <param name="channelName">The name of the channel.</param>
        public void OpenChannel(string channelName) => chat.OpenChannel(chat.AvailableChannels.Find(c => c.Name == channelName));

        /// <summary>
        /// Show a beatmap set as an overlay.
        /// </summary>
        /// <param name="setId">The set to display.</param>
        public void ShowBeatmapSet(int setId) => beatmapSetOverlay.FetchAndShowBeatmapSet(setId);

        /// <summary>
        /// Present a beatmap at song select.
        /// </summary>
        /// <param name="beatmap">The beatmap to select.</param>
        public void PresentBeatmap(BeatmapSetInfo beatmap)
        {
            CloseAllOverlays(false);

            void setBeatmap()
            {
                if (Beatmap.Disabled)
                {
                    Schedule(setBeatmap);
                    return;
                }

                Beatmap.Value = BeatmapManager.GetWorkingBeatmap(beatmap.Beatmaps.First());
            }

            switch (currentScreen)
            {
                case SongSelect _:
                    break;
                default:
                    // navigate to song select if we are not already there.
                    var menu = (MainMenu)intro.ChildScreen;

                    menu.MakeCurrent();
                    menu.LoadToSolo();
                    break;
            }

            setBeatmap();
        }

        /// <summary>
        /// Show a user's profile as an overlay.
        /// </summary>
        /// <param name="userId">The user to display.</param>
        public void ShowUser(long userId) => userProfile.ShowUser(userId);

        /// <summary>
        /// Show a beatmap's set as an overlay, displaying the given beatmap.
        /// </summary>
        /// <param name="beatmapId">The beatmap to show.</param>
        public void ShowBeatmap(int beatmapId) => beatmapSetOverlay.FetchAndShowBeatmap(beatmapId);

        protected void LoadScore(Score s)
        {
            scoreLoad?.Cancel();

            var menu = intro.ChildScreen;

            if (menu == null)
            {
                scoreLoad = Schedule(() => LoadScore(s));
                return;
            }

            if (!menu.IsCurrentScreen)
            {
                menu.MakeCurrent();
                this.Delay(500).Schedule(() => LoadScore(s), out scoreLoad);
                return;
            }

            if (s.Beatmap == null)
            {
                notifications.Post(new SimpleNotification
                {
                    Text = @"Tried to load a score for a beatmap we don't have!",
                    Icon = FontAwesome.fa_life_saver,
                });
                return;
            }

            ruleset.Value = s.Ruleset;

            Beatmap.Value = BeatmapManager.GetWorkingBeatmap(s.Beatmap);
            Beatmap.Value.Mods.Value = s.Mods;

            menu.Push(new PlayerLoader(new ReplayPlayer(s.Replay)));
        }

        protected override void LoadComplete()
        {
            // this needs to be cached before base.LoadComplete as it is used by MenuCursorContainer.
            dependencies.Cache(screenshotManager = new ScreenshotManager());

            base.LoadComplete();

            // The next time this is updated is in UpdateAfterChildren, which occurs too late and results
            // in the cursor being shown for a few frames during the intro.
            // This prevents the cursor from showing until we have a screen with CursorVisible = true
            MenuCursorContainer.CanShowCursor = currentScreen?.CursorVisible ?? false;

            // hook up notifications to components.
            SkinManager.PostNotification = n => notifications?.Post(n);
            BeatmapManager.PostNotification = n => notifications?.Post(n);

            BeatmapManager.GetStableStorage = GetStorageForStableInstall;
            BeatmapManager.PresentBeatmap = PresentBeatmap;

            AddRange(new Drawable[]
            {
                new VolumeControlReceptor
                {
                    RelativeSizeAxes = Axes.Both,
                    ActionRequested = action => volume.Adjust(action),
                    ScrollActionRequested = (action, amount, isPrecise) => volume.Adjust(action, amount, isPrecise),
                },
                mainContent = new Container { RelativeSizeAxes = Axes.Both },
                overlayContent = new Container { RelativeSizeAxes = Axes.Both, Depth = float.MinValue },
            });

            loadComponentSingleFile(screenStack = new Loader(), d =>
            {
                screenStack.ModePushed += screenAdded;
                screenStack.Exited += screenRemoved;
                mainContent.Add(screenStack);
            });

            loadComponentSingleFile(Toolbar = new Toolbar
            {
                Depth = -5,
                OnHome = delegate
                {
                    CloseAllOverlays(false);
                    intro?.ChildScreen?.MakeCurrent();
                },
            }, overlayContent.Add);

            loadComponentSingleFile(volume = new VolumeOverlay(), overlayContent.Add);
            loadComponentSingleFile(onscreenDisplay = new OnScreenDisplay(), Add);

            loadComponentSingleFile(screenshotManager, Add);

            //overlay elements
            loadComponentSingleFile(direct = new DirectOverlay { Depth = -1 }, mainContent.Add);
            loadComponentSingleFile(social = new SocialOverlay { Depth = -1 }, mainContent.Add);
            loadComponentSingleFile(chat = new ChatOverlay { Depth = -1 }, mainContent.Add);
            loadComponentSingleFile(settings = new MainSettings
            {
                GetToolbarHeight = () => ToolbarOffset,
                Depth = -1
            }, overlayContent.Add);
            loadComponentSingleFile(userProfile = new UserProfileOverlay { Depth = -2 }, mainContent.Add);
            loadComponentSingleFile(beatmapSetOverlay = new BeatmapSetOverlay { Depth = -3 }, mainContent.Add);
            loadComponentSingleFile(musicController = new MusicController
            {
                Depth = -4,
                Position = new Vector2(0, Toolbar.HEIGHT),
                Anchor = Anchor.TopRight,
                Origin = Anchor.TopRight,
            }, overlayContent.Add);

            loadComponentSingleFile(notifications = new NotificationOverlay
            {
                GetToolbarHeight = () => ToolbarOffset,
                Depth = -4,
                Anchor = Anchor.TopRight,
                Origin = Anchor.TopRight,
            }, overlayContent.Add);

            loadComponentSingleFile(dialogOverlay = new DialogOverlay
            {
                Depth = -6,
            }, overlayContent.Add);

            dependencies.Cache(settings);
            dependencies.Cache(onscreenDisplay);
            dependencies.Cache(social);
            dependencies.Cache(direct);
            dependencies.Cache(chat);
            dependencies.Cache(userProfile);
            dependencies.Cache(musicController);
            dependencies.Cache(beatmapSetOverlay);
            dependencies.Cache(notifications);
            dependencies.Cache(dialogOverlay);

            var singleDisplaySideOverlays = new OverlayContainer[] { settings, notifications };
            overlays.AddRange(singleDisplaySideOverlays);

            foreach (var overlay in singleDisplaySideOverlays)
            {
                overlay.StateChanged += state =>
                {
                    if (state == Visibility.Hidden) return;
                    singleDisplaySideOverlays.Where(o => o != overlay).ForEach(o => o.Hide());
                };
            }

            // eventually informational overlays should be displayed in a stack, but for now let's only allow one to stay open at a time.
            var informationalOverlays = new OverlayContainer[] { beatmapSetOverlay, userProfile };
            overlays.AddRange(informationalOverlays);

            foreach (var overlay in informationalOverlays)
            {
                overlay.StateChanged += state =>
                {
                    if (state == Visibility.Hidden) return;
                    informationalOverlays.Where(o => o != overlay).ForEach(o => o.Hide());
                };
            }

            // ensure only one of these overlays are open at once.
            var singleDisplayOverlays = new OverlayContainer[] { chat, social, direct };
            overlays.AddRange(singleDisplayOverlays);

            foreach (var overlay in singleDisplayOverlays)
            {
                overlay.StateChanged += state =>
                {
                    // informational overlays should be dismissed on a show or hide of a full overlay.
                    informationalOverlays.ForEach(o => o.Hide());

                    if (state == Visibility.Hidden) return;

                    singleDisplayOverlays.Where(o => o != overlay).ForEach(o => o.Hide());
                };
            }

            OverlayActivationMode.ValueChanged += v =>
            {
                if (v != OverlayActivation.All) CloseAllOverlays();
            };

            void updateScreenOffset()
            {
                float offset = 0;

                if (settings.State == Visibility.Visible)
                    offset += ToolbarButton.WIDTH / 2;
                if (notifications.State == Visibility.Visible)
                    offset -= ToolbarButton.WIDTH / 2;

                screenStack.MoveToX(offset, SettingsOverlay.TRANSITION_LENGTH, Easing.OutQuint);
            }

            settings.StateChanged += _ => updateScreenOffset();
            notifications.StateChanged += _ => updateScreenOffset();
        }

        private void forwardLoggedErrorsToNotifications()
        {
            int recentLogCount = 0;

            const double debounce = 5000;

            Logger.NewEntry += entry =>
            {
                if (entry.Level < LogLevel.Important || entry.Target == null) return;

                const int short_term_display_limit = 3;

                if (recentLogCount < short_term_display_limit)
                {
                    Schedule(() => notifications.Post(new SimpleNotification
                    {
                        Icon = entry.Level == LogLevel.Important ? FontAwesome.fa_exclamation_circle : FontAwesome.fa_bomb,
                        Text = entry.Message,
                    }));
                }
                else if (recentLogCount == short_term_display_limit)
                {
                    Schedule(() => notifications.Post(new SimpleNotification
                    {
                        Icon = FontAwesome.fa_ellipsis_h,
                        Text = "Subsequent messages have been logged. Click to view log files.",
                        Activated = () =>
                        {
                            Host.Storage.GetStorageForDirectory("logs").OpenInNativeExplorer();
                            return true;
                        }
                    }));
                }

                Interlocked.Increment(ref recentLogCount);
                Scheduler.AddDelayed(() => Interlocked.Decrement(ref recentLogCount), debounce);
            };
        }

        private Task asyncLoadStream;
        private int visibleOverlayCount;

        private void loadComponentSingleFile<T>(T d, Action<T> add)
            where T : Drawable
        {
            var focused = d as FocusedOverlayContainer;
            if (focused != null)
            {
                focused.StateChanged += s =>
                {
                    visibleOverlayCount += s == Visibility.Visible ? 1 : -1;
                    screenStack.FadeColour(visibleOverlayCount > 0 ? OsuColour.Gray(0.5f) : Color4.White, 500, Easing.OutQuint);
                };
            }

            // schedule is here to ensure that all component loads are done after LoadComplete is run (and thus all dependencies are cached).
            // with some better organisation of LoadComplete to do construction and dependency caching in one step, followed by calls to loadComponentSingleFile,
            // we could avoid the need for scheduling altogether.
            Schedule(() => { asyncLoadStream = asyncLoadStream?.ContinueWith(t => LoadComponentAsync(d, add).Wait()) ?? LoadComponentAsync(d, add); });
        }

        public bool OnPressed(GlobalAction action)
        {
            if (intro == null) return false;

            switch (action)
            {
                case GlobalAction.ToggleChat:
                    chat.ToggleVisibility();
                    return true;
                case GlobalAction.ToggleSocial:
                    social.ToggleVisibility();
                    return true;
                case GlobalAction.ResetInputSettings:
                    var sensitivity = frameworkConfig.GetBindable<double>(FrameworkSetting.CursorSensitivity);

                    sensitivity.Disabled = false;
                    sensitivity.Value = 1;
                    sensitivity.Disabled = true;

                    frameworkConfig.Set(FrameworkSetting.IgnoredInputHandlers, string.Empty);
                    frameworkConfig.GetBindable<ConfineMouseMode>(FrameworkSetting.ConfineMouseMode).SetDefault();
                    return true;
                case GlobalAction.ToggleToolbar:
                    Toolbar.ToggleVisibility();
                    return true;
                case GlobalAction.ToggleSettings:
                    settings.ToggleVisibility();
                    return true;
                case GlobalAction.ToggleDirect:
                    direct.ToggleVisibility();
                    return true;
                case GlobalAction.ToggleGameplayMouseButtons:
                    LocalConfig.Set(OsuSetting.MouseDisableButtons, !LocalConfig.Get<bool>(OsuSetting.MouseDisableButtons));
                    return true;
            }

            return false;
        }

        private readonly BindableDouble inactiveVolumeAdjust = new BindableDouble();

        protected override void OnDeactivated()
        {
            base.OnDeactivated();
            Audio.AddAdjustment(AdjustableProperty.Volume, inactiveVolumeAdjust);
        }

        protected override void OnActivated()
        {
            base.OnActivated();
            Audio.RemoveAdjustment(AdjustableProperty.Volume, inactiveVolumeAdjust);
        }

        public bool OnReleased(GlobalAction action) => false;

        private Container mainContent;

        private Container overlayContent;

        private OsuScreen currentScreen;
        private FrameworkConfigManager frameworkConfig;

        protected override bool OnExiting()
        {
            if (screenStack.ChildScreen == null) return false;

            if (intro == null) return true;

            if (!intro.DidLoadMenu || intro.ChildScreen != null)
            {
                Scheduler.Add(intro.MakeCurrent);
                return true;
            }

            return base.OnExiting();
        }

        /// <summary>
        /// Use to programatically exit the game as if the user was triggering via alt-f4.
        /// Will keep persisting until an exit occurs (exit may be blocked multiple times).
        /// </summary>
        public void GracefullyExit()
        {
            if (!OnExiting())
                Exit();
            else
                Scheduler.AddDelayed(GracefullyExit, 2000);
        }

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();

            // we only want to apply these restrictions when we are inside a screen stack.
            // the use case for not applying is in visual/unit tests.
            bool applyBeatmapRulesetRestrictions = !currentScreen?.AllowBeatmapRulesetChange ?? false;

            ruleset.Disabled = applyBeatmapRulesetRestrictions;
            Beatmap.Disabled = applyBeatmapRulesetRestrictions;

            mainContent.Padding = new MarginPadding { Top = ToolbarOffset };

            MenuCursorContainer.CanShowCursor = currentScreen?.CursorVisible ?? false;
        }

        private void screenAdded(Screen newScreen)
        {
            currentScreen = (OsuScreen)newScreen;

            newScreen.ModePushed += screenAdded;
            newScreen.Exited += screenRemoved;
        }

        private void screenRemoved(Screen newScreen)
        {
            currentScreen = (OsuScreen)newScreen;

            if (newScreen == null)
                Exit();
        }
    }
}
