// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Screens;
using osu.Game.Beatmaps;
using osu.Game.Replays;
using osu.Game.Rulesets.Judgements;
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
    public partial class TestSceneSpinnerInput : RateAdjustedBeatmapTestScene
    {
        private const int centre_x = 256;
        private const int centre_y = 192;
        private const double time_spinner_start = 1500;
        private const double time_spinner_end = 4000;

        private readonly List<JudgementResult> judgementResults = new List<JudgementResult>();

        private ScoreAccessibleReplayPlayer currentPlayer = null!;

        /// <summary>
        /// While off-centre, vibrates backwards and forwards on the x-axis, from centre-50 to centre+50, every 50ms.
        /// </summary>
        [Test]
        public void TestVibrateWithoutSpinningOffCentre()
        {
            List<ReplayFrame> frames = new List<ReplayFrame>();

            const int vibrate_time = 50;
            const float y_pos = centre_y - 50;

            int direction = -1;

            for (double i = time_spinner_start; i <= time_spinner_end; i += vibrate_time)
            {
                frames.Add(new OsuReplayFrame(i, new Vector2(centre_x + direction * 50, y_pos), OsuAction.LeftButton));
                frames.Add(new OsuReplayFrame(i + vibrate_time, new Vector2(centre_x - direction * 50, y_pos), OsuAction.LeftButton));

                direction *= -1;
            }

            performTest(frames);

            assertTicksHit(0);
            assertSpinnerHit(false);
        }

        /// <summary>
        /// While centred on the slider, vibrates backwards and forwards on the x-axis, from centre-50 to centre+50, every 50ms.
        /// </summary>
        [Test]
        public void TestVibrateWithoutSpinningOnCentre()
        {
            List<ReplayFrame> frames = new List<ReplayFrame>();

            const int vibrate_time = 50;

            int direction = -1;

            for (double i = time_spinner_start; i <= time_spinner_end; i += vibrate_time)
            {
                frames.Add(new OsuReplayFrame(i, new Vector2(centre_x + direction * 50, centre_y), OsuAction.LeftButton));
                frames.Add(new OsuReplayFrame(i + vibrate_time, new Vector2(centre_x - direction * 50, centre_y), OsuAction.LeftButton));

                direction *= -1;
            }

            performTest(frames);

            assertTicksHit(0);
            assertSpinnerHit(false);
        }

        /// <summary>
        /// Spins in a single direction.
        /// </summary>
        [TestCase(0.5f, 0)]
        [TestCase(-0.5f, 0)]
        [TestCase(1, 1)]
        [TestCase(-1, 1)]
        [TestCase(1.5f, 1)]
        [TestCase(-1.5f, 1)]
        [TestCase(2f, 2)]
        [TestCase(-2f, 2)]
        public void TestSpinSingleDirection(float amount, int expectedTicks)
        {
            performTest(new SpinFramesGenerator(time_spinner_start)
                        .Spin(amount, 500)
                        .Build());

            assertTicksHit(expectedTicks);
            assertSpinnerHit(false);
        }

        /// <summary>
        /// Spin half-way clockwise then perform one full spin counter-clockwise.
        /// No ticks should be hit since the total rotation is -0.5 (0.5 CW + 1 CCW = 0.5 CCW).
        /// </summary>
        [Test]
        public void TestSpinHalfBothDirections()
        {
            performTest(new SpinFramesGenerator(time_spinner_start)
                        .Spin(0.5f, 500) // Rotate to +0.5.
                        .Spin(-1f, 500) // Rotate to -0.5
                        .Build());

            assertTicksHit(0);
            assertSpinnerHit(false);
        }

        /// <summary>
        /// Spin in one direction then spin in the other.
        /// </summary>
        [TestCase(0.5f, -1.5f, 1)]
        [TestCase(-0.5f, 1.5f, 1)]
        [TestCase(0.5f, -2.5f, 2)]
        [TestCase(-0.5f, 2.5f, 2)]
        public void TestSpinOneDirectionThenChangeDirection(float direction1, float direction2, int expectedTicks)
        {
            performTest(new SpinFramesGenerator(time_spinner_start)
                        .Spin(direction1, 500)
                        .Spin(direction2, 500)
                        .Build());

            assertTicksHit(expectedTicks);
            assertSpinnerHit(false);
        }

        private void assertTicksHit(int count)
        {
            AddAssert($"{count} ticks hit", () => judgementResults.Where(r => r.HitObject is SpinnerTick).Count(r => r.IsHit), () => Is.EqualTo(count));
        }

        private void assertSpinnerHit(bool shouldBeHit)
        {
            AddAssert($"spinner is {(shouldBeHit ? "hit" : "missed")}", () => judgementResults.Single(r => r.HitObject is Spinner).IsHit, () => Is.EqualTo(shouldBeHit));
        }

        private void performTest(List<ReplayFrame> frames)
        {
            AddStep("load player", () =>
            {
                Beatmap.Value = CreateWorkingBeatmap(new Beatmap<OsuHitObject>
                {
                    HitObjects =
                    {
                        new Spinner
                        {
                            StartTime = time_spinner_start,
                            EndTime = time_spinner_end,
                            Position = new Vector2(centre_x, centre_y)
                        }
                    },
                    BeatmapInfo =
                    {
                        Difficulty = new BeatmapDifficulty(),
                        Ruleset = new OsuRuleset().RulesetInfo
                    },
                });

                var p = new ScoreAccessibleReplayPlayer(new Score { Replay = new Replay { Frames = frames } });

                p.OnLoadComplete += _ =>
                {
                    p.ScoreProcessor.NewJudgement += result =>
                    {
                        if (currentPlayer == p) judgementResults.Add(result);
                    };
                };

                LoadScreen(currentPlayer = p);
                judgementResults.Clear();
            });

            AddUntilStep("Beatmap at 0", () => Beatmap.Value.Track.CurrentTime == 0);
            AddUntilStep("Wait until player is loaded", () => currentPlayer.IsCurrentScreen());
            AddUntilStep("Wait for completion", () => currentPlayer.ScoreProcessor.HasCompleted.Value);
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
