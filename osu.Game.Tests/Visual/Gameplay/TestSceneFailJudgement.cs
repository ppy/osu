// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Screens.Play;

namespace osu.Game.Tests.Visual.Gameplay
{
    public class TestSceneFailJudgement : TestSceneAllRulesetPlayers
    {
        protected override Player CreatePlayer(Ruleset ruleset)
        {
            SelectedMods.Value = Array.Empty<Mod>();
            return new FailPlayer();
        }

        protected override void AddCheckSteps()
        {
            AddUntilStep("wait for fail", () => Player.HasFailed);
            AddUntilStep("wait for multiple judgements", () => ((FailPlayer)Player).ScoreProcessor.JudgedHits > 1);
            AddAssert("total number of results == 1", () =>
            {
                var score = new ScoreInfo();
                ((FailPlayer)Player).ScoreProcessor.PopulateScore(score);

                return score.Statistics.Values.Sum() == 1;
            });
        }

        private class FailPlayer : TestPlayer
        {
            public new HealthProcessor HealthProcessor => base.HealthProcessor;

            public FailPlayer()
                : base(false, false)
            {
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();
                HealthProcessor.FailConditions += (_, __) => true;
            }
        }
    }
}
