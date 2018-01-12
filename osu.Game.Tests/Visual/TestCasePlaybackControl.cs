// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Screens.Edit.Components;
using osu.Game.Tests.Beatmaps;
using OpenTK;

namespace osu.Game.Tests.Visual
{
    public class TestCasePlaybackControl : OsuTestCase
    {
        public TestCasePlaybackControl()
        {
            var playback = new PlaybackControl
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Size = new Vector2(200,100)
            };
            playback.Beatmap.Value = new TestWorkingBeatmap(new Beatmap());

            Add(playback);
        }
    }
}
