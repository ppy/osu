// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Beatmaps;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;

namespace osu.Game.Tests.Visual
{
    public abstract partial class ModPerfectTestScene : ModTestScene
    {
        private readonly ModPerfect mod;

        protected ModPerfectTestScene(ModPerfect mod)
        {
            this.mod = mod;
        }

        protected void CreateHitObjectTest(HitObjectTestData testData, bool shouldMiss) => CreateModTest(new ModTestData
        {
            Mod = mod,
            Beatmap = new Beatmap
            {
                BeatmapInfo = { Ruleset = CreatePlayerRuleset().RulesetInfo },
                HitObjects = { testData.HitObject }
            },
            Autoplay = !shouldMiss,
            PassCondition = () => ((PerfectModTestPlayer)Player).CheckFailed(shouldMiss && testData.FailOnMiss)
        });

        protected override TestPlayer CreateModPlayer(Ruleset ruleset) => new PerfectModTestPlayer(CurrentTestData, AllowFail);

        protected partial class PerfectModTestPlayer : ModTestPlayer
        {
            public PerfectModTestPlayer(ModTestData data, bool allowFail)
                : base(data, allowFail)
            {
            }

            protected override bool CheckModsAllowFailure() => true;

            public bool CheckFailed(bool failed)
            {
                if (!failed)
                    return ScoreProcessor.HasCompleted.Value && !HealthProcessor.HasFailed;

                return HealthProcessor.HasFailed;
            }
        }

        protected class HitObjectTestData
        {
            public readonly HitObject HitObject;
            public readonly bool FailOnMiss;

            public HitObjectTestData(HitObject hitObject, bool failOnMiss = true)
            {
                HitObject = hitObject;
                FailOnMiss = failOnMiss;
            }
        }
    }
}
