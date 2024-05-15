// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Testing;
using osu.Game.Overlays;
using osu.Game.Overlays.Mods;
using osu.Game.Screens.Footer;
using osu.Game.Screens.SelectV2.Footer;

namespace osu.Game.Tests.Visual.UserInterface
{
    public partial class TestSceneScreenFooter : OsuManualInputManagerTestScene
    {
        private FooterV2 footer = null!;
        private TestModSelectOverlay overlay = null!;

        [Cached]
        private OverlayColourProvider colourProvider { get; set; } = new OverlayColourProvider(OverlayColourScheme.Aquamarine);

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
                    Child = footer = new FooterV2(),
                },
            };

            footer.SetButtons(new FooterButtonV2[]
            {
                new FooterButtonModsV2(overlay) { Current = SelectedMods },
                new FooterButtonRandomV2(),
                new FooterButtonOptionsV2(),
            });
        });

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("show footer", () => footer.Show());
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
            AddStep("clear buttons", () => footer.SetButtons(Array.Empty<FooterButtonV2>()));
        }

        /// <summary>
        /// Transition when moving from a screen with buttons to a screen with buttons.
        /// </summary>
        [Test]
        public void TestReplaceButtons()
        {
            AddStep("replace buttons", () => footer.SetButtons(new[]
            {
                new FooterButtonV2 { Text = "One", Icon = FontAwesome.Solid.ArrowUp, Action = () => { } },
                new FooterButtonV2 { Text = "Two", Icon = FontAwesome.Solid.ArrowLeft, Action = () => { } },
                new FooterButtonV2 { Text = "Three", Icon = FontAwesome.Solid.ArrowDown, Action = () => { } },
            }));
        }

        private partial class TestModSelectOverlay : UserModSelectOverlay
        {
            protected override bool ShowPresets => true;
        }
    }
}
