// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Testing;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays.Mods;
using osu.Game.Overlays.Mods.Sections;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mania;
using osu.Game.Rulesets.Mania.Mods;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.UI;
using osu.Game.Screens.Play.HUD;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tests.Visual.UserInterface
{
    public class TestSceneModButton : OsuTestScene
    {
        private ModButton modButton;

        [SetUp]
        public void SetUp() => Schedule(() =>
        {
            Children = new Drawable[]
            {
                modButton = new ModButton(new MultiMod(new TestMod1(), new TestMod2(), new TestMod3(), new TestMod4()))
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre
                }
            };
        });

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
