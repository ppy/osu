// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Game.Configuration;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu;
using osu.Game.Scoring;

namespace osu.Game.Tests.Visual.Gameplay
{
    public partial class TestSceneContinuePlayingAfterFailed : PlayerTestScene
    {
        protected override Ruleset CreatePlayerRuleset() => new OsuRuleset();

        protected override TestPlayer CreatePlayer(Ruleset ruleset)
        {
            SelectedMods.Value = Array.Empty<Mod>();
            return new FailPlayer();
        }

        [Test]
        public void TestPlayingKeepPlayingWhenKeepPlayingEnable()
        {
            AddStep("set Continue play after failed to true", () => LocalConfig.SetValue(OsuSetting.KeepGameplayAfterFailed, true));
            AddUntilStep("player is playing", () => Player.LocalUserPlaying.Value);
            AddUntilStep("wait for multiple judgements", () => Player.ScoreProcessor.JudgedHits > 1);
            AddAssert("total number of results > 1", () =>
            {
                var score = new ScoreInfo { Ruleset = Ruleset.Value };

                Player.ScoreProcessor.PopulateScore(score);

                return score.Statistics.Values.Sum() > 1;
            });
            AddAssert("score marked as F", () =>
            {
                var score = new ScoreInfo { Ruleset = Ruleset.Value };

                Player.ScoreProcessor.PopulateScore(score);

                return score.Rank == ScoreRank.F;
            });
        }

        [Test]
        public void TestPlayingFailWhenKeepPlayingDisable()
        {
            AddStep("set Continue play after failed to false", () => LocalConfig.SetValue(OsuSetting.KeepGameplayAfterFailed, false));
            AddUntilStep("player is playing", () => Player.LocalUserPlaying.Value);
            AddUntilStep("wait for multiple judgements", () => Player.ScoreProcessor.JudgedHits > 1);
            AddAssert("total number of results = 1", () =>
            {
                var score = new ScoreInfo { Ruleset = Ruleset.Value };

                Player.ScoreProcessor.PopulateScore(score);

                return score.Statistics.Values.Sum() == 1;
            });
        }

        private partial class FailPlayer : TestPlayer
        {
            public FailPlayer()
                : base(false, false)
            {
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();
                HealthProcessor.FailConditions += (_, _) => true;
            }
        }
    }
}
