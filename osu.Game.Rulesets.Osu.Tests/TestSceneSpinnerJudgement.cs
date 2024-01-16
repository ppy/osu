// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Extensions.TypeExtensions;
using osu.Framework.Screens;
using osu.Game.Beatmaps;
using osu.Game.Replays;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.UI;
using osu.Game.Rulesets.Replays;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Screens.Play;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Osu.Tests
{
    public partial class TestSceneSpinnerJudgement : RateAdjustedBeatmapTestScene
    {
        private const double time_spinner_start = 2000;
        private const double time_spinner_end = 4000;

        private List<Judgement> judgementResults = new List<Judgement>();
        private ScoreAccessibleReplayPlayer currentPlayer = null!;

        [Test]
        public void TestHitNothing()
        {
            performTest(new List<ReplayFrame>());

            AddAssert("all min judgements", () => judgementResults.All(result => result.Type == result.JudgementCriteria.MinResult));
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(5)]
        public void TestNumberOfSpins(int spins)
        {
            performTest(generateReplay(spins));

            for (int i = 0; i < spins; ++i)
                assertResult<SpinnerTick>(i, HitResult.SmallBonus);

            assertResult<SpinnerTick>(spins, HitResult.IgnoreMiss);
        }

        [Test]
        public void TestHitEverything()
        {
            performTest(generateReplay(20));

            AddAssert("all max judgements", () => judgementResults.All(result => result.Type == result.JudgementCriteria.MaxResult));
        }

        private static List<ReplayFrame> generateReplay(int spins) => new SpinFramesGenerator(time_spinner_start)
                                                                      .Spin(spins * 360, time_spinner_end - time_spinner_start)
                                                                      .Build();

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
                            Position = OsuPlayfield.BASE_SIZE / 2
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
                judgementResults = new List<Judgement>();
            });

            AddUntilStep("Beatmap at 0", () => Beatmap.Value.Track.CurrentTime == 0);
            AddUntilStep("Wait until player is loaded", () => currentPlayer.IsCurrentScreen());
            AddUntilStep("Wait for completion", () => currentPlayer.ScoreProcessor.HasCompleted.Value);
        }

        private void assertResult<T>(int index, HitResult expectedResult)
        {
            AddAssert($"{typeof(T).ReadableName()} ({index}) judged as {expectedResult}",
                () => judgementResults.Where(j => j.HitObject is T).OrderBy(j => j.HitObject.StartTime).ElementAt(index).Type,
                () => Is.EqualTo(expectedResult));
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
