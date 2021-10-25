// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Testing;
using osu.Framework.Utils;
using osu.Game.Graphics.Cursor;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osuTK.Graphics;
using osuTK.Input;

namespace osu.Game.Tests.Visual.UserInterface
{
    public class TestSceneLabelledColourPalette : OsuManualInputManagerTestScene
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

        [Test]
        public void TestUserInteractions()
        {
            createColourPalette();
            assertColourCount(4);

            clickAddColour();
            assertColourCount(5);

            deleteFirstColour();
            assertColourCount(4);

            clickFirstColour();
            AddAssert("colour picker spawned", () => this.ChildrenOfType<OsuColourPicker>().Any());
        }

        private void createColourPalette(bool hasDescription = false)
        {
            AddStep("create component", () =>
            {
                Child = new PopoverContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Child = new OsuContextMenuContainer
                    {
                        RelativeSizeAxes = Axes.Both,
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
                        }
                    }
                };

                component.Label = "a sample component";
                component.Description = hasDescription ? "this text describes the component" : string.Empty;

                component.Colours.AddRange(new[]
                {
                    Colour4.DarkRed,
                    Colour4.Aquamarine,
                    Colour4.Goldenrod,
                    Colour4.Gainsboro
                });
            });
        }

        private Colour4 randomColour() => new Color4(
            RNG.NextSingle(),
            RNG.NextSingle(),
            RNG.NextSingle(),
            1);

        private void assertColourCount(int count) => AddAssert($"colour count is {count}", () => component.Colours.Count == count);

        private void clickAddColour() => AddStep("click new colour button", () =>
        {
            InputManager.MoveMouseTo(this.ChildrenOfType<ColourPalette.AddColourButton>().Single());
            InputManager.Click(MouseButton.Left);
        });

        private void clickFirstColour() => AddStep("click first colour", () =>
        {
            InputManager.MoveMouseTo(this.ChildrenOfType<ColourDisplay>().First());
            InputManager.Click(MouseButton.Left);
        });

        private void deleteFirstColour()
        {
            AddStep("right-click first colour", () =>
            {
                InputManager.MoveMouseTo(this.ChildrenOfType<ColourDisplay>().First());
                InputManager.Click(MouseButton.Right);
            });

            AddUntilStep("wait for menu", () => this.ChildrenOfType<OsuContextMenu>().Any());

            AddStep("click delete", () =>
            {
                InputManager.MoveMouseTo(this.ChildrenOfType<DrawableOsuMenuItem>().Single());
                InputManager.Click(MouseButton.Left);
            });
        }
    }
}
