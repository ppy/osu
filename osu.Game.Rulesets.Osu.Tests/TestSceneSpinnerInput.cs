// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Screens;
using osu.Framework.Testing;
using osu.Framework.Timing;
using osu.Game.Beatmaps;
using osu.Game.Replays;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Rulesets.Osu.Replays;
using osu.Game.Rulesets.Replays;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;
using osu.Game.Scoring;
using osu.Game.Screens.Play;
using osu.Game.Storyboards;
using osu.Game.Tests.Visual;
using osuTK;

namespace osu.Game.Rulesets.Osu.Tests
{
    public partial class TestSceneSpinnerInput : RateAdjustedBeatmapTestScene
    {
        private const int centre_x = 256;
        private const int centre_y = 192;
        private const double time_spinner_start = 1500;
        private const double time_spinner_end = 8000;

        private readonly List<JudgementResult> judgementResults = new List<JudgementResult>();

        private ScoreAccessibleReplayPlayer currentPlayer = null!;
        private ManualClock? manualClock;

        protected override WorkingBeatmap CreateWorkingBeatmap(IBeatmap beatmap, Storyboard? storyboard = null)
        {
            return manualClock == null
                ? base.CreateWorkingBeatmap(beatmap, storyboard)
                : new ClockBackedTestWorkingBeatmap(beatmap, storyboard, new FramedClock(manualClock), Audio);
        }

        [SetUp]
        public void Setup() => Schedule(() =>
        {
            manualClock = null;
            SelectedMods.Value = Array.Empty<Mod>();
        });

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

        [Test]
        public void TestVibrateWithoutSpinningOnCentreWithDoubleTime()
        {
            List<ReplayFrame> frames = new List<ReplayFrame>();

            const int rate = 2;
            // the track clock is going to be playing twice as fast,
            // so the vibration time in clock time needs to be twice as long
            // to keep constant speed in real time.
            const int vibrate_time = 50 * rate;

            int direction = -1;

            for (double i = time_spinner_start; i <= time_spinner_end; i += vibrate_time)
            {
                frames.Add(new OsuReplayFrame(i, new Vector2(centre_x + direction * 50, centre_y), OsuAction.LeftButton));
                frames.Add(new OsuReplayFrame(i + vibrate_time, new Vector2(centre_x - direction * 50, centre_y), OsuAction.LeftButton));

                direction *= -1;
            }

            AddStep("set DT", () => SelectedMods.Value = new[] { new OsuModDoubleTime { SpeedChange = { Value = rate } } });
            performTest(frames);

            assertSpinnerHit(false);
        }

        /// <summary>
        /// Spins in a single direction.
        /// </summary>
        [TestCase(180, 0)]
        [TestCase(-180, 0)]
        [TestCase(360, 1)]
        [TestCase(-360, 1)]
        [TestCase(540, 1)]
        [TestCase(-540, 1)]
        [TestCase(720, 2)]
        [TestCase(-720, 2)]
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
                        .Spin(180, 500) // Rotate to +0.5.
                        .Spin(-360, 500) // Rotate to -0.5
                        .Build());

            assertTicksHit(0);
            assertSpinnerHit(false);
        }

        /// <summary>
        /// Spin in one direction then spin in the other.
        /// </summary>
        [TestCase(180, -540, 1)]
        [TestCase(-180, 540, 1)]
        [TestCase(180, -900, 2)]
        [TestCase(-180, 900, 2)]
        public void TestSpinOneDirectionThenChangeDirection(float direction1, float direction2, int expectedTicks)
        {
            performTest(new SpinFramesGenerator(time_spinner_start)
                        .Spin(direction1, 500)
                        .Spin(direction2, 500)
                        .Build());

            assertTicksHit(expectedTicks);
            assertSpinnerHit(false);
        }

        [Test]
        public void TestRewind()
        {
            AddStep("set manual clock", () => manualClock = new ManualClock
            {
                // Avoids interpolation trying to run ahead during testing.
                Rate = 0
            });

            List<ReplayFrame> frames =
                new SpinFramesGenerator(time_spinner_start)
                    // 1500ms start
                    .Spin(360, 500)
                    // 2000ms -> 1 full CW spin
                    .Spin(-180, 500)
                    // 2500ms -> 1 full CW spin + 0.5 CCW spins
                    .Spin(90, 500)
                    // 3000ms -> 1 full CW spin + 0.25 CCW spins
                    .Spin(450, 500)
                    // 3500ms -> 2 full CW spins
                    .Spin(180, 500)
                    // 4000ms -> 2 full CW spins + 0.5 CW spins
                    .Build();

            loadPlayer(frames);

            GameplayClockContainer clock = null!;
            DrawableRuleset drawableRuleset = null!;
            AddStep("get gameplay objects", () =>
            {
                clock = currentPlayer.ChildrenOfType<GameplayClockContainer>().Single();
                drawableRuleset = currentPlayer.ChildrenOfType<DrawableRuleset>().Single();
            });

            addSeekStep(frames.Last().Time);

            DrawableSpinner drawableSpinner = null!;
            AddUntilStep("get spinner", () => (drawableSpinner = currentPlayer.ChildrenOfType<DrawableSpinner>().Single()) != null);

            assertFinalRotationCorrect();
            assertTotalRotation(3750, 810);
            assertTotalRotation(3500, 720);
            assertTotalRotation(3250, 530);
            assertTotalRotation(3000, 450);
            assertTotalRotation(2750, 540);
            assertTotalRotation(2500, 540);
            assertTotalRotation(2250, 450);
            assertTotalRotation(2000, 360);
            assertTotalRotation(1500, 0);

            // same thing but always returning to final time to check.
            assertFinalRotationCorrect();
            assertTotalRotation(3750, 810);
            assertFinalRotationCorrect();
            assertTotalRotation(3500, 720);
            assertFinalRotationCorrect();
            assertTotalRotation(3250, 530);
            assertFinalRotationCorrect();
            assertTotalRotation(3000, 450);
            assertFinalRotationCorrect();
            assertTotalRotation(2750, 540);
            assertFinalRotationCorrect();
            assertTotalRotation(2500, 540);
            assertFinalRotationCorrect();
            assertTotalRotation(2250, 450);
            assertFinalRotationCorrect();
            assertTotalRotation(2000, 360);
            assertFinalRotationCorrect();
            assertTotalRotation(1500, 0);

            void assertTotalRotation(double time, float expected)
            {
                addSeekStep(time);
                AddAssert($"total rotation @ {time} is {expected}", () => drawableSpinner.Result.TotalRotation,
                    () => Is.EqualTo(expected).Within(MathHelper.RadiansToDegrees(SpinFramesGenerator.SPIN_ERROR * 2)));
            }

            void addSeekStep(double time)
            {
                AddStep($"seek to {time}", () => clock.Seek(time));
                // Lenience is required due to interpolation running slightly ahead on a stalled clock.
                AddUntilStep("wait for seek to finish", () => drawableRuleset.FrameStableClock.CurrentTime, () => Is.EqualTo(time));
            }

            void assertFinalRotationCorrect() => assertTotalRotation(4000, 900);
        }

        private void assertTicksHit(int count)
        {
            AddAssert($"{count} ticks hit", () => judgementResults.Where(r => r.HitObject is SpinnerTick && !(r.HitObject is SpinnerHealthTick)).Count(r => r.IsHit), () => Is.EqualTo(count));
        }

        private void assertSpinnerHit(bool shouldBeHit)
        {
            AddAssert($"spinner is {(shouldBeHit ? "hit" : "missed")}", () => judgementResults.Single(r => r.HitObject is Spinner).IsHit, () => Is.EqualTo(shouldBeHit));
        }

        private void loadPlayer(List<ReplayFrame> frames)
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
        }

        private void performTest(List<ReplayFrame> frames)
        {
            loadPlayer(frames);
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
