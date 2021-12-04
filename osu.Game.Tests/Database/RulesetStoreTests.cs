// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Game.Models;
using osu.Game.Stores;

namespace osu.Game.Tests.Database
{
    public class RulesetStoreTests : RealmTest
    {
        [Test]
        public void TestCreateStore()
        {
            RunTestWithRealm((realmFactory, storage) =>
            {
                var rulesets = new RealmRulesetStore(realmFactory, storage);

                Assert.AreEqual(4, rulesets.AvailableRulesets.Count());
                Assert.AreEqual(4, realmFactory.Context.All<RealmRuleset>().Count());
            });
        }

        [Test]
        public void TestCreateStoreTwiceDoesntAddRulesetsAgain()
        {
            RunTestWithRealm((realmFactory, storage) =>
            {
                var rulesets = new RealmRulesetStore(realmFactory, storage);
                var rulesets2 = new RealmRulesetStore(realmFactory, storage);

                Assert.AreEqual(4, rulesets.AvailableRulesets.Count());
                Assert.AreEqual(4, rulesets2.AvailableRulesets.Count());

                Assert.AreEqual(rulesets.AvailableRulesets.First(), rulesets2.AvailableRulesets.First());
                Assert.AreEqual(4, realmFactory.Context.All<RealmRuleset>().Count());
            });
        }

        [Test]
        public void TestRetrievedRulesetsAreDetached()
        {
            RunTestWithRealm((realmFactory, storage) =>
            {
                var rulesets = new RealmRulesetStore(realmFactory, storage);

                Assert.IsFalse(rulesets.AvailableRulesets.First().IsManaged);
                Assert.IsFalse(rulesets.GetRuleset(0)?.IsManaged);
                Assert.IsFalse(rulesets.GetRuleset("mania")?.IsManaged);
            });
        }
    }
}
