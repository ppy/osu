// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
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
        private TestOverlay createOverlay() => new TestOverlay
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            RelativeSizeAxes = Axes.Both,
            Size = new Vector2(0.5f)
        };

        private void loadOverlay() => Schedule(() => Child = createOverlay());

        private void loadBlockingOverlay() => Schedule(() => Child = createOverlay().With(d =>
        {
            d.Child = new BlockingContainer
            {
                RelativeSizeAxes = Axes.Both,
            };
        }));

        [Test]
        public void TestScrollBlocked()
        {
            OsuScrollContainer scroll = null!;

            AddStep("load overlay", loadOverlay);
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
            TestGlobalAltScrollAdjustsVolume volumeAdjust = null!;

            AddStep("load overlay", loadOverlay);
            AddStep("add volume control receptor", () => Add(volumeAdjust = new TestGlobalAltScrollAdjustsVolume
            {
                RelativeSizeAxes = Axes.Both,
                Depth = float.MaxValue,
            }));

            AddStep("hold alt", () => InputManager.PressKey(Key.AltLeft));
            AddStep("perform scroll", () =>
            {
                InputManager.MoveMouseTo(Content);
                InputManager.ScrollVerticalBy(10);
            });

            AddAssert("receptor received scroll input", () => volumeAdjust.ScrollReceived);
            AddStep("release alt", () => InputManager.ReleaseKey(Key.AltLeft));
        }

        [Test]
        public void TestAltScrollBlockedByOptOut()
        {
            TestGlobalAltScrollAdjustsVolume volumeAdjust = null!;

            AddStep("add blocker and receptor", () =>
            {
                Add(volumeAdjust = new TestGlobalAltScrollAdjustsVolume
                {
                    RelativeSizeAxes = Axes.Both,
                    Depth = float.MaxValue,
                });
            });
            AddStep("load blocking overlay", loadBlockingOverlay);

            AddStep("hold alt", () => InputManager.PressKey(Key.AltLeft));
            AddStep("perform scroll", () =>
            {
                InputManager.MoveMouseTo(Content);
                InputManager.ScrollVerticalBy(10);
            });

            AddAssert("receptor did not receive scroll input", () => !volumeAdjust.ScrollReceived);
            AddStep("release alt", () => InputManager.ReleaseKey(Key.AltLeft));
        }

        public partial class TestGlobalAltScrollAdjustsVolume : GlobalAltScrollAdjustsVolume
        {
            public bool ScrollReceived { get; private set; }

            protected override bool OnScroll(ScrollEvent e)
            {
                ScrollReceived = true;
                return base.OnScroll(e);
            }
        }

        private partial class BlockingContainer : Container, IBlockGlobalAltScrollVolume;

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
