// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Timing;
using osu.Game.Beatmaps;
using osu.Game.Screens.Edit.Components;
using osu.Game.Tests.Beatmaps;
using OpenTK;

namespace osu.Game.Tests.Visual
{
    [TestFixture]
    public class TestCasePlaybackControl : OsuTestCase
    {
        [BackgroundDependencyLoader]
        private void load()
        {
            var clock = new DecoupleableInterpolatingFramedClock { IsCoupled = false };
            Dependencies.CacheAs<IAdjustableClock>(clock);
            Dependencies.CacheAs<IFrameBasedClock>(clock);

            var playback = new PlaybackControl
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Size = new Vector2(200,100)
            };

            Beatmap.Value = new TestWorkingBeatmap(new Beatmap());

            Child = playback;
        }
    }
}
