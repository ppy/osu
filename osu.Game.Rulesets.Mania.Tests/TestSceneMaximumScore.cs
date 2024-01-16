// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Screens;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Replays;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Mania.Replays;
using osu.Game.Rulesets.Replays;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Screens.Play;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Mania.Tests
{
    public partial class TestSceneMaximumScore : RateAdjustedBeatmapTestScene
    {
        private ScoreAccessibleReplayPlayer currentPlayer = null!;

        private List<Judgement> judgementResults = new List<Judgement>();

        [Test]
        public void TestSimultaneousTickAndNote()
        {
            performTest(
                new List<ManiaHitObject>
                {
                    new HoldNote
                    {
                        StartTime = 1000,
                        Duration = 2000,
                        Column = 0,
                    },
                    new Note
                    {
                        StartTime = 2000,
                        Column = 1
                    }
                },
                new List<ReplayFrame>
                {
                    new ManiaReplayFrame(1000, ManiaAction.Key1),
                    new ManiaReplayFrame(2000, ManiaAction.Key1, ManiaAction.Key2),
                    new ManiaReplayFrame(2001, ManiaAction.Key1),
                    new ManiaReplayFrame(3000)
                });

            AddAssert("all objects perfectly judged",
                () => judgementResults.Select(result => result.Type),
                () => Is.EquivalentTo(judgementResults.Select(result => result.JudgementCriteria.MaxResult)));
            AddAssert("score is correct", () => currentPlayer.ScoreProcessor.TotalScore.Value, () => Is.EqualTo(1_000_000));
        }

        [Test]
        public void TestSimultaneousLongNotes()
        {
            performTest(
                new List<ManiaHitObject>
                {
                    new HoldNote
                    {
                        StartTime = 1000,
                        Duration = 2000,
                        Column = 0,
                    },
                    new HoldNote
                    {
                        StartTime = 2000,
                        Duration = 2000,
                        Column = 1
                    }
                },
                new List<ReplayFrame>
                {
                    new ManiaReplayFrame(1000, ManiaAction.Key1),
                    new ManiaReplayFrame(2000, ManiaAction.Key1, ManiaAction.Key2),
                    new ManiaReplayFrame(3000, ManiaAction.Key2),
                    new ManiaReplayFrame(4000)
                });

            AddAssert("all objects perfectly judged",
                () => judgementResults.Select(result => result.Type),
                () => Is.EquivalentTo(judgementResults.Select(result => result.JudgementCriteria.MaxResult)));
            AddAssert("score is correct", () => currentPlayer.ScoreProcessor.TotalScore.Value, () => Is.EqualTo(1_000_000));
        }

        private void performTest(List<ManiaHitObject> hitObjects, List<ReplayFrame> frames)
        {
            var beatmap = new Beatmap<ManiaHitObject>
            {
                HitObjects = hitObjects,
                BeatmapInfo =
                {
                    Difficulty = new BeatmapDifficulty { SliderTickRate = 4 },
                    Ruleset = new ManiaRuleset().RulesetInfo
                },
            };

            beatmap.ControlPointInfo.Add(0, new EffectControlPoint { ScrollSpeed = 0.1f });

            AddStep("load player", () =>
            {
                Beatmap.Value = CreateWorkingBeatmap(beatmap);

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
