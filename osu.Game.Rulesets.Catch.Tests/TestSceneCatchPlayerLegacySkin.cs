// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Screens;
using osu.Framework.Testing;
using osu.Game.Screens.Play.HUD;
using osu.Game.Skinning;
using osu.Game.Tests.Visual;
using osuTK;

namespace osu.Game.Rulesets.Catch.Tests
{
    [TestFixture]
    public class TestSceneCatchPlayerLegacySkin : LegacySkinPlayerTestScene
    {
        protected override Ruleset CreatePlayerRuleset() => new CatchRuleset();

        [Test]
        [Ignore("HUD components broken, remove when fixed.")]
        public void TestLegacyHUDComboCounterHidden([Values] bool withModifiedSkin)
        {
            if (withModifiedSkin)
            {
                AddStep("change component scale", () => Player.ChildrenOfType<LegacyScoreCounter>().First().Scale = new Vector2(2f));
                AddStep("update target", () => Player.ChildrenOfType<SkinnableTargetContainer>().ForEach(LegacySkin.UpdateDrawableTarget));
                AddStep("exit player", () => Player.Exit());
                CreateTest(null);
            }

            AddAssert("legacy HUD combo counter hidden", () =>
            {
                return Player.ChildrenOfType<LegacyComboCounter>().All(counter => !counter.IsPresent || !counter.IsAlive);
            });
        }
    }
}
