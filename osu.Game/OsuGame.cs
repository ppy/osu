﻿//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Configuration;
using osu.Framework.GameModes;
using osu.Game.Configuration;
using osu.Game.GameModes.Menu;
using OpenTK;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Platform;
using osu.Game.GameModes.Play;
using osu.Game.Overlays;
using osu.Framework;
using osu.Framework.Input;
using osu.Game.Input;
using OpenTK.Input;
using osu.Framework.Logging;
using osu.Game.Graphics.UserInterface.Volume;
using osu.Game.Online;

namespace osu.Game
{
    public class OsuGame : OsuGameBase
    {
        public LocalUser LocalUser;
        public Toolbar Toolbar;
        public ChatConsole Chat;
        public MainMenu MainMenu => intro?.ChildGameMode as MainMenu;
        private Intro intro;

        private VolumeControl volume;

        public Bindable<PlayMode> PlayMode;

        string[] args;

        public OsuGame(string[] args = null)
        {
            this.args = args;
        }

        public override void SetHost(BasicGameHost host)
        {
            base.SetHost(host);

            host.Size = new Vector2(Config.Get<int>(OsuConfig.Width), Config.Get<int>(OsuConfig.Height));
        }

        public override void Load(BaseGame game)
        {
            if (!Host.IsPrimaryInstance)
            {
                Logger.Log(@"osu! does not support multiple running instances.", LoggingTarget.Runtime, LogLevel.Error);
                Environment.Exit(0);
            }

            base.Load(game);

            LocalUser = new LocalUser(API);
            LocalUser.CheckUser();
            Scheduler.AddDelayed(delegate {
                LocalUser.CheckUser();
                Toolbar.toolbarUserButton.UpdateButton(LocalUser);
            }, 10000, true);

            if (args?.Length > 0)
                Schedule(delegate { Beatmaps.Import(args); });

            //todo: Some intelligent comment
            LocalUser = new LocalUser(API);
            LocalUser.CheckUser();
            Scheduler.AddDelayed(delegate {
                LocalUser.CheckUser();
                Toolbar.toolbarUserButton.UpdateButton(LocalUser);
                //Debug.Write("Checked!");
            }, 10000, true);

            //attach our bindables to the audio subsystem.
            Audio.Volume.Weld(Config.GetBindable<double>(OsuConfig.VolumeGlobal));
            Audio.VolumeSample.Weld(Config.GetBindable<double>(OsuConfig.VolumeEffect));
            Audio.VolumeTrack.Weld(Config.GetBindable<double>(OsuConfig.VolumeMusic));

            Add(new Drawable[] {
                new VolumeControlReceptor
                {
                    RelativeSizeAxes = Axes.Both,
                    ActivateRequested = delegate { volume.Show(); }
                },
                intro = new Intro(),
                Toolbar = new Toolbar
                {
                    OnHome = delegate { MainMenu?.MakeCurrent(); },
                    OnSettings = Options.ToggleVisibility,
                    OnPlayModeChange = delegate (PlayMode m) { PlayMode.Value = m; },
                },
                Chat = new ChatConsole(API),
                volume = new VolumeControl
                {
                    VolumeGlobal = Audio.Volume,
                    VolumeSample = Audio.VolumeSample,
                    VolumeTrack = Audio.VolumeTrack
                },
                new GlobalHotkeys //exists because UserInputManager is at a level below us.
                {
                    Handler = globalHotkeyPressed
                }
            });
            Toolbar.toolbarUserButton.UpdateButton(LocalUser);

            intro.ModePushed += modeAdded;
            intro.Exited += modeRemoved;

            PlayMode = Config.GetBindable<PlayMode>(OsuConfig.PlayMode);
            PlayMode.ValueChanged += delegate { Toolbar.SetGameMode(PlayMode.Value); };
            PlayMode.TriggerChange();

            Cursor.Alpha = 0;
        }

        private bool globalHotkeyPressed(InputState state, KeyDownEventArgs args)
        {
            switch (args.Key)
            {
                case Key.F8:
                    Chat.ToggleVisibility();
                    return true;
            }

            return base.OnKeyDown(state, args);
        }

        public Action<GameMode> ModeChanged;

        private void modeChanged(GameMode newMode)
        {
            // - Ability to change window size
            // - Ability to adjust music playback
            // - Frame limiter changes

            //central game mode change logic.
            if (newMode is Player || newMode is Intro)
            {
                Toolbar.State = Visibility.Hidden;
                Chat.State = Visibility.Hidden;
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
