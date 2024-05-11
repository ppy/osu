// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.UserInterfaceV2;

namespace osu.Game.Tests.Visual.UserInterface
{
    public partial class TestSceneLabelledSwitchButton : OsuTestScene
    {
        [TestCase(false)]
        [TestCase(true)]
        public void TestSwitchButton(bool hasDescription) => createSwitchButton(hasDescription);

        private void createSwitchButton(bool hasDescription = false)
        {
            AddStep("create component", () =>
            {
                LabelledSwitchButton component;

                Child = new Container
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Width = 500,
                    AutoSizeAxes = Axes.Y,
                    Child = component = new LabelledSwitchButton
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                    }
                };

                component.Label = "a sample component";
                component.Description = hasDescription ? "this text describes the component" : string.Empty;
            });
        }
    }
}
