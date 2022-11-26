// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Testing;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays;
using osu.Game.Overlays.Mods;
using osuTK;
using osuTK.Graphics;
using osuTK.Input;

namespace osu.Game.Tests.Visual.UserInterface
{
    [TestFixture]
    public partial class TestSceneShearedOverlayContainer : OsuManualInputManagerTestScene
    {
        private TestShearedOverlayContainer overlay;

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("create overlay", () =>
            {
                Child = overlay = new TestShearedOverlayContainer
                {
                    State = { Value = Visibility.Visible }
                };
            });
        }

        [Test]
        public void TestClickAwayToExit()
        {
            AddStep("click inside header", () =>
            {
                InputManager.MoveMouseTo(overlay.ChildrenOfType<ShearedOverlayHeader>().First().ScreenSpaceDrawQuad.Centre);
                InputManager.Click(MouseButton.Left);
            });

            AddAssert("overlay not dismissed", () => overlay.State.Value == Visibility.Visible);

            AddStep("click inside content", () =>
            {
                InputManager.MoveMouseTo(overlay.ScreenSpaceDrawQuad.Centre);
                InputManager.Click(MouseButton.Left);
            });

            AddAssert("overlay not dismissed", () => overlay.State.Value == Visibility.Visible);

            AddStep("click outside header", () =>
            {
                InputManager.MoveMouseTo(new Vector2(overlay.ScreenSpaceDrawQuad.TopLeft.X, overlay.ScreenSpaceDrawQuad.Centre.Y));
                InputManager.Click(MouseButton.Left);
            });

            AddAssert("overlay dismissed", () => overlay.State.Value == Visibility.Hidden);
        }

        public partial class TestShearedOverlayContainer : ShearedOverlayContainer
        {
            public TestShearedOverlayContainer()
                : base(OverlayColourScheme.Green)
            {
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                Header.Title = "Sheared overlay header";
                Header.Description = string.Join(" ", Enumerable.Repeat("This is a description.", 20));

                MainAreaContent.Child = new InputBlockingContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(0.9f),
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            Colour = Color4.Blue,
                            RelativeSizeAxes = Axes.Both,
                        },
                        new OsuSpriteText
                        {
                            Font = OsuFont.Default.With(size: 24),
                            Text = "Content",
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                        }
                    }
                };
            }
        }
    }
}
