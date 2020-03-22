// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Utils;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Overlays;
using osu.Game.Rulesets.Osu;
using osu.Framework.Graphics.Containers;
using System.Linq;
using System;

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

        [Test]
        public void TestPrevTrackBehavior()
        {
            AddStep(@"Play track", () =>
            {
                musicController.NextTrack();
                currentBeatmap = Beatmap.Value;
            });

            AddStep(@"Seek track to 6 second", () => musicController.SeekTo(6000));
            AddUntilStep(@"Wait for current time to update", () => currentBeatmap.Track.CurrentTime > 5000);
            AddAssert(@"Check action is restart track", () => musicController.PreviousTrack() == PreviousTrackResult.Restart);
            AddUntilStep("Wait for current time to update", () => Precision.AlmostEquals(currentBeatmap.Track.CurrentTime, 0));
            AddAssert(@"Check track didn't change", () => currentBeatmap == Beatmap.Value);
            AddAssert(@"Check action is not restart", () => musicController.PreviousTrack() != PreviousTrackResult.Restart);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void TestBackgroundNotScrolling(bool goNext)
        {
            AddAssert("More than one in playlist", () => musicController.BeatmapSets.Count() > 1);

            BufferedContainer currentBackground = null;
            float currentX = 0;
            void step()
            {
                AddStep(@"hide", () => nowPlayingOverlay.Hide());
                AddUntilStep("Is hidden", () => nowPlayingOverlay.Alpha == 0);

                AddStep("Assign current background", () => currentBackground = getFirstBackground());
                if (goNext)
                    AddStep("next track", () => musicController.NextTrack());
                else
                    AddUntilStep("previous track", () => musicController.PreviousTrack() != PreviousTrackResult.Restart);

                AddStep(@"show", () => nowPlayingOverlay.Show());
            }

            step();
            AddUntilStep("Background changed", () => getFirstBackground() != currentBackground);
            step();
            AddAssert("Background count is 1", () => nowPlayingOverlay.ChildrenOfType<BufferedContainer>().Count() == 1);
            step();
            AddAssert("New background is present", () => getFirstBackground().IsPresent);
            step();
            AddStep("Get new background position", () => currentX = getFirstBackground().X);
            AddAssert("New background didn't scroll", () => currentX == getFirstBackground().X);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void TestBackgroundScrolling(bool goNext)
        {
            AddAssert("More than one in playlist", () => musicController.BeatmapSets.Count() > 1);
            AddStep(@"show", () => nowPlayingOverlay.Show());
            AddUntilStep("Is Visible", () => nowPlayingOverlay.Alpha > 0);

            BufferedContainer currentBackground = null;
            float currentX = 0;
            void step()
            {
                AddStep("Assign current background", () => currentBackground = getFirstBackground());
                if (goNext)
                    AddStep("next track", () => musicController.NextTrack());
                else
                    AddUntilStep("previous track", () => musicController.PreviousTrack() != PreviousTrackResult.Restart);
            }

            step();
            AddUntilStep("Background changed", () => getFirstBackground() != currentBackground);
            step();
            AddAssert("Background count is 2", () => nowPlayingOverlay.ChildrenOfType<BufferedContainer>().Count() == 2);
            step();
            AddAssert("Previous background is present", () => getSecondBackground().IsPresent);
            step();
            AddAssert("New background is present", () => getFirstBackground().IsPresent);
            step();
            AddStep("Get new background position", () => currentX = getFirstBackground().X);
            AddUntilStep("New background scrolled", () => currentX != getFirstBackground().X);
            step();
            AddStep("Get previous background position", () => currentX = getSecondBackground().X);
            AddUntilStep("Previous background scrolled", () => currentX != getSecondBackground().X);
        }

        private BufferedContainer getFirstBackground()
        {
            return nowPlayingOverlay.ChildrenOfType<BufferedContainer>().First();
        }
        private BufferedContainer getSecondBackground()
        {
            return nowPlayingOverlay.ChildrenOfType<BufferedContainer>().Skip(1).First();
        }
    }
}
