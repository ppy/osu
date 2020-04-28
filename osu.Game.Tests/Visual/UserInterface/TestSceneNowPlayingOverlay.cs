// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Overlays;
using osu.Game.Rulesets.Osu;

namespace osu.Game.Tests.Visual.UserInterface
{
    [TestFixture]
    public class TestSceneNowPlayingOverlay : OsuTestScene
    {
        [Cached]
        private MusicController musicController = new MusicController();

        private WorkingBeatmap currentBeatmap;

        private NowPlayingOverlay nowPlayingOverlay;

        [BackgroundDependencyLoader]
        private void load()
        {
            Beatmap.Value = CreateWorkingBeatmap(new OsuRuleset().RulesetInfo);

            nowPlayingOverlay = new NowPlayingOverlay
            {
                Origin = Anchor.Centre,
                Anchor = Anchor.Centre
            };

            Add(musicController);
            Add(nowPlayingOverlay);
        }

        [Test]
        public void TestShowHideDisable()
        {
            AddStep(@"show", () => nowPlayingOverlay.Show());
            AddToggleStep(@"toggle beatmap lock", state => Beatmap.Disabled = state);
            AddStep(@"hide", () => nowPlayingOverlay.Hide());
        }

        [Resolved]
        private BeatmapManager manager { get; set; }

        [Test]
        public void TestPrevTrackBehavior()
        {
            // ensure we have at least two beatmaps available.
            AddRepeatStep("import beatmap", () => manager.Import(new BeatmapSetInfo
            {
                Beatmaps = new List<BeatmapInfo>
                {
                    new BeatmapInfo
                    {
                        BaseDifficulty = new BeatmapDifficulty(),
                    }
                },
                Metadata = new BeatmapMetadata
                {
                    Artist = "a test map",
                    Title = "title",
                }
            }).Wait(), 5);

            AddStep(@"Next track", () => musicController.NextTrack());
            AddStep("Store track", () => currentBeatmap = Beatmap.Value);

            AddStep(@"Seek track to 6 second", () => musicController.SeekTo(6000));
            AddUntilStep(@"Wait for current time to update", () => currentBeatmap.Track.CurrentTime > 5000);

            AddStep(@"Set previous", () => musicController.PreviousTrack());

            AddAssert(@"Check beatmap didn't change", () => currentBeatmap == Beatmap.Value);
            AddUntilStep("Wait for current time to update", () => currentBeatmap.Track.CurrentTime < 5000);

            AddStep(@"Set previous", () => musicController.PreviousTrack());
            AddAssert(@"Check beatmap did change", () => currentBeatmap != Beatmap.Value);
        }
    }
}
