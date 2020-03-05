// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Beatmaps;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;

namespace osu.Game.Tests.Visual
{
    public abstract class ModPerfectTestScene : ModTestScene
    {
        private readonly Ruleset ruleset;
        private readonly ModPerfect mod;

        protected ModPerfectTestScene(Ruleset ruleset, ModPerfect mod)
            : base(ruleset)
        {
            this.ruleset = ruleset;
            this.mod = mod;
        }

        protected void CreateHitObjectTest(HitObjectTestCase testCaseData, bool shouldMiss) => CreateModTest(new ModTestData
        {
            Mod = mod,
            Beatmap = new Beatmap
            {
                BeatmapInfo = { Ruleset = ruleset.RulesetInfo },
                HitObjects = { testCaseData.HitObject }
            },
            Autoplay = !shouldMiss,
            PassCondition = () => ((PerfectModTestPlayer)Player).CheckFailed(shouldMiss && testCaseData.FailOnMiss)
        });

        protected override TestPlayer CreatePlayer(Ruleset ruleset) => new PerfectModTestPlayer();

        private class PerfectModTestPlayer : TestPlayer
        {
            protected override bool AllowFail => true;

            public bool CheckFailed(bool failed)
            {
                if (!failed)
                    return ScoreProcessor.HasCompleted && !HealthProcessor.HasFailed;

                return HealthProcessor.HasFailed;
            }
        }

        protected class HitObjectTestCase
        {
            public readonly HitObject HitObject;
            public readonly bool FailOnMiss;

            public HitObjectTestCase(HitObject hitObject, bool failOnMiss = true)
            {
                HitObject = hitObject;
                FailOnMiss = failOnMiss;
            }
        }
    }
}
