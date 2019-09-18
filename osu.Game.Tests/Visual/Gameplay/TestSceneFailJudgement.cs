// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Screens.Play;

namespace osu.Game.Tests.Visual.Gameplay
{
    public class TestSceneFailJudgement : AllPlayersTestScene
    {
        protected override Player CreatePlayer(Ruleset ruleset)
        {
            Mods.Value = Array.Empty<Mod>();
            return new FailPlayer();
        }

        protected override void AddCheckSteps()
        {
            AddUntilStep("wait for fail", () => Player.HasFailed);
            AddUntilStep("wait for multiple judged objects", () => ((FailPlayer)Player).DrawableRuleset.Playfield.AllHitObjects.Count(h => h.AllJudged) > 1);
            AddAssert("total judgements == 1", () => ((FailPlayer)Player).ScoreProcessor.JudgedHits == 1);
        }

        private class FailPlayer : TestPlayer
        {
            public new ScoreProcessor ScoreProcessor => base.ScoreProcessor;

            public FailPlayer()
                : base(false, false)
            {
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();
                ScoreProcessor.FailConditions += (_, __) => true;
            }
        }
    }
}
