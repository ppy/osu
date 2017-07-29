﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Configuration;
using osu.Framework.Screens;
using osu.Game.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Overlays;
using osu.Framework.Input;
using OpenTK.Input;
using osu.Framework.Logging;
using osu.Game.Graphics.UserInterface.Volume;
using osu.Framework.Allocation;
using osu.Framework.Timing;
using osu.Game.Overlays.Toolbar;
using osu.Game.Screens;
using osu.Game.Screens.Menu;
using OpenTK;
using System.Linq;
using System.Threading.Tasks;
using osu.Framework.Threading;
using osu.Game.Graphics;
using osu.Game.Rulesets.Scoring;
using osu.Game.Overlays.Notifications;
using osu.Game.Rulesets;
using osu.Game.Screens.Play;

namespace osu.Game
{
    public class OsuGame : OsuGameBase
    {
        public Toolbar Toolbar;

        private ChatOverlay chat;

        private MusicController musicController;

        private NotificationManager notificationManager;

        private DialogOverlay dialogOverlay;

        private DirectOverlay direct;

        private SocialOverlay social;

        private UserProfileOverlay userProfile;

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

        private OsuScreen screenStack;

        private VolumeControl volume;

        private Bindable<int> configRuleset;
        public Bindable<RulesetInfo> Ruleset = new Bindable<RulesetInfo>();

        private readonly string[] args;

        private SettingsOverlay settings;

        public OsuGame(string[] args = null)
        {
            this.args = args;
        }

        public void ToggleSettings() => settings.ToggleVisibility();

        public void ToggleDirect() => direct.ToggleVisibility();

        private DependencyContainer dependencies;

        protected override IReadOnlyDependencyContainer CreateLocalDependencies(IReadOnlyDependencyContainer parent) =>
            dependencies = new DependencyContainer(base.CreateLocalDependencies(parent));

        [BackgroundDependencyLoader]
        private void load(FrameworkConfigManager frameworkConfig)
        {
            this.frameworkConfig = frameworkConfig;

            if (!Host.IsPrimaryInstance)
            {
                Logger.Log(@"osu! does not support multiple running instances.", LoggingTarget.Runtime, LogLevel.Error);
                Environment.Exit(0);
            }

            if (args?.Length > 0)
            {
                var paths = args.Where(a => !a.StartsWith(@"-"));
                Task.Run(() => BeatmapManager.Import(paths.ToArray()));
            }

            dependencies.Cache(this);

            configRuleset = LocalConfig.GetBindable<int>(OsuSetting.Ruleset);
            Ruleset.Value = RulesetStore.GetRuleset(configRuleset.Value);
            Ruleset.ValueChanged += r => configRuleset.Value = r.ID ?? 0;
        }

        private ScheduledDelegate scoreLoad;

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
                notificationManager.Post(new SimpleNotification
                {
                    Text = @"Tried to load a score for a beatmap we don't have!",
                    Icon = FontAwesome.fa_life_saver,
                });
                return;
            }

            Beatmap.Value = BeatmapManager.GetWorkingBeatmap(s.Beatmap);

            menu.Push(new PlayerLoader(new ReplayPlayer(s.Replay)));
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            AddRange(new Drawable[] {
                new VolumeControlReceptor
                {
                    RelativeSizeAxes = Axes.Both,
                    ActionRequested = delegate(InputState state) { volume.Adjust(state); }
                },
                mainContent = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                },
                volume = new VolumeControl(),
                overlayContent = new Container { RelativeSizeAxes = Axes.Both },
                new OnScreenDisplay(),
                new GlobalHotkeys //exists because UserInputManager is at a level below us.
                {
                    Handler = globalHotkeyPressed
                }
            });

            LoadComponentAsync(screenStack = new Loader(), d =>
            {
                screenStack.ModePushed += screenAdded;
                screenStack.Exited += screenRemoved;
                mainContent.Add(screenStack);
            });

            //overlay elements
            LoadComponentAsync(direct = new DirectOverlay { Depth = -1 }, mainContent.Add);
            LoadComponentAsync(social = new SocialOverlay { Depth = -1 }, mainContent.Add);
            LoadComponentAsync(chat = new ChatOverlay { Depth = -1 }, mainContent.Add);
            LoadComponentAsync(settings = new SettingsOverlay { Depth = -1 }, overlayContent.Add);
            LoadComponentAsync(userProfile = new UserProfileOverlay { Depth = -2 }, mainContent.Add);
            LoadComponentAsync(musicController = new MusicController
            {
                Depth = -3,
                Position = new Vector2(0, Toolbar.HEIGHT),
                Anchor = Anchor.TopRight,
                Origin = Anchor.TopRight,
            }, overlayContent.Add);

            LoadComponentAsync(notificationManager = new NotificationManager
            {
                Depth = -3,
                Anchor = Anchor.TopRight,
                Origin = Anchor.TopRight,
            }, overlayContent.Add);

            LoadComponentAsync(dialogOverlay = new DialogOverlay
            {
                Depth = -5,
            }, overlayContent.Add);

            Logger.NewEntry += entry =>
            {
                if (entry.Level < LogLevel.Important) return;

                notificationManager.Post(new SimpleNotification
                {
                    Text = $@"{entry.Level}: {entry.Message}"
                });
            };

            dependencies.Cache(settings);
            dependencies.Cache(social);
            dependencies.Cache(chat);
            dependencies.Cache(userProfile);
            dependencies.Cache(musicController);
            dependencies.Cache(notificationManager);
            dependencies.Cache(dialogOverlay);

            // ensure both overlays aren't presented at the same time
            chat.StateChanged += (container, state) => social.State = state == Visibility.Visible ? Visibility.Hidden : social.State;
            social.StateChanged += (container, state) => chat.State = state == Visibility.Visible ? Visibility.Hidden : chat.State;

            LoadComponentAsync(Toolbar = new Toolbar
            {
                Depth = -4,
                OnHome = delegate { intro?.ChildScreen?.MakeCurrent(); },
            }, overlayContent.Add);

            settings.StateChanged += delegate
            {
                switch (settings.State)
                {
                    case Visibility.Hidden:
                        intro.MoveToX(0, SettingsOverlay.TRANSITION_LENGTH, Easing.OutQuint);
                        break;
                    case Visibility.Visible:
                        intro.MoveToX(SettingsOverlay.SIDEBAR_WIDTH / 2, SettingsOverlay.TRANSITION_LENGTH, Easing.OutQuint);
                        break;
                }
            };

            Cursor.State = Visibility.Hidden;
        }

        private bool globalHotkeyPressed(InputState state, KeyDownEventArgs args)
        {
            if (args.Repeat || intro == null) return false;

            switch (args.Key)
            {
                case Key.F8:
                    chat.ToggleVisibility();
                    return true;
                case Key.F9:
                    social.ToggleVisibility();
                    return true;
                case Key.PageUp:
                case Key.PageDown:
                    var swClock = (Clock as ThrottledFrameClock)?.Source as StopwatchClock;
                    if (swClock == null) return false;

                    swClock.Rate *= args.Key == Key.PageUp ? 1.1f : 0.9f;
                    Logger.Log($@"Adjusting game clock to {swClock.Rate}", LoggingTarget.Debug);
                    return true;
            }

            if (state.Keyboard.ControlPressed)
            {
                switch (args.Key)
                {
                    case Key.R:
                        if (state.Keyboard.AltPressed)
                        {
                            var sensitivity = frameworkConfig.GetBindable<double>(FrameworkSetting.CursorSensitivity);

                            sensitivity.Disabled = false;
                            sensitivity.Value = 1;
                            sensitivity.Disabled = true;

                            frameworkConfig.Set(FrameworkSetting.ActiveInputHandlers, string.Empty);
                            return true;
                        }
                        break;
                    case Key.T:
                        Toolbar.ToggleVisibility();
                        return true;
                    case Key.O:
                        settings.ToggleVisibility();
                        return true;
                    case Key.D:
                        if (state.Keyboard.ShiftPressed || state.Keyboard.AltPressed)
                            return false;

                        direct.ToggleVisibility();
                        return true;
                }
            }

            return false;
        }

        public event Action<Screen> ScreenChanged;

        private Container mainContent;

        private Container overlayContent;

        private OsuScreen currentScreen;
        private FrameworkConfigManager frameworkConfig;

        private void screenChanged(Screen newScreen)
        {
            currentScreen = newScreen as OsuScreen;

            if (currentScreen == null)
            {
                Exit();
                return;
            }

            //central game screen change logic.
            if (!currentScreen.ShowOverlays)
            {
                settings.State = Visibility.Hidden;
                Toolbar.State = Visibility.Hidden;
                musicController.State = Visibility.Hidden;
                chat.State = Visibility.Hidden;
                direct.State = Visibility.Hidden;
                social.State = Visibility.Hidden;
                userProfile.State = Visibility.Hidden;
            }
            else
            {
                Toolbar.State = Visibility.Visible;
            }

            ScreenChanged?.Invoke(newScreen);
        }

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

            mainContent.Padding = new MarginPadding { Top = ToolbarOffset };

            Cursor.State = currentScreen?.HasLocalCursorDisplayed == false ? Visibility.Visible : Visibility.Hidden;
        }

        private void screenAdded(Screen newScreen)
        {
            newScreen.ModePushed += screenAdded;
            newScreen.Exited += screenRemoved;

            screenChanged(newScreen);
        }

        private void screenRemoved(Screen newScreen)
        {
            screenChanged(newScreen);
        }
    }
}
