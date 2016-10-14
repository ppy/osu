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
using osu.Framework;
using osu.Framework.Input;
using osu.Game.Input;
using OpenTK.Input;
using System.IO;
using osu.Game.Beatmaps.IO;

namespace osu.Game
{
    public class OsuGame : OsuGameBase
    {
        private class ImportBeatmap
        {
            public string Path;
        }

        public Toolbar Toolbar;
        public ChatConsole Chat;
        public MainMenu MainMenu => intro?.ChildGameMode as MainMenu;
        private Intro intro;
        private string[] args;
        private IpcChannel<ImportBeatmap> BeatmapIPC;

        public Bindable<PlayMode> PlayMode;

        public OsuGame(string[] args)
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
            BeatmapIPC = new IpcChannel<ImportBeatmap>(Host);

            if (!Host.IsPrimaryInstance)
            {
                if (args.Length == 1 && File.Exists(args[0]))
                {
                    BeatmapIPC.SendMessage(new ImportBeatmap { Path = args[0] }).Wait();
                    Console.WriteLine(@"Sent file to running instance");
                }
                else
                    Console.WriteLine(@"osu! does not support multiple running instances.");
                Environment.Exit(0);
            }

            BeatmapIPC.MessageReceived += message =>
            {
                try
                {
                    Beatmaps.AddBeatmap(message.Path);
                    // TODO: Switch to beatmap list and select the new song
                }
                catch (Exception ex)
                {
                    // TODO: Show the user some info?
                    Console.WriteLine($@"Failed to import beatmap: {ex}");
                }
            };

            base.Load(game);

            //attach our bindables to the audio subsystem.
            Audio.Volume.Weld(Config.GetBindable<double>(OsuConfig.VolumeGlobal));
            Audio.VolumeSample.Weld(Config.GetBindable<double>(OsuConfig.VolumeEffect));
            Audio.VolumeTrack.Weld(Config.GetBindable<double>(OsuConfig.VolumeMusic));

            Add(new Drawable[] {
                intro = new Intro(),
                Toolbar = new Toolbar
                {
                    OnHome = delegate { MainMenu?.MakeCurrent(); },
                    OnSettings = Options.ToggleVisibility,
                    OnPlayModeChange = delegate (PlayMode m) { PlayMode.Value = m; },
                },
                Chat = new ChatConsole(API),
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
                Config.Set(OsuConfig.Width, Size.X);
                Config.Set(OsuConfig.Height, Size.Y);
            }
            return true;
        }
    }
}
