// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays.Volume;
using osuTK;
using osuTK.Graphics;
using osuTK.Input;
using Box = osu.Framework.Graphics.Shapes.Box;

namespace osu.Game.Tests.Visual.UserInterface
{
    public partial class TestSceneOverlayContainer : OsuManualInputManagerTestScene
    {
        [SetUp]
        public void SetUp() => Schedule(() => Child = new TestOverlay
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            RelativeSizeAxes = Axes.Both,
            Size = new Vector2(0.5f)
        });

        [Test]
        public void TestScrollBlocked()
        {
            OsuScrollContainer scroll = null!;

            AddStep("add scroll container", () =>
            {
                Add(scroll = new OsuScrollContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Depth = float.MaxValue,
                    Child = new Box
                    {
                        RelativeSizeAxes = Axes.X,
                        Height = DrawHeight * 10,
                        Colour = ColourInfo.GradientVertical(Colour4.Black, Colour4.White),
                    }
                });
            });

            AddStep("perform scroll", () =>
            {
                InputManager.MoveMouseTo(Content);
                InputManager.ScrollVerticalBy(-10);
            });

            AddAssert("scroll didn't receive input", () => scroll.Current == 0);
        }

        [Test]
        public void TestAltScrollNotBlocked()
        {
            bool scrollReceived = false;

            AddStep("add volume control receptor", () => Add(new VolumeControlReceptor
            {
                RelativeSizeAxes = Axes.Both,
                Depth = float.MaxValue,
                ScrollActionRequested = (_, _, _) => scrollReceived = true,
            }));

            AddStep("hold alt", () => InputManager.PressKey(Key.AltLeft));
            AddStep("perform scroll", () =>
            {
                InputManager.MoveMouseTo(Content);
                InputManager.ScrollVerticalBy(10);
            });

            AddAssert("receptor received scroll input", () => scrollReceived);
            AddStep("release alt", () => InputManager.ReleaseKey(Key.AltLeft));
        }

        private partial class TestOverlay : OsuFocusedOverlayContainer
        {
            [BackgroundDependencyLoader]
            private void load()
            {
                State.Value = Visibility.Visible;

                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both
                    },
                    new OsuSpriteText
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Text = "Overlay content",
                        Colour = Color4.Black,
                    },
                };
            }

            protected override void PopIn()
            {
            }
        }
    }
}
