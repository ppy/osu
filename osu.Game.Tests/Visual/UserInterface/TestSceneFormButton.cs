// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Cursor;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Overlays;
using osuTK;

namespace osu.Game.Tests.Visual.UserInterface
{
    public partial class TestSceneFormButton : ThemeComparisonTestScene
    {
        public TestSceneFormButton()
            : base(false)
        {
        }

        protected override Drawable CreateContent() => new OsuContextMenuContainer
        {
            RelativeSizeAxes = Axes.Both,
            Children = new Drawable[]
            {
                new BackgroundBox
                {
                    RelativeSizeAxes = Axes.Both,
                },
                new PopoverContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Child = new OsuScrollContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                        Child = new FillFlowContainer
                        {
                            AutoSizeAxes = Axes.Y,
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Width = 400,
                            Direction = FillDirection.Vertical,
                            Spacing = new Vector2(5),
                            Padding = new MarginPadding(10),
                            Children = new Drawable[]
                            {
                                new FormButton
                                {
                                    Caption = "Button with default style",
                                    Action = () => { },
                                },
                                new FormButton
                                {
                                    Caption = "Button with default style",
                                    Enabled = { Value = false },
                                },
                                new FormButton
                                {
                                    Caption = "Button with custom style",
                                    BackgroundColour = new OsuColour().DangerousButtonColour,
                                    ButtonIcon = FontAwesome.Solid.Hamburger,
                                    Action = () => { },
                                },
                                new FormButton
                                {
                                    Caption = "Button with custom style",
                                    BackgroundColour = new OsuColour().DangerousButtonColour,
                                    ButtonIcon = FontAwesome.Solid.Hamburger,
                                    Enabled = { Value = false },
                                },
                                new FormButton
                                {
                                    Caption = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua",
                                    BackgroundColour = new OsuColour().Blue3,
                                    ButtonIcon = FontAwesome.Solid.Book,
                                    Action = () => { },
                                },
                                new FormButton
                                {
                                    Caption = "Button with text inside",
                                    ButtonText = "Text in button",
                                    Action = () => { },
                                },
                                new FormButton
                                {
                                    Caption = "Button with text inside",
                                    ButtonText = "Text in button",
                                    Enabled = { Value = false },
                                },
                                new FormButton
                                {
                                    Caption = "Button with text inside",
                                    ButtonText = "Text in button",
                                    BackgroundColour = new OsuColour().DangerousButtonColour,
                                    Action = () => { },
                                },
                                new FormButton
                                {
                                    Caption = "Button with text inside",
                                    ButtonText = "Text in button",
                                    BackgroundColour = new OsuColour().DangerousButtonColour,
                                    Enabled = { Value = false },
                                },
                                new FormButton
                                {
                                    Caption = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor",
                                    ButtonText = "Text in button",
                                    BackgroundColour = new OsuColour().Blue3,
                                    Action = () => { },
                                },
                                new FormButton
                                {
                                    Caption = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor",
                                    ButtonText = "Text in button",
                                    BackgroundColour = new OsuColour().Blue3,
                                    Enabled = { Value = false },
                                },
                            },
                        },
                    },
                }
            }
        };

        private partial class BackgroundBox : Box
        {
            [BackgroundDependencyLoader]
            private void load(OverlayColourProvider colourProvider)
            {
                Colour = colourProvider.Background4;
            }
        }
    }
}
