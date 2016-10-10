//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Configuration;
using osu.Framework.GameModes;
using osu.Game.Configuration;
using osu.Game.GameModes.Menu;
using OpenTK;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Platform;
using osu.Game.GameModes;
using osu.Game.Graphics.Background;
using osu.Game.GameModes.Play;
using osu.Game.Graphics.Containers;
using osu.Game.Overlays;

namespace osu.Game
{
    public class OsuGame : OsuGameBase
    {
        public Toolbar Toolbar;
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
                    Alpha = 0.001f,
                },
                new VolumeControl
                {
                    VolumeGlobal = Audio.Volume,
                    VolumeSample = Audio.VolumeSample,
                    VolumeTrack = Audio.VolumeTrack
                }
            });

            Toolbar.SetState(ToolbarState.Hidden, true);

            intro.ModePushed += modeAdded;
            intro.Exited += modeRemoved;

            PlayMode = Config.GetBindable<PlayMode>(OsuConfig.PlayMode);
            PlayMode.ValueChanged += delegate { Toolbar.SetGameMode(PlayMode.Value); };
            PlayMode.TriggerChange();

            Cursor.Alpha = 0;
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
                Toolbar.SetState(ToolbarState.Hidden);
            }
            else
            {
                Toolbar.SetState(ToolbarState.Visible);
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
                intro.MakeCurrent();
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
