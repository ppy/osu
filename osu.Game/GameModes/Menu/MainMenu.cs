//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Audio.Sample;
using osu.Framework.Audio.Track;
using osu.Framework.GameModes;
using osu.Framework.GameModes.Testing;
using osu.Framework.Graphics;
using osu.Game.GameModes.Charts;
using osu.Game.GameModes.Direct;
using osu.Game.GameModes.Edit;
using osu.Game.GameModes.Multiplayer;
using osu.Game.GameModes.Play;
using osu.Game.Graphics.Containers;

namespace osu.Game.GameModes.Menu
{
    internal class MainMenu : OsuGameMode
    {
        public override string Name => @"Main Menu";

        //private AudioTrackBass bgm;

        public override void Load()
        {
            base.Load();

            AudioSample welcome = Game.Audio.Sample.Get(@"welcome");
            welcome.Play();

            Children = new Drawable[]
            {
                new ButtonSystem()
                {
                    OnChart = delegate { Push(new ChartListing()); },
                    OnDirect = delegate { Push(new OnlineListing()); },
                    OnEdit = delegate { Push(new SongSelectEdit()); },
                    OnSolo = delegate { Push(new SongSelectPlay()); },
                    OnMulti = delegate { Push(new Lobby()); },
                    OnTest  = delegate { Push(new TestBrowser()); },
                    OnExit = delegate {
                        Game.Scheduler.AddDelayed(delegate {
                            Game.Host.Exit();
                        }, ButtonSystem.EXIT_DELAY);
                    },
                    OnSettings = delegate {
                        Game.Options.PoppedOut = !Game.Options.PoppedOut;
                    },
                }
            };
        }
    }
}
