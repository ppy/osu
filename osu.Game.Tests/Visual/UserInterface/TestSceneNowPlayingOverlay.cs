// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Timing;
using osu.Game.Beatmaps;
using osu.Game.Overlays;

namespace osu.Game.Tests.Visual.UserInterface
{
    [TestFixture]
    public class TestSceneNowPlayingOverlay : OsuTestScene
    {
        [Cached]
        private MusicController musicController = new MusicController();

        private WorkingBeatmap currentTrack;

        public TestSceneNowPlayingOverlay()
        {
            Clock = new FramedClock();

            var np = new NowPlayingOverlay
            {
                Origin = Anchor.Centre,
                Anchor = Anchor.Centre
            };

            Add(musicController);
            Add(np);

            AddStep(@"show", () => np.Show());
            AddToggleStep(@"toggle beatmap lock", state => Beatmap.Disabled = state);
            AddStep(@"hide", () => np.Hide());
        }

        [Test]
        public void TestPrevTrackBehavior()
        {
            AddStep(@"Play track", () =>
            {
                musicController.NextTrack();
                currentTrack = Beatmap.Value;
            });

            AddStep(@"Seek track to 6 second", () => musicController.SeekTo(6000));
            AddAssert(@"Check action is restart track", () => musicController.PreviousTrack() == PreviousTrackResult.Restart);
            AddAssert(@"Check track didn't change", () => currentTrack == Beatmap.Value);

            AddStep(@"Seek track to 2 second", () => musicController.SeekTo(2000));
            AddAssert(@"Check action is previous track", () => musicController.PreviousTrack() == PreviousTrackResult.Previous);
        }
    }
}
