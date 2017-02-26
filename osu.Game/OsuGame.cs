// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Configuration;
using osu.Framework.Screens;
using osu.Game.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Overlays;
using osu.Framework.Input;
using osu.Game.Input;
using OpenTK.Input;
using osu.Framework.Logging;
using osu.Game.Graphics.UserInterface.Volume;
using osu.Game.Database;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Transformations;
using osu.Framework.Timing;
using osu.Game.Modes;
using osu.Game.Overlays.Toolbar;
using osu.Game.Screens;
using osu.Game.Screens.Menu;
using OpenTK;
using System.Linq;
using osu.Framework.Graphics.Primitives;
using System.Collections.Generic;
using System.Threading.Tasks;
using osu.Game.Overlays.Notifications;

namespace osu.Game
{
    public class OsuGame : OsuGameBase
    {
        public virtual bool IsDeployedBuild => false;

        public Toolbar Toolbar;

        private ChatOverlay chat;

        private MusicController musicController;

        private NotificationManager notificationManager;

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

        private OsuScreen screenStack;

        private VolumeControl volume;

        public Bindable<PlayMode> PlayMode;

        string[] args;

        private OptionsOverlay options;

        public OsuGame(string[] args = null)
        {
            this.args = args;
        }

        public void ToggleOptions() => options.ToggleVisibility();

        [BackgroundDependencyLoader]
        private void load()
        {
            if (!Host.IsPrimaryInstance)
            {
                Logger.Log(@"osu! does not support multiple running instances.", LoggingTarget.Runtime, LogLevel.Error);
                Environment.Exit(0);
            }

            if (args?.Length > 0)
            {
                var paths = args.Where(a => !a.StartsWith(@"-"));
                ImportBeatmapsAsync(paths);
            }

            Dependencies.Cache(this);

            PlayMode = LocalConfig.GetBindable<PlayMode>(OsuConfig.PlayMode);
        }

        protected async void ImportBeatmapsAsync(IEnumerable<string> paths)
        {
            await Task.Run(() => BeatmapDatabase.Import(paths));
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Add(new Drawable[] {
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
                overlayContent = new Container{ RelativeSizeAxes = Axes.Both },
                new GlobalHotkeys //exists because UserInputManager is at a level below us.
                {
                    Handler = globalHotkeyPressed
                }
            });

            (screenStack = new Loader()).LoadAsync(this, d =>
            {
                screenStack.ModePushed += screenAdded;
                screenStack.Exited += screenRemoved;
                mainContent.Add(screenStack);
            });

            //overlay elements
            (chat = new ChatOverlay { Depth = 0 }).LoadAsync(this, overlayContent.Add);
            (options = new OptionsOverlay { Depth = -1 }).LoadAsync(this, overlayContent.Add);
            (musicController = new MusicController()
            {
                Depth = -2,
                Position = new Vector2(0, Toolbar.HEIGHT),
                Anchor = Anchor.TopRight,
                Origin = Anchor.TopRight,
            }).LoadAsync(this, overlayContent.Add);

            (notificationManager = new NotificationManager
            {
                Depth = -2,
                Anchor = Anchor.TopRight,
                Origin = Anchor.TopRight,
            }).LoadAsync(this, overlayContent.Add);

            Logger.NewEntry += entry =>
            {
                if (entry.Level < LogLevel.Important) return;

                notificationManager.Post(new SimpleNotification
                {
                    Text = $@"{entry.Level}: {entry.Message}"
                });
            };

            Dependencies.Cache(options);
            Dependencies.Cache(musicController);
            Dependencies.Cache(notificationManager);

            (Toolbar = new Toolbar
            {
                Depth = -3,
                OnHome = delegate { intro?.ChildScreen?.MakeCurrent(); },
                OnPlayModeChange = delegate (PlayMode m) { PlayMode.Value = m; },
            }).LoadAsync(this, t =>
            {
                PlayMode.ValueChanged += delegate { Toolbar.SetGameMode(PlayMode.Value); };
                PlayMode.TriggerChange();
                overlayContent.Add(Toolbar);
            });

            options.StateChanged += delegate
            {
                switch (options.State)
                {
                    case Visibility.Hidden:
                        intro.MoveToX(0, OptionsOverlay.TRANSITION_LENGTH, EasingTypes.OutQuint);
                        break;
                    case Visibility.Visible:
                        intro.MoveToX(OptionsOverlay.SIDEBAR_WIDTH / 2, OptionsOverlay.TRANSITION_LENGTH, EasingTypes.OutQuint);
                        break;
                }
            };

            Cursor.Alpha = 0;
        }

        private bool globalHotkeyPressed(InputState state, KeyDownEventArgs args)
        {
            if (args.Repeat) return false;

            switch (args.Key)
            {
                case Key.F8:
                    chat.ToggleVisibility();
                    return true;
                case Key.PageUp:
                case Key.PageDown:
                    var rate = ((Clock as ThrottledFrameClock).Source as StopwatchClock).Rate * (args.Key == Key.PageUp ? 1.1f : 0.9f);
                    ((Clock as ThrottledFrameClock).Source as StopwatchClock).Rate = rate;
                    Logger.Log($@"Adjusting game clock to {rate}", LoggingTarget.Debug);
                    return true;
            }

            if (state.Keyboard.ControlPressed)
            {
                switch (args.Key)
                {
                    case Key.T:
                        Toolbar.ToggleVisibility();
                        return true;
                    case Key.O:
                        options.ToggleVisibility();
                        return true;
                }
            }

            return base.OnKeyDown(state, args);
        }

        public event Action<Screen> ModeChanged;

        private Container mainContent;

        private Container overlayContent;

        private void modeChanged(Screen newScreen)
        {
            //central game mode change logic.
            if ((newScreen as OsuScreen)?.ShowOverlays != true)
            {
                Toolbar.State = Visibility.Hidden;
                musicController.State = Visibility.Hidden;
                chat.State = Visibility.Hidden;
            }
            else
            {
                Toolbar.State = Visibility.Visible;
            }

            if (newScreen is MainMenu)
                Cursor.FadeIn(100);

            ModeChanged?.Invoke(newScreen);

            if (newScreen == null)
                Exit();
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

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();

            if (intro?.ChildScreen != null)
                intro.ChildScreen.Padding = new MarginPadding { Top = Toolbar.Position.Y + Toolbar.DrawHeight };
        }

        private void screenAdded(Screen newScreen)
        {
            newScreen.ModePushed += screenAdded;
            newScreen.Exited += screenRemoved;

            modeChanged(newScreen);
        }

        private void screenRemoved(Screen newScreen)
        {
            modeChanged(newScreen);
        }
    }
}
