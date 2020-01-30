// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Timing;
using osu.Game.Beatmaps;
using osu.Game.Screens.Edit.Components;
using osuTK;

namespace osu.Game.Tests.Visual.Editor
{
    [TestFixture]
    public class TestScenePlaybackControl : OsuTestScene
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
                Size = new Vector2(200, 100)
            };

            Beatmap.Value = CreateWorkingBeatmap(new Beatmap());

            Child = playback;
        }
    }
}
