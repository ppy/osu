// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Testing;
using osu.Framework.Utils;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Overlays.Settings;
using osuTK.Graphics;
using osuTK.Input;

namespace osu.Game.Tests.Visual.UserInterface
{
    public partial class TestSceneSettingsColour : OsuManualInputManagerTestScene
    {
        private SettingsColour? component;

        [Test]
        public void TestColour()
        {
            createContent();

            AddRepeatStep("set random colour", () => component!.Current.Value = randomColour(), 4);
        }

        [Test]
        public void TestUserInteractions()
        {
            createContent();

            AddStep("click colour", () =>
            {
                InputManager.MoveMouseTo(component!);
                InputManager.Click(MouseButton.Left);
            });

            AddAssert("colour picker spawned", () => this.ChildrenOfType<OsuColourPicker>().Any());
        }

        private void createContent()
        {
            AddStep("create component", () =>
            {
                Child = new PopoverContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Child = new FillFlowContainer
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Width = 500,
                        AutoSizeAxes = Axes.Y,
                        Children = new Drawable[]
                        {
                            component = new SettingsColour
                            {
                                LabelText = "a sample component",
                            },
                        },
                    },
                };
            });
        }

        private Colour4 randomColour() => new Color4(
            RNG.NextSingle(),
            RNG.NextSingle(),
            RNG.NextSingle(),
            1);
    }
}
