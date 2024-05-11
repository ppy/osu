// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Testing;
using osu.Game.Rulesets.Mania.Mods;
using osu.Game.Rulesets.Mania.Objects.Drawables;
using osu.Game.Rulesets.UI.Scrolling;
using osu.Game.Rulesets.UI.Scrolling.Algorithms;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Mania.Tests.Mods
{
    public partial class TestSceneManiaModConstantSpeed : ModTestScene
    {
        protected override Ruleset CreatePlayerRuleset() => new ManiaRuleset();

        [Test]
        public void TestConstantScroll() => CreateModTest(new ModTestData
        {
            Mod = new ManiaModConstantSpeed(),
            PassCondition = () =>
            {
                var hitObject = Player.ChildrenOfType<DrawableManiaHitObject>().FirstOrDefault();
                return hitObject?.Dependencies.Get<IScrollingInfo>().Algorithm.Value is ConstantScrollAlgorithm;
            }
        });
    }
}
