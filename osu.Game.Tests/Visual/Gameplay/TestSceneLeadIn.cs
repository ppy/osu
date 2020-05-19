// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Graphics.Sprites;
using osu.Game.Rulesets.Osu;
using osu.Game.Screens.Play;
using osu.Game.Storyboards;
using osu.Game.Tests.Beatmaps;
using osuTK;

namespace osu.Game.Tests.Visual.Gameplay
{
    public class TestSceneLeadIn : RateAdjustedBeatmapTestScene
    {
        private LeadInPlayer player;

        private const double lenience_ms = 10;

        private const double first_hit_object = 2170;

        [TestCase(1000, 0)]
        [TestCase(2000, 0)]
        [TestCase(3000, first_hit_object - 3000)]
        [TestCase(10000, first_hit_object - 10000)]
        public void TestLeadInProducesCorrectStartTime(double leadIn, double expectedStartTime)
        {
            loadPlayerWithBeatmap(new TestBeatmap(new OsuRuleset().RulesetInfo)
            {
                BeatmapInfo = { AudioLeadIn = leadIn }
            });

            AddAssert($"first frame is {expectedStartTime}", () =>
            {
                Debug.Assert(player.FirstFrameClockTime != null);
                return Precision.AlmostEquals(player.FirstFrameClockTime.Value, expectedStartTime, lenience_ms);
            });
        }

        [TestCase(1000, 0)]
        [TestCase(0, 0)]
        [TestCase(-1000, -1000)]
        [TestCase(-10000, -10000)]
        public void TestStoryboardProducesCorrectStartTime(double firstStoryboardEvent, double expectedStartTime)
        {
            var storyboard = new Storyboard();

            var sprite = new StoryboardSprite("unknown", Anchor.TopLeft, Vector2.Zero);
            sprite.TimelineGroup.Alpha.Add(Easing.None, firstStoryboardEvent, firstStoryboardEvent + 500, 0, 1);

            storyboard.GetLayer("Background").Add(sprite);

            loadPlayerWithBeatmap(new TestBeatmap(new OsuRuleset().RulesetInfo), storyboard);

            AddAssert($"first frame is {expectedStartTime}", () =>
            {
                Debug.Assert(player.FirstFrameClockTime != null);
                return Precision.AlmostEquals(player.FirstFrameClockTime.Value, expectedStartTime, lenience_ms);
            });
        }

        private void loadPlayerWithBeatmap(IBeatmap beatmap, Storyboard storyboard = null)
        {
            AddStep("create player", () =>
            {
                Beatmap.Value = CreateWorkingBeatmap(beatmap, storyboard);
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

            public double FirstHitObjectTime => DrawableRuleset.Objects.First().StartTime;

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
                               + $"FirstHitObjectTime: {FirstHitObjectTime} "
                               + $"LeadInTime: {Beatmap.Value.BeatmapInfo.AudioLeadIn} "
                               + $"FirstFrameClockTime: {FirstFrameClockTime}"
                    });
                }
            }
        }
    }
}
