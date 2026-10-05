// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Tests.Beatmaps;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Osu.Tests.Mods
{
    public partial class TestSceneOsuModEasy : OsuModTestScene
    {
        protected override bool AllowFail => true;

        [Test]
        public void TestMultipleApplication()
        {
            bool reapplied = false;
            CreateModTest(new ModTestData
            {
                Mods = [new OsuModEasy { Retries = { Value = 1 } }],
                Autoplay = false,
                CreateBeatmap = () =>
                {
                    // do stuff to speed up fails
                    var b = new TestBeatmap(new OsuRuleset().RulesetInfo)
                    {
                        Difficulty = { DrainRate = 10 }
                    };

                    foreach (var ho in b.HitObjects)
                        ho.StartTime /= 4;

                    return b;
                },
                PassCondition = () =>
                {
                    if (((ModEasyTestPlayer)Player).FailuresSuppressed > 0 && !reapplied)
                    {
                        try
                        {
                            foreach (var mod in Player.GameplayState.Mods.OfType<IApplicableToDifficulty>())
                                mod.ApplyToDifficulty(new BeatmapDifficulty());

                            foreach (var mod in Player.GameplayState.Mods.OfType<IApplicableToPlayer>())
                                mod.ApplyToPlayer(Player);
                        }
                        catch
                        {
                            // don't care if this fails. in fact a failure here is probably better than the alternative.
                        }
                        finally
                        {
                            reapplied = true;
                        }
                    }

                    return Player.GameplayState.HasFailed && ((ModEasyTestPlayer)Player).FailuresSuppressed <= 1;
                }
            });
        }

        protected override TestPlayer CreateModPlayer(Ruleset ruleset) => new ModEasyTestPlayer(CurrentTestData, AllowFail);

        private partial class ModEasyTestPlayer : ModTestPlayer
        {
            public int FailuresSuppressed { get; private set; }

            public ModEasyTestPlayer(ModTestData data, bool allowFail)
                : base(data, allowFail)
            {
            }

            protected override bool CheckModsAllowFailure()
            {
                bool failureAllowed = GameplayState.Mods.OfType<IApplicableFailOverride>().All(m => m.PerformFail());

                if (!failureAllowed)
                    FailuresSuppressed++;

                return failureAllowed;
            }
        }
    }
}
