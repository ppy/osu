// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

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

        private List<JudgementResult> judgementResults;

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

            AddAssert("spinner is missed", () => !judgementResults.Single(r => r.HitObject is Spinner).IsHit);
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

            AddAssert("spinner is missed", () => !judgementResults.Single(r => r.HitObject is Spinner).IsHit);
        }

        /// <summary>
        /// Performs one half-spin, then changes direction and performs a full spin. +0.5 rotation -> -1 rotation -> 1 judgement.
        /// </summary>
        [Test]
        public void TestSpinHalfThenChangeDirection()
        {
            List<ReplayFrame> frames =
                // HALF clockwise rotation.
                generateSpinnerFrames(time_spinner_start, time_spinner_start + 500, 0, MathF.PI)
                    // Just a bit more than one FULL counter-clockwise rotation.
                    .Concat(generateSpinnerFrames(time_spinner_start + 500, time_spinner_start + 1000, MathF.PI, -MathF.PI * 1.1f))
                    // Just a bit more than a HALF clockwise rotation.
                    .Concat(generateSpinnerFrames(time_spinner_start + 1000, time_spinner_start + 1500, -MathF.PI * 1.1f, 0))
                    .Select(t => new OsuReplayFrame(t.time, t.pos, OsuAction.LeftButton))
                    .Cast<ReplayFrame>()
                    .ToList();

            performTest(frames);

            AddAssert("one tick hit", () => judgementResults.Where(r => r.HitObject is SpinnerTick).Count(r => r.IsHit), () => Is.EqualTo(1));
            AddAssert("spinner is missed", () => !judgementResults.Single(r => r.HitObject is Spinner).IsHit);
        }

        /// <summary>
        /// Generates rotational spinner frames.
        /// </summary>
        /// <param name="startTime">The start time.</param>
        /// <param name="endTime">The end time.</param>
        /// <param name="startAngle">The starting angle, clockwise with origin at the top of the spinner.</param>
        /// <param name="endAngle">The ending angle, clockwise with origin at the top of the spinner.</param>
        /// <returns></returns>
        private IEnumerable<(double time, Vector2 pos)> generateSpinnerFrames(double startTime, double endTime, float startAngle, float endAngle)
        {
            startAngle -= MathF.PI / 2;
            endAngle -= MathF.PI / 2;

            Vector2 offset = new Vector2(50);

            for (double t = startTime; t < endTime; t += 10)
                yield return (t, calcOffset((t - startTime) / (endTime - startTime)));

            yield return (endTime, calcOffset(1));

            Vector2 calcOffset(double p)
            {
                float angle = startAngle + (endAngle - startAngle) * (float)p;
                return new Vector2(centre_x, centre_y) + offset * new Vector2(MathF.Cos(angle), MathF.Sin(angle));
            }
        }

        private ScoreAccessibleReplayPlayer currentPlayer;

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
                judgementResults = new List<JudgementResult>();
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
