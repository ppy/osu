// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Beatmaps;
using osu.Game.Rulesets.Osu.Difficulty;
using osu.Game.Rulesets.Osu.UI;
using osu.Game.Rulesets.UI;

namespace osu.Game.Tests.Database
{
    public class RulesetStoreTests : RealmTest
    {
        [Test]
        public void TestCreateStore()
        {
            RunTestWithRealm((realm, storage) =>
            {
                var rulesets = new RealmRulesetStore(realm, storage);

                Assert.AreEqual(4, rulesets.AvailableRulesets.Count());
                Assert.AreEqual(4, realm.Realm.All<RulesetInfo>().Count());
            });
        }

        [Test]
        public void TestCreateStoreTwiceDoesntAddRulesetsAgain()
        {
            RunTestWithRealm((realm, storage) =>
            {
                var rulesets = new RealmRulesetStore(realm, storage);
                var rulesets2 = new RealmRulesetStore(realm, storage);

                Assert.AreEqual(4, rulesets.AvailableRulesets.Count());
                Assert.AreEqual(4, rulesets2.AvailableRulesets.Count());

                Assert.AreEqual(rulesets.AvailableRulesets.First(), rulesets2.AvailableRulesets.First());
                Assert.AreEqual(4, realm.Realm.All<RulesetInfo>().Count());
            });
        }

        [Test]
        public void TestRetrievedRulesetsAreDetached()
        {
            RunTestWithRealm((realm, storage) =>
            {
                var rulesets = new RealmRulesetStore(realm, storage);

                Assert.IsFalse(rulesets.AvailableRulesets.First().IsManaged);
                Assert.IsFalse(rulesets.GetRuleset(0)?.IsManaged);
                Assert.IsFalse(rulesets.GetRuleset("mania")?.IsManaged);
            });
        }

        [Test]
        public void TestRulesetThrowingOnMethods()
        {
            RunTestWithRealm((realm, storage) =>
            {
                LoadTestRuleset.Version = Ruleset.CURRENT_RULESET_API_VERSION;
                LoadTestRuleset.HasImplementations = false;

                var ruleset = new LoadTestRuleset();
                string rulesetShortName = ruleset.RulesetInfo.ShortName;

                realm.Write(r => r.Add(new RulesetInfo(rulesetShortName, ruleset.RulesetInfo.Name, ruleset.RulesetInfo.InstantiationInfo, ruleset.RulesetInfo.OnlineID)
                {
                    Available = true,
                }));

                Assert.That(realm.Run(r => r.Find<RulesetInfo>(rulesetShortName)!.Available), Is.True);

                // Availability is updated on construction of a RealmRulesetStore
                var _ = new RealmRulesetStore(realm, storage);

                Assert.That(realm.Run(r => r.Find<RulesetInfo>(rulesetShortName)!.Available), Is.False);
            });
        }

        [Test]
        public void TestOutdatedRulesetNotAvailable()
        {
            RunTestWithRealm((realm, storage) =>
            {
                LoadTestRuleset.Version = "2021.101.0";
                LoadTestRuleset.HasImplementations = true;

                var ruleset = new LoadTestRuleset();
                string rulesetShortName = ruleset.RulesetInfo.ShortName;

                realm.Write(r => r.Add(new RulesetInfo(rulesetShortName, ruleset.RulesetInfo.Name, ruleset.RulesetInfo.InstantiationInfo, ruleset.RulesetInfo.OnlineID)
                {
                    Available = true,
                }));

                Assert.That(realm.Run(r => r.Find<RulesetInfo>(rulesetShortName)!.Available), Is.True);

                // Availability is updated on construction of a RealmRulesetStore
                var _ = new RealmRulesetStore(realm, storage);

                Assert.That(realm.Run(r => r.Find<RulesetInfo>(rulesetShortName)!.Available), Is.False);

                // Simulate the ruleset getting updated
                LoadTestRuleset.Version = Ruleset.CURRENT_RULESET_API_VERSION;
                var __ = new RealmRulesetStore(realm, storage);

                Assert.That(realm.Run(r => r.Find<RulesetInfo>(rulesetShortName)!.Available), Is.True);
            });
        }

        private class LoadTestRuleset : Ruleset
        {
            public override string RulesetAPIVersionSupported => Version;

            public static bool HasImplementations = true;

            public static string Version { get; set; } = CURRENT_RULESET_API_VERSION;

            public override IEnumerable<Mod> GetModsFor(ModType type)
            {
                if (!HasImplementations)
                    throw new NotImplementedException();

                return Array.Empty<Mod>();
            }

            public override DrawableRuleset CreateDrawableRulesetWith(IBeatmap beatmap, IReadOnlyList<Mod>? mods = null)
            {
                if (!HasImplementations)
                    throw new NotImplementedException();

                return new DrawableOsuRuleset(new OsuRuleset(), beatmap, mods);
            }

            public override IBeatmapConverter CreateBeatmapConverter(IBeatmap beatmap)
            {
                if (!HasImplementations)
                    throw new NotImplementedException();

                return new OsuBeatmapConverter(beatmap, new OsuRuleset());
            }

            public override DifficultyCalculator CreateDifficultyCalculator(IWorkingBeatmap beatmap)
            {
                if (!HasImplementations)
                    throw new NotImplementedException();

                return new OsuDifficultyCalculator(new OsuRuleset().RulesetInfo, beatmap);
            }

            public override string Description => "outdated ruleset";
            public override string ShortName => "ruleset-outdated";
        }
    }
}
