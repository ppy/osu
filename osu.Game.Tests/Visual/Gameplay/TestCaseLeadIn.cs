// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Game.Beatmaps;
using osu.Game.Graphics.Sprites;
using osu.Game.Rulesets;
using osu.Game.Screens.Play;
using osu.Game.Tests.Beatmaps;
using osuTK.Graphics;

namespace osu.Game.Tests.Visual.Gameplay
{
    public class TestCaseLeadIn : RateAdjustedBeatmapTestCase
    {
        private Ruleset ruleset;

        private LeadInPlayer player;

        [BackgroundDependencyLoader]
        private void load(RulesetStore rulesets)
        {
            Add(new Box
            {
                RelativeSizeAxes = Axes.Both,
                Colour = Color4.Black,
                Depth = int.MaxValue
            });

            ruleset = rulesets.AvailableRulesets.First().CreateInstance();
        }

        [Test]
        public void TestShortLeadIn()
        {
            AddStep("create player", () => loadPlayerWithBeatmap(new TestBeatmap(ruleset.RulesetInfo) { BeatmapInfo = { AudioLeadIn = 1000 } }));
            AddUntilStep("correct lead-in", () => player.FirstFrameClockTime == 0);
        }

        [Test]
        public void TestLongLeadIn()
        {
            AddStep("create player", () => loadPlayerWithBeatmap(new TestBeatmap(ruleset.RulesetInfo) { BeatmapInfo = { AudioLeadIn = 10000 } }));
            AddUntilStep("correct lead-in", () => player.FirstFrameClockTime == player.GameplayStartTime - 10000);
        }

        private void loadPlayerWithBeatmap(IBeatmap beatmap)
        {
            Beatmap.Value = new TestWorkingBeatmap(beatmap, Clock);

            LoadScreen(player = new LeadInPlayer
            {
                AllowPause = false,
                AllowResults = false,
            });
        }

        private class LeadInPlayer : Player
        {
            public double? FirstFrameClockTime;

            public new GameplayClockContainer GameplayClockContainer => base.GameplayClockContainer;

            public double GameplayStartTime => DrawableRuleset.GameplayStartTime;

            public double GameplayClockTime => GameplayClockContainer.GameplayClock.CurrentTime;

            protected override void UpdateAfterChildren()
            {
                base.UpdateAfterChildren();
                if (!FirstFrameClockTime.HasValue)
                {
                    FirstFrameClockTime = GameplayClockContainer.GameplayClock.CurrentTime;
                    AddInternal(new OsuSpriteText
                    {
                        Text = $"GameplayStartTime: {DrawableRuleset.GameplayStartTime} "
                               + $"LeadInTime: {Beatmap.Value.BeatmapInfo.AudioLeadIn} "
                               + $"FirstFrameClockTime: {FirstFrameClockTime}"
                    });
                }
            }
        }
    }
}
