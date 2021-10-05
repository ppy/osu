// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Diagnostics;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Judgements;
using osu.Game.Rulesets.Scoring;
using osu.Game.Screens.Play;
using osu.Game.Screens.Play.HUD;
using osuTK;

namespace osu.Game.Tests.Visual.Gameplay
{
    public class TestScenePerformancePointsCounter : OsuTestScene
    {
        [Cached]
        private GameplayState gameplayState;

        [Cached]
        private ScoreProcessor scoreProcessor;

        private int iteration;

        public TestScenePerformancePointsCounter()
        {
            var ruleset = CreateRuleset();

            Debug.Assert(ruleset != null);

            var beatmap = CreateWorkingBeatmap(ruleset.RulesetInfo)
                .GetPlayableBeatmap(ruleset.RulesetInfo);

            gameplayState = new GameplayState(beatmap, ruleset);
            scoreProcessor = new ScoreProcessor();
        }

        protected override Ruleset CreateRuleset() => new OsuRuleset();

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("Create counter", () =>
            {
                Child = new PerformancePointsCounter
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Scale = new Vector2(5),
                };
            });

            AddRepeatStep("Add judgement", () =>
            {
                var scoreInfo = gameplayState.Score.ScoreInfo;

                scoreInfo.MaxCombo = iteration * 1000;
                scoreInfo.Accuracy = 1;
                scoreInfo.Statistics[HitResult.Great] = iteration * 1000;

                scoreProcessor.ApplyResult(new OsuJudgementResult(new HitObject
                {
                    StartTime = iteration * 10000,
                }, new OsuJudgement())
                {
                    Type = HitResult.Perfect,
                });

                iteration++;
            }, 10);

            AddStep("Revert judgement", () =>
            {
                scoreProcessor.RevertResult(new JudgementResult(new HitObject(), new OsuJudgement()));
            });
        }
    }
}
