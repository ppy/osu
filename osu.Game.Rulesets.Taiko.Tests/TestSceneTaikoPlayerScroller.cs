// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Taiko.Skinning.Legacy;
using osu.Game.Storyboards;
using osu.Game.Tests.Visual;
using osuTK;

namespace osu.Game.Rulesets.Taiko.Tests
{
    public partial class TestSceneTaikoPlayerScroller : LegacySkinPlayerTestScene
    {
        private Storyboard? currentStoryboard;

        protected override bool HasCustomSteps => true;

        [Test]
        public void TestForegroundSpritesHidesScroller()
        {
            AddStep("load storyboard", () =>
            {
                currentStoryboard = new Storyboard();

                for (int i = 0; i < 10; i++)
                    currentStoryboard.GetLayer("Foreground").Add(new StoryboardSprite($"test{i}", Anchor.Centre, Vector2.Zero));
            });

            CreateTest();
            AddAssert("taiko scroller not present", () => !this.ChildrenOfType<LegacyTaikoScroller>().Any());
        }

        [Test]
        public void TestOverlaySpritesKeepsScroller()
        {
            AddStep("load storyboard", () =>
            {
                currentStoryboard = new Storyboard();

                for (int i = 0; i < 10; i++)
                    currentStoryboard.GetLayer("Overlay").Add(new StoryboardSprite($"test{i}", Anchor.Centre, Vector2.Zero));
            });

            CreateTest();
            AddAssert("taiko scroller present", () => this.ChildrenOfType<LegacyTaikoScroller>().Single().IsPresent);
        }

        protected override Ruleset CreatePlayerRuleset() => new TaikoRuleset();

        protected override WorkingBeatmap CreateWorkingBeatmap(IBeatmap beatmap, Storyboard? storyboard = null)
            => base.CreateWorkingBeatmap(beatmap, currentStoryboard ?? storyboard);
    }
}
