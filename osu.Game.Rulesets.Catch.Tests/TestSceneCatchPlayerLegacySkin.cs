// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Screens;
using osu.Framework.Testing;
using osu.Game.Skinning;
using osu.Game.Tests.Visual;
using osuTK;

namespace osu.Game.Rulesets.Catch.Tests
{
    [TestFixture]
    public partial class TestSceneCatchPlayerLegacySkin : LegacySkinPlayerTestScene
    {
        protected override Ruleset CreatePlayerRuleset() => new CatchRuleset();

        [Test]
        public void TestLegacyHUDComboCounterNotExistent([Values] bool withModifiedSkin)
        {
            if (withModifiedSkin)
            {
                AddStep("change component scale", () => Player.ChildrenOfType<LegacyScoreCounter>().First().Scale = new Vector2(2f));
                AddStep("update target", () => Player.ChildrenOfType<SkinComponentsContainer>().ForEach(LegacySkin.UpdateDrawableTarget));
                AddStep("exit player", () => Player.Exit());
                CreateTest();
            }

            AddAssert("legacy HUD combo counter not added", () => !Player.ChildrenOfType<LegacyDefaultComboCounter>().Any());
        }
    }
}
