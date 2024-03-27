// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Timing;
using osu.Game.Beatmaps;
using osu.Game.Graphics.Sprites;
using osu.Game.Rulesets.Osu;
using osu.Game.Screens.Play;
using osu.Game.Storyboards;
using osu.Game.Tests.Beatmaps;
using osuTK;

namespace osu.Game.Tests.Visual.Gameplay
{
    public partial class TestSceneLeadIn : RateAdjustedBeatmapTestScene
    {
        private LeadInPlayer player = null!;

        private const double lenience_ms = 100;

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

            checkFirstFrameTime(expectedStartTime);
        }

        [TestCase(1000, 0)]
        [TestCase(0, 0)]
        [TestCase(-1000, -1000)]
        [TestCase(-10000, -10000)]
        public void TestStoryboardProducesCorrectStartTimeSimpleAlpha(double firstStoryboardEvent, double expectedStartTime)
        {
            var storyboard = new Storyboard();

            var sprite = new StoryboardSprite("unknown", Anchor.TopLeft, Vector2.Zero);

            sprite.TimelineGroup.Alpha.Add(Easing.None, firstStoryboardEvent, firstStoryboardEvent + 500, 0, 1);

            storyboard.GetLayer("Background").Add(sprite);

            loadPlayerWithBeatmap(new TestBeatmap(new OsuRuleset().RulesetInfo), storyboard);

            checkFirstFrameTime(expectedStartTime);
        }

        [TestCase(1000, 0, false)]
        [TestCase(0, 0, false)]
        [TestCase(-1000, -1000, false)]
        [TestCase(-10000, -10000, false)]
        [TestCase(1000, 0, true)]
        [TestCase(0, 0, true)]
        [TestCase(-1000, -1000, true)]
        [TestCase(-10000, -10000, true)]
        public void TestStoryboardProducesCorrectStartTimeFadeInAfterOtherEvents(double firstStoryboardEvent, double expectedStartTime, bool addEventToLoop)
        {
            const double loop_start_time = -20000;

            var storyboard = new Storyboard();

            var sprite = new StoryboardSprite("unknown", Anchor.TopLeft, Vector2.Zero);

            // these should be ignored as we have an alpha visibility blocker proceeding this command.
            sprite.TimelineGroup.Scale.Add(Easing.None, loop_start_time, -18000, Vector2.Zero, Vector2.One);
            var loopGroup = sprite.AddLoop(loop_start_time, 50);
            loopGroup.Scale.Add(Easing.None, loop_start_time, -18000, Vector2.Zero, Vector2.One);

            var target = addEventToLoop ? loopGroup : sprite.TimelineGroup;
            double loopRelativeOffset = addEventToLoop ? -loop_start_time : 0;
            target.Alpha.Add(Easing.None, loopRelativeOffset + firstStoryboardEvent, loopRelativeOffset + firstStoryboardEvent + 500, 0, 1);

            // these should be ignored due to being in the future.
            sprite.TimelineGroup.Alpha.Add(Easing.None, 18000, 20000, 0, 1);
            loopGroup.Alpha.Add(Easing.None, 38000, 40000, 0, 1);

            storyboard.GetLayer("Background").Add(sprite);

            loadPlayerWithBeatmap(new TestBeatmap(new OsuRuleset().RulesetInfo), storyboard);

            checkFirstFrameTime(expectedStartTime);
        }

        private void checkFirstFrameTime(double expectedStartTime) =>
            AddAssert("check first frame time", () => player.FirstFrameClockTime, () => Is.EqualTo(expectedStartTime).Within(lenience_ms));

        private void loadPlayerWithBeatmap(IBeatmap beatmap, Storyboard? storyboard = null)
        {
            AddStep("create player", () =>
            {
                Beatmap.Value = new ClockBackedTestWorkingBeatmap(beatmap, storyboard, new FramedClock(new ManualClock { Rate = 1 }), Audio);
                LoadScreen(player = new LeadInPlayer());
            });

            AddUntilStep("player loaded", () => player.IsLoaded && player.Alpha == 1);
        }

        private partial class LeadInPlayer : TestPlayer
        {
            public LeadInPlayer()
                : base(false, false)
            {
            }

            public double? FirstFrameClockTime;

            public new GameplayClockContainer GameplayClockContainer => base.GameplayClockContainer;

            public double FirstHitObjectTime => DrawableRuleset.Objects.First().StartTime;

            protected override void UpdateAfterChildren()
            {
                base.UpdateAfterChildren();

                if (!FirstFrameClockTime.HasValue)
                {
                    FirstFrameClockTime = GameplayClockContainer.CurrentTime;
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
