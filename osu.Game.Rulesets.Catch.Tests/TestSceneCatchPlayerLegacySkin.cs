// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Catch.Skinning.Legacy;
using osu.Game.Skinning;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Catch.Tests
{
    [TestFixture]
    public class TestSceneCatchPlayerLegacySkin : LegacySkinPlayerTestScene
    {
        [Test]
        public void TestUsingLegacySkin()
        {
            // check for the existence of a random legacy component to ensure using legacy skin.
            // this should exist in LegacySkinPlayerTestScene but the weird transformer logic below needs to be "fixed" or otherwise first.
            AddAssert("using legacy skin", () => this.ChildrenOfType<LegacyScoreCounter>().Any());
        }

        protected override Ruleset CreatePlayerRuleset() => new TestCatchRuleset();

        private class TestCatchRuleset : CatchRuleset
        {
            public override ISkin CreateLegacySkinProvider(ISkinSource source, IBeatmap beatmap) => new TestCatchLegacySkinTransformer(source);
        }

        private class TestCatchLegacySkinTransformer : CatchLegacySkinTransformer
        {
            public TestCatchLegacySkinTransformer(ISkinSource source)
                : base(source)
            {
            }

            public override Drawable GetDrawableComponent(ISkinComponent component)
            {
                var drawable = base.GetDrawableComponent(component);
                if (drawable != null)
                    return drawable;

                // it shouldn't really matter whether to return null or return this,
                // but returning null skips over the beatmap skin, so this needs to exist to test things properly.
                return Source.GetDrawableComponent(component);
            }
        }
    }
}
