//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Audio.Sample;
using osu.Framework.Audio.Track;
using osu.Framework.GameModes;

namespace osu.Game.GameModes.Menu
{
    class Intro : OsuGameMode
    {
        public override void Load()
        {
            base.Load();

            AudioSample welcome = Game.Audio.Sample.Get(@"welcome");
            welcome.Play();

            AudioTrack bgm = Game.Audio.Track.Get(@"circles");
            bgm.Looping = true;

            Game.Scheduler.AddDelayed(delegate
            {
                bgm.Start();
            }, 600);

            Game.Scheduler.AddDelayed(delegate
            {
                Push(new MainMenu());
            }, 2900);
        }
    }
}
