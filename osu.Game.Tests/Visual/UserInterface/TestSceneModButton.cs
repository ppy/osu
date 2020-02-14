// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Overlays.Mods;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Tests.Visual.UserInterface
{
    public class TestSceneModButton : OsuTestScene
    {
        public TestSceneModButton()
        {
            Children = new Drawable[]
            {
                new ModButton(new MultiMod(new TestMod1(), new TestMod2(), new TestMod3(), new TestMod4()))
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre
                }
            };
        }

        private class TestMod1 : TestMod
        {
            public override string Name => "Test mod 1";

            public override string Acronym => "M1";
        }

        private class TestMod2 : TestMod
        {
            public override string Name => "Test mod 2";

            public override string Acronym => "M2";

            public override IconUsage? Icon => FontAwesome.Solid.Exclamation;
        }

        private class TestMod3 : TestMod
        {
            public override string Name => "Test mod 3";

            public override string Acronym => "M3";

            public override IconUsage? Icon => FontAwesome.Solid.ArrowRight;
        }

        private class TestMod4 : TestMod
        {
            public override string Name => "Test mod 4";

            public override string Acronym => "M4";
        }

        private abstract class TestMod : Mod, IApplicableMod
        {
            public override double ScoreMultiplier => 1.0;
        }
    }
}
