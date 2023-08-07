// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Scoring;
using osu.Game.Screens.Play.HUD;
using osu.Game.Skinning;

namespace osu.Game.Tests.Visual.Gameplay
{
    public partial class TestSceneSkinnableRankDisplay : SkinnableHUDComponentTestScene
    {
        [Cached]
        private ScoreProcessor scoreProcessor = new ScoreProcessor(new OsuRuleset());

        protected override Drawable CreateDefaultImplementation() => new DefaultRankDisplay();

        protected override Drawable CreateLegacyImplementation() => new LegacyRankDisplay();

        [Test]
        public void TestChangingRank()
        {
            AddStep("Set rank to SS Hidden", () => scoreProcessor.Rank.Value = Scoring.ScoreRank.XH);
            AddStep("Set rank to SS", () => scoreProcessor.Rank.Value = Scoring.ScoreRank.X);
            AddStep("Set rank to S Hidden", () => scoreProcessor.Rank.Value = Scoring.ScoreRank.SH);
            AddStep("Set rank to S", () => scoreProcessor.Rank.Value = Scoring.ScoreRank.S);
            AddStep("Set rank to A", () => scoreProcessor.Rank.Value = Scoring.ScoreRank.A);
            AddStep("Set rank to B", () => scoreProcessor.Rank.Value = Scoring.ScoreRank.B);
            AddStep("Set rank to C", () => scoreProcessor.Rank.Value = Scoring.ScoreRank.C);
            AddStep("Set rank to D", () => scoreProcessor.Rank.Value = Scoring.ScoreRank.D);
            AddStep("Set rank to F", () => scoreProcessor.Rank.Value = Scoring.ScoreRank.F);
        }
    }
}