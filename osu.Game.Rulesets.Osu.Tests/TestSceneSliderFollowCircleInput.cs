// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Screens;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Replays;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Replays;
using osu.Game.Rulesets.Replays;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Screens.Play;
using osu.Game.Tests.Visual;
using osuTK;

namespace osu.Game.Rulesets.Osu.Tests
{
    [HeadlessTest]
    public partial class TestSceneSliderFollowCircleInput : RateAdjustedBeatmapTestScene
    {
        private List<JudgementResult>? judgementResults;
        private ScoreAccessibleReplayPlayer? currentPlayer;

        [Test]
        public void TestMaximumDistanceTrackingWithoutMovement(
            [Values(0, 5, 10)] float circleSize,
            [Values(0, 5, 10)] double velocity)
        {
            const double time_slider_start = 1000;

            float circleRadius = OsuHitObject.OBJECT_RADIUS * (1.0f - 0.7f * (circleSize - 5) / 5) / 2;
            float followCircleRadius = circleRadius * 1.2f;

            performTest(new Beatmap<OsuHitObject>
            {
                HitObjects =
                {
                    new Slider
                    {
                        StartTime = time_slider_start,
                        Position = new Vector2(0, 0),
                        SliderVelocityMultiplier = velocity,
                        Path = new SliderPath(PathType.Linear, new[]
                        {
                            Vector2.Zero,
                            new Vector2(followCircleRadius, 0),
                        }, followCircleRadius),
                    },
                },
                BeatmapInfo =
                {
                    Difficulty = new BeatmapDifficulty
                    {
                        CircleSize = circleSize,
                        SliderTickRate = 1
                    },
                    Ruleset = new OsuRuleset().RulesetInfo
                },
            }, new List<ReplayFrame>
            {
                new OsuReplayFrame { Position = new Vector2(-circleRadius + 1, 0), Actions = { OsuAction.LeftButton }, Time = time_slider_start },
            });

            AddAssert("Tracking kept", assertMaxJudge);
        }

        private bool assertMaxJudge() => judgementResults?.Any() == true && judgementResults.All(t => t.Type == t.Judgement.MaxResult);

        private void performTest(Beatmap<OsuHitObject> beatmap, List<ReplayFrame> frames)
        {
            AddStep("load player", () =>
            {
                Beatmap.Value = CreateWorkingBeatmap(beatmap);

                var p = new ScoreAccessibleReplayPlayer(new Score { Replay = new Replay { Frames = frames } });

                p.OnLoadComplete += _ =>
                {
                    p.ScoreProcessor.NewJudgement += result =>
                    {
                        if (currentPlayer == p) judgementResults?.Add(result);
                    };
                };

                LoadScreen(currentPlayer = p);
                judgementResults = new List<JudgementResult>();
            });

            AddUntilStep("Beatmap at 0", () => Beatmap.Value.Track.CurrentTime == 0);
            AddUntilStep("Wait until player is loaded", () => currentPlayer.IsCurrentScreen());
            AddUntilStep("Wait for completion", () => currentPlayer?.ScoreProcessor.HasCompleted.Value == true);
        }

        private partial class ScoreAccessibleReplayPlayer : ReplayPlayer
        {
            public new ScoreProcessor ScoreProcessor => base.ScoreProcessor;

            protected override bool PauseOnFocusLost => false;

            public ScoreAccessibleReplayPlayer(Score score)
                : base(score, new PlayerConfiguration
                {
                    AllowPause = false,
                    ShowResults = false,
                })
            {
            }
        }
    }
}
