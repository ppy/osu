// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.MathUtils;
using osu.Game.Beatmaps;
using osu.Game.Graphics.Sprites;
using osu.Game.Rulesets.Osu;
using osu.Game.Screens.Play;
using osu.Game.Tests.Beatmaps;

namespace osu.Game.Tests.Visual.Gameplay
{
    public class TestSceneLeadIn : RateAdjustedBeatmapTestScene
    {
        private LeadInPlayer player;

        [Test]
        public void TestShortLeadIn()
        {
            loadPlayerWithBeatmap(new TestBeatmap(new OsuRuleset().RulesetInfo)
            {
                BeatmapInfo = { AudioLeadIn = 1000 }
            });

            AddAssert("correct lead-in", () => Precision.AlmostEquals(player.FirstFrameClockTime.Value, 0, 100));
        }

        [Test]
        public void TestLongLeadIn()
        {
            loadPlayerWithBeatmap(new TestBeatmap(new OsuRuleset().RulesetInfo)
            {
                BeatmapInfo = { AudioLeadIn = 10000 }
            });

            AddAssert("correct lead-in", () => Precision.AlmostEquals(player.FirstFrameClockTime.Value, player.GameplayStartTime - 10000, 100));
        }

        private void loadPlayerWithBeatmap(IBeatmap beatmap)
        {
            AddStep("create player", () =>
            {
                Beatmap.Value = CreateWorkingBeatmap(beatmap);
                LoadScreen(player = new LeadInPlayer());
            });

            AddUntilStep("player loaded", () => player.IsLoaded && player.Alpha == 1);
        }

        private class LeadInPlayer : TestPlayer
        {
            public LeadInPlayer()
                : base(false, false)
            {
            }

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
