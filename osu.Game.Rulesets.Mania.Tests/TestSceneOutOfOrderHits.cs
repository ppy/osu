// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Extensions.TypeExtensions;
using osu.Framework.Screens;
using osu.Framework.Utils;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Replays;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Mania.Beatmaps;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Mania.Replays;
using osu.Game.Rulesets.Replays;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Screens.Play;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Mania.Tests
{
    public class TestSceneOutOfOrderHits : RateAdjustedBeatmapTestScene
    {
        [Test]
        public void TestPreviousHitWindowDoesNotExtendPastNextObject()
        {
            var objects = new List<ManiaHitObject>();
            var frames = new List<ReplayFrame>();

            for (int i = 0; i < 7; i++)
            {
                double time = 1000 + i * 100;

                objects.Add(new Note { StartTime = time });

                if (i > 0)
                {
                    frames.Add(new ManiaReplayFrame(time + 10, ManiaAction.Key1));
                    frames.Add(new ManiaReplayFrame(time + 11));
                }
            }

            performTest(objects, frames);

            addJudgementAssert(objects[0], HitResult.Miss);

            for (int i = 1; i < 7; i++)
            {
                addJudgementAssert(objects[i], HitResult.Perfect);
                addJudgementOffsetAssert(objects[i], 10);
            }
        }

        private void addJudgementAssert(ManiaHitObject hitObject, HitResult result)
        {
            AddAssert($"({hitObject.GetType().ReadableName()} @ {hitObject.StartTime}) judgement is {result}",
                () => judgementResults.Single(r => r.HitObject == hitObject).Type == result);
        }

        private void addJudgementOffsetAssert(ManiaHitObject hitObject, double offset)
        {
            AddAssert($"({hitObject.GetType().ReadableName()} @ {hitObject.StartTime}) judged at {offset}",
                () => Precision.AlmostEquals(judgementResults.Single(r => r.HitObject == hitObject).TimeOffset, offset, 100));
        }

        private ScoreAccessibleReplayPlayer currentPlayer;
        private List<JudgementResult> judgementResults;

        private void performTest(List<ManiaHitObject> hitObjects, List<ReplayFrame> frames)
        {
            AddStep("load player", () =>
            {
                Beatmap.Value = CreateWorkingBeatmap(new ManiaBeatmap(new StageDefinition { Columns = 4 })
                {
                    HitObjects = hitObjects,
                    BeatmapInfo =
                    {
                        Ruleset = new ManiaRuleset().RulesetInfo
                    },
                });

                Beatmap.Value.Beatmap.ControlPointInfo.Add(0, new DifficultyControlPoint { SpeedMultiplier = 0.1f });

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

        private class ScoreAccessibleReplayPlayer : ReplayPlayer
        {
            public new ScoreProcessor ScoreProcessor => base.ScoreProcessor;

            protected override bool PauseOnFocusLost => false;

            public ScoreAccessibleReplayPlayer(Score score)
                : base(score, false, false)
            {
            }
        }
    }
}
