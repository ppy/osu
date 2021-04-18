// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Utils;
using osu.Game.Graphics.UserInterfaceV2;
using osuTK.Graphics;

namespace osu.Game.Tests.Visual.UserInterface
{
    public class TestSceneLabelledColourPalette : OsuTestScene
    {
        private LabelledColourPalette component;

        [Test]
        public void TestPalette([Values] bool hasDescription)
        {
            createColourPalette(hasDescription);

            AddRepeatStep("add random colour", () => component.Colours.Add(randomColour()), 4);

            AddStep("set custom prefix", () => component.ColourNamePrefix = "Combo");

            AddRepeatStep("remove random colour", () =>
            {
                if (component.Colours.Count > 0)
                    component.Colours.RemoveAt(RNG.Next(component.Colours.Count));
            }, 8);
        }

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
                        ColourNamePrefix = "My colour #"
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

        private Color4 randomColour() => new Color4(
            RNG.NextSingle(),
            RNG.NextSingle(),
            RNG.NextSingle(),
            1);
    }
}
