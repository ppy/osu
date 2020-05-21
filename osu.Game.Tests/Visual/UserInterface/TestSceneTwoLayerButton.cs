// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osuTK;

namespace osu.Game.Tests.Visual.UserInterface
{
    public class TestSceneTwoLayerButton : OsuTestScene
    {
        public TestSceneTwoLayerButton()
        {
            Add(new TwoLayerButton
            {
                Position = new Vector2(100),
                Text = "button",
                Icon = FontAwesome.Solid.Check,
                BackgroundColour = Colour4.SlateGray,
                HoverColour = Colour4.SlateGray.Darken(0.2f)
            });
        }
    }
}
