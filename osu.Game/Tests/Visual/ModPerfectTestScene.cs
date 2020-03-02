// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Extensions.TypeExtensions;
using osu.Game.Beatmaps;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Scoring;

namespace osu.Game.Tests.Visual
{
    public abstract class ModPerfectTestScene : ModSandboxTestScene
    {
        private readonly Ruleset ruleset;
        private readonly ModPerfect perfectMod;

        protected ModPerfectTestScene(Ruleset ruleset, ModPerfect perfectMod)
            : base(ruleset)
        {
            this.ruleset = ruleset;
            this.perfectMod = perfectMod;
        }

        protected void CreateHitObjectTest(HitObjectTestCase testCaseData, bool shouldMiss) => CreateModTest(new ModTestCaseData(testCaseData.HitObject.GetType().ReadableName(), perfectMod)
        {
            Beatmap = new Beatmap
            {
                BeatmapInfo = { Ruleset = ruleset.RulesetInfo },
                HitObjects = { testCaseData.HitObject }
            },
            Autoplay = !shouldMiss,
            PassCondition = () => ((PerfectModTestPlayer)Player).CheckFailed(shouldMiss && testCaseData.FailOnMiss)
        });

        protected sealed override TestPlayer CreateReplayPlayer(Score score) => new PerfectModTestPlayer(score);

        private class PerfectModTestPlayer : TestPlayer
        {
            public PerfectModTestPlayer(Score score)
                : base(score)
            {
            }

            protected override bool AllowFail => true;

            public bool CheckFailed(bool failed)
            {
                if (!failed)
                    return ScoreProcessor.HasCompleted && !HealthProcessor.HasFailed;

                return ScoreProcessor.JudgedHits > 0 && HealthProcessor.HasFailed;
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
