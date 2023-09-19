// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Objects;
using osuTK;

namespace osu.Game.Tests.Visual.Mods
{
    public partial class TestSceneModAccuracyChallenge : ModTestScene
    {
        protected override Ruleset CreatePlayerRuleset() => new OsuRuleset();

        protected override TestPlayer CreateModPlayer(Ruleset ruleset)
        {
            var player = base.CreateModPlayer(ruleset);
            return player;
        }

        protected override bool AllowFail => true;

        [Test]
        public void TestMaximumAchievableAccuracy() =>
            CreateModTest(new ModTestData
            {
                Mod = new ModAccuracyChallenge
                {
                    MinimumAccuracy = { Value = 0.6 }
                },
                Autoplay = false,
                Beatmap = new Beatmap
                {
                    HitObjects = Enumerable.Range(0, 5).Select(i => new HitCircle
                    {
                        StartTime = i * 250,
                        Position = new Vector2(i * 50)
                    }).Cast<HitObject>().ToList()
                },
                PassCondition = () => Player.GameplayState.HasFailed && Player.ScoreProcessor.JudgedHits >= 3
            });

        [Test]
        public void TestStandardAccuracy() =>
            CreateModTest(new ModTestData
            {
                Mod = new ModAccuracyChallenge
                {
                    MinimumAccuracy = { Value = 0.6 },
                    AccuracyJudgeMode = { Value = ModAccuracyChallenge.AccuracyMode.Standard }
                },
                Autoplay = false,
                Beatmap = new Beatmap
                {
                    HitObjects = Enumerable.Range(0, 5).Select(i => new HitCircle
                    {
                        StartTime = i * 250,
                        Position = new Vector2(i * 50)
                    }).Cast<HitObject>().ToList()
                },
                PassCondition = () => Player.GameplayState.HasFailed && Player.ScoreProcessor.JudgedHits >= 1
            });
    }
}
