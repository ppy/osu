//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Threading;
using osu.Framework.Configuration;
using osu.Framework.GameModes;
using osu.Game.Configuration;
using OpenTK;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Platform;
using osu.Game.Overlays;
using osu.Framework;
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
using osu.Game.Screens.Play;

namespace osu.Game
{
    public class OsuGame : OsuGameBase
    {
        public Toolbar Toolbar;

        private ChatOverlay chat;

        private MusicController musicController;

        private MainMenu mainMenu => modeStack?.ChildGameMode as MainMenu;
        private Intro intro => modeStack as Intro;

        private OsuGameMode modeStack;

        private VolumeControl volume;

        public Bindable<PlayMode> PlayMode;

        string[] args;

        private OptionsOverlay options;

        public OsuGame(string[] args = null)
        {
            this.args = args;
        }

        public override void SetHost(BasicGameHost host)
        {
            base.SetHost(host);

            host.Size = new Vector2(Config.Get<int>(OsuConfig.Width), Config.Get<int>(OsuConfig.Height));
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
                Schedule(delegate { Dependencies.Get<BeatmapDatabase>().Import(args); });

            Dependencies.Cache(this);

            //attach our bindables to the audio subsystem.
            Audio.Volume.Weld(Config.GetBindable<double>(OsuConfig.VolumeUniversal));
            Audio.VolumeSample.Weld(Config.GetBindable<double>(OsuConfig.VolumeEffect));
            Audio.VolumeTrack.Weld(Config.GetBindable<double>(OsuConfig.VolumeMusic));

            PlayMode = Config.GetBindable<PlayMode>(OsuConfig.PlayMode);
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

            (modeStack = new Intro()).Preload(this, d =>
            {
                mainContent.Add(d);

                modeStack.ModePushed += modeAdded;
                modeStack.Exited += modeRemoved;
                modeStack.DisplayAsRoot();
            });

            //overlay elements
            (chat = new ChatOverlay { Depth = 0 }).Preload(this, overlayContent.Add);
            (options = new OptionsOverlay { Depth = -1 }).Preload(this, overlayContent.Add);
            (musicController = new MusicController() { Depth = -3 }).Preload(this, overlayContent.Add);

            Dependencies.Cache(options);
            Dependencies.Cache(musicController);

            (Toolbar = new Toolbar
            {
                Depth = -2,
                OnHome = delegate { mainMenu?.MakeCurrent(); },
                OnPlayModeChange = delegate (PlayMode m) { PlayMode.Value = m; },
            }).Preload(this, t =>
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
                    case Key.O:
                        options.ToggleVisibility();
                        return true;
                }
            }

            return base.OnKeyDown(state, args);
        }

        public Action<GameMode> ModeChanged;

        private Container mainContent;

        private Container overlayContent;

        private void modeChanged(GameMode newMode)
        {
            // - Ability to change window size
            // - Ability to adjust music playback
            // - Frame limiter changes

            //central game mode change logic.
            if ((newMode as OsuGameMode)?.ShowOverlays != true)
            {
                Toolbar.State = Visibility.Hidden;
                musicController.State = Visibility.Hidden;
                chat.State = Visibility.Hidden;
            }
            else
            {
                Toolbar.State = Visibility.Visible;
            }

            Cursor.FadeIn(100);

            ModeChanged?.Invoke(newMode);

            if (newMode == null)
                Host.Exit();
        }

        protected override bool OnExiting()
        {
            if (!intro.DidLoadMenu || intro.ChildGameMode != null)
            {
                Scheduler.Add(delegate
                {
                    intro.MakeCurrent();
                });
                return true;
            }

            return base.OnExiting();
        }

        private void modeAdded(GameMode newMode)
        {
            newMode.ModePushed += modeAdded;
            newMode.Exited += modeRemoved;

            modeChanged(newMode);
        }

        private void modeRemoved(GameMode newMode)
        {
            modeChanged(newMode);
        }

        public override bool Invalidate(Invalidation invalidation = Invalidation.All, Drawable source = null, bool shallPropagate = true)
        {
            if (!base.Invalidate(invalidation, source, shallPropagate)) return false;

            if (Parent != null)
            {
                Config.Set(OsuConfig.Width, DrawSize.X);
                Config.Set(OsuConfig.Height, DrawSize.Y);
            }
            return true;
        }
    }
}
