// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Beatmaps;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;

namespace osu.Game.Tests.Visual
{
    public abstract partial class ModFailConditionTestScene : ModTestScene
    {
        private readonly ModFailCondition mod;

        protected ModFailConditionTestScene(ModFailCondition mod)
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
            PassCondition = () => ((ModFailConditionTestPlayer)Player).CheckFailed(shouldMiss && testData.FailOnMiss)
        });

        protected override TestPlayer CreateModPlayer(Ruleset ruleset) => new ModFailConditionTestPlayer(CurrentTestData, AllowFail);

        protected partial class ModFailConditionTestPlayer : ModTestPlayer
        {
            public ModFailConditionTestPlayer(ModTestData data, bool allowFail)
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
