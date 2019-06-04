// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.MathUtils;
using osu.Game.Overlays.Profile.Header.Components;
using osuTK.Graphics;
using System;
using System.Collections.Generic;

namespace osu.Game.Tests.Visual.Online
{
    public class TestSceneGamemodeControl : OsuTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(GamemodeControl),
            typeof(GamemodeTabItem),
        };

        private readonly GamemodeControl control;

        public TestSceneGamemodeControl()
        {
            Child = control = new GamemodeControl
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            };

            AddStep("set osu! as default", () => control.SetDefaultGamemode("osu"));
            AddStep("set mania as default", () => control.SetDefaultGamemode("mania"));
            AddStep("set taiko as default", () => control.SetDefaultGamemode("taiko"));
            AddStep("set catch as default", () => control.SetDefaultGamemode("fruits"));
            AddStep("select default gamemode", () => control.SelectDefaultGamemode());

            AddStep("set random colour", () => control.AccentColour = new Color4(RNG.NextSingle(), RNG.NextSingle(), RNG.NextSingle(), 1));
        }
    }
}
