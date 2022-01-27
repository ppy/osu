// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Catch;
using osu.Game.Rulesets.Osu;

namespace osu.Game.Tests.NonVisual
{
    [TestFixture]
    public class RulesetInfoOrderingTest
    {
        [Test]
        public void TestOrdering()
        {
            var rulesets = new[]
            {
                new RulesetInfo("custom2", "Custom Ruleset 2", string.Empty, -1),
                new OsuRuleset().RulesetInfo,
                new RulesetInfo("custom3", "Custom Ruleset 3", string.Empty, -1),
                new RulesetInfo("custom2", "Custom Ruleset 2", string.Empty, -1),
                new CatchRuleset().RulesetInfo,
                new RulesetInfo("custom3", "Custom Ruleset 3", string.Empty, -1),
            };

            var orderedRulesets = rulesets.OrderBy(r => r);

            // Ensure all customs are after official.
            Assert.That(orderedRulesets.Select(r => r.OnlineID), Is.EqualTo(new[] { 0, 2, -1, -1, -1, -1 }));

            // Ensure customs are grouped next to each other (ie. stably sorted).
            Assert.That(orderedRulesets.SkipWhile(r => r.ShortName != "custom2").Skip(1).First().ShortName, Is.EqualTo("custom2"));
            Assert.That(orderedRulesets.SkipWhile(r => r.ShortName != "custom3").Skip(1).First().ShortName, Is.EqualTo("custom3"));
        }
    }
}
