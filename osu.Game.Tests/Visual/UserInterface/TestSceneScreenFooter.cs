// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Testing;
using osu.Game.Overlays;
using osu.Game.Overlays.Mods;
using osu.Game.Screens.Footer;
using osu.Game.Screens.SelectV2.Footer;

namespace osu.Game.Tests.Visual.UserInterface
{
    public partial class TestSceneScreenFooter : OsuManualInputManagerTestScene
    {
        private ScreenFooter screenFooter = null!;
        private TestModSelectOverlay overlay = null!;

        [SetUp]
        public void SetUp() => Schedule(() =>
        {
            Children = new Drawable[]
            {
                overlay = new TestModSelectOverlay
                {
                    Padding = new MarginPadding
                    {
                        Bottom = ScreenFooter.HEIGHT
                    }
                },
                new PopoverContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Child = screenFooter = new ScreenFooter(),
                },
            };

            screenFooter.SetButtons(new ScreenFooterButton[]
            {
                new ScreenFooterButtonMods(overlay) { Current = SelectedMods },
                new ScreenFooterButtonRandom(),
                new ScreenFooterButtonOptions(),
            });
        });

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("show footer", () => screenFooter.Show());
        }

        /// <summary>
        /// Transition when moving from a screen with no buttons to a screen with buttons.
        /// </summary>
        [Test]
        public void TestButtonsIn()
        {
        }

        /// <summary>
        /// Transition when moving from a screen with buttons to a screen with no buttons.
        /// </summary>
        [Test]
        public void TestButtonsOut()
        {
            AddStep("clear buttons", () => screenFooter.SetButtons(Array.Empty<ScreenFooterButton>()));
        }

        /// <summary>
        /// Transition when moving from a screen with buttons to a screen with buttons.
        /// </summary>
        [Test]
        public void TestReplaceButtons()
        {
            AddStep("replace buttons", () => screenFooter.SetButtons(new[]
            {
                new ScreenFooterButton { Text = "One", Action = () => { } },
                new ScreenFooterButton { Text = "Two", Action = () => { } },
                new ScreenFooterButton { Text = "Three", Action = () => { } },
            }));
        }

        private partial class TestModSelectOverlay : UserModSelectOverlay
        {
            protected override bool ShowPresets => true;

            public TestModSelectOverlay()
                : base(OverlayColourScheme.Aquamarine)
            {
            }
        }
    }
}
