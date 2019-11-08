// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Tests.Visual.UserInterface.MenuItems
{
    public class TestSceneThreeStateMenuItem : OsuTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(OsuMenu),
            typeof(ThreeStateMenuItem),
            typeof(DrawableStatefulMenuItem)
        };

        public TestSceneThreeStateMenuItem()
        {
            Add(new OsuMenu(Direction.Vertical, true)
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Items = new[]
                {
                    new ThreeStateMenuItem("First"),
                    new ThreeStateMenuItem("Second") { State = { Value = ThreeStates.Indeterminate } },
                    new ThreeStateMenuItem("Third") { State = { Value = ThreeStates.Enabled } },
                }
            });
        }
    }
}
