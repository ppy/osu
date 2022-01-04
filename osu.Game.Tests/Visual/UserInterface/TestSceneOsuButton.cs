// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Game.Graphics.UserInterface;
using osuTK;

namespace osu.Game.Tests.Visual.UserInterface
{
    public class TestSceneOsuButton : OsuTestScene
    {
        [Test]
        public void TestToggleEnabled()
        {
            OsuButton button = null;

            AddStep("add button", () => Child = button = new OsuButton
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Size = new Vector2(200),
                Text = "Button"
            });

            AddToggleStep("toggle enabled", toggle =>
            {
                for (int i = 0; i < 6; i++)
                    button.Action = toggle ? () => { } : (Action)null;
            });
        }

        [Test]
        public void TestInitiallyDisabled()
        {
            AddStep("add button", () => Child = new OsuButton
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Size = new Vector2(200),
                Text = "Button"
            });
        }
    }
}
