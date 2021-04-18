// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.UserInterfaceV2;
using osuTK.Graphics;

namespace osu.Game.Tests.Visual.UserInterface
{
    public class TestSceneLabelledColourPalette : OsuTestScene
    {
        private LabelledColourPalette component;

        [Test]
        public void TestPalette([Values] bool hasDescription) => createColourPalette(hasDescription);

        private void createColourPalette(bool hasDescription = false)
        {
            AddStep("create component", () =>
            {
                Child = new Container
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Width = 500,
                    AutoSizeAxes = Axes.Y,
                    Child = component = new LabelledColourPalette
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                    }
                };

                component.Label = "a sample component";
                component.Description = hasDescription ? "this text describes the component" : string.Empty;

                component.Colours.AddRange(new[]
                {
                    Color4.DarkRed,
                    Color4.Aquamarine,
                    Color4.Goldenrod,
                    Color4.Gainsboro
                });
            });
        }
    }
}
