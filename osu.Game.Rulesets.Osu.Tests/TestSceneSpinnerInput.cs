// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
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

        /// <summary>
        /// A small amount to spin beyond a given angle to avoid any issues with floating-point precision.
        /// </summary>
        private const float spin_error = 1.1f;

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
            performTest(
                SpinGenerator.From(0)
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
            performTest(
                SpinGenerator.From(0)
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
            performTest(
                SpinGenerator.From(0)
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

        private class SpinGenerator
        {
            private readonly SpinGenerator? last;
            private readonly float startAngle;
            private readonly float endAngle;
            private readonly double duration;

            private SpinGenerator(float startAngle)
                : this(null, startAngle, 0)
            {
            }

            private SpinGenerator(SpinGenerator? last, float endAngle, double duration)
            {
                this.last = last;
                startAngle = last?.endAngle ?? endAngle;
                this.endAngle = endAngle;
                this.duration = duration;
            }

            public List<ReplayFrame> Build()
            {
                List<ReplayFrame> frames = new List<ReplayFrame>();

                List<SpinGenerator> allGenerators = new List<SpinGenerator>();

                SpinGenerator? l = this;

                while (l != null)
                {
                    allGenerators.Add(l);
                    l = l.last;
                }

                allGenerators.Reverse();

                double currentTime = time_spinner_start;

                foreach (var gen in allGenerators)
                {
                    double startTime = currentTime;
                    double endTime = currentTime + gen.duration;

                    for (; currentTime < endTime; currentTime += 10)
                        frames.Add(new OsuReplayFrame(currentTime, calcOffset(gen, (currentTime - startTime) / (endTime - startTime)), OsuAction.LeftButton));

                    frames.Add(new OsuReplayFrame(currentTime, calcOffset(gen, 1), OsuAction.LeftButton));
                }

                frames.Add(new OsuReplayFrame(currentTime, calcOffset(this, 1)));

                return frames;
            }

            private static Vector2 calcOffset(SpinGenerator generator, double p)
            {
                Vector2 offset = new Vector2(50);
                float angle = generator.startAngle + (generator.endAngle - generator.startAngle) * (float)p;
                return new Vector2(centre_x, centre_y) + offset * new Vector2(MathF.Cos(angle), MathF.Sin(angle));
            }

            public static SpinGenerator From(float startAngle) => new SpinGenerator(startAngle - MathF.PI / 2f);

            public SpinGenerator Spin(float amount, double duration) => new SpinGenerator(this, endAngle + amount * 2 * MathF.PI * spin_error, duration);
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
