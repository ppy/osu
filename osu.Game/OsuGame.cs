//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Configuration;
using osu.Framework.GameModes;
using osu.Game.Configuration;
using osu.Game.GameModes.Menu;
using OpenTK;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Platform;
using osu.Game.GameModes;
using osu.Game.Graphics.Background;
using osu.Game.GameModes.Play;
using osu.Game.Graphics.Containers;
using osu.Game.Overlays;
using osu.Framework.Input;
using osu.Game.Input;
using OpenTK.Input;

namespace osu.Game
{
    public class OsuGame : OsuGameBase
    {
        public Toolbar Toolbar;
        public ChatConsole Chat;
        public MainMenu MainMenu => intro?.ChildGameMode as MainMenu;
        private Intro intro;

        public Bindable<PlayMode> PlayMode;

        public override void SetHost(BasicGameHost host)
        {
            base.SetHost(host);

            host.Size = new Vector2(Config.Get<int>(OsuConfig.Width), Config.Get<int>(OsuConfig.Height));
        }

        public override void Load()
        {
            base.Load();

            //attach our bindables to the audio subsystem.
            Audio.Volume.Weld(Config.GetBindable<double>(OsuConfig.VolumeGlobal));
            Audio.VolumeSample.Weld(Config.GetBindable<double>(OsuConfig.VolumeEffect));
            Audio.VolumeTrack.Weld(Config.GetBindable<double>(OsuConfig.VolumeMusic));

            Add(new Drawable[] {
                intro = new Intro(),
                Toolbar = new Toolbar
                {
                    OnHome = delegate { MainMenu?.MakeCurrent(); },
                    OnSettings = delegate { Options.PoppedOut = !Options.PoppedOut; },
                    OnPlayModeChange = delegate (PlayMode m) { PlayMode.Value = m; },
                    Alpha = 0.001f //fixes invalidation fuckup
                },
                Chat = new ChatConsole(),
                new VolumeControl
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

            Toolbar.State = ToolbarState.Hidden;
            Toolbar.Flush();

            Chat.State = ChatConsoleState.Hidden;
            Chat.Flush();

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
                    Chat.State = Chat.State == ChatConsoleState.Hidden ? ChatConsoleState.Visible : ChatConsoleState.Hidden;
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
                Toolbar.State = ToolbarState.Hidden;
                Chat.State = ChatConsoleState.Hidden;
            }
            else
            {
                Toolbar.State = ToolbarState.Visible;
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
                Config.Set(OsuConfig.Width, Size.X);
                Config.Set(OsuConfig.Height, Size.Y);
            }
            return true;
        }
    }
}
