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
    public class TestSceneProfileRulesetSelector : OsuTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(ProfileRulesetSelector),
            typeof(RulesetTabItem),
        };

        public TestSceneProfileRulesetSelector()
        {
            ProfileRulesetSelector selector;

            Child = selector = new ProfileRulesetSelector
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            };

            AddStep("set osu! as default", () => selector.SetDefaultGamemode("osu"));
            AddStep("set mania as default", () => selector.SetDefaultGamemode("mania"));
            AddStep("set taiko as default", () => selector.SetDefaultGamemode("taiko"));
            AddStep("set catch as default", () => selector.SetDefaultGamemode("fruits"));
            AddStep("select default gamemode", selector.SelectDefaultGamemode);

            AddStep("set random colour", () => selector.AccentColour = new Color4(RNG.NextSingle(), RNG.NextSingle(), RNG.NextSingle(), 1));
        }
    }
}
