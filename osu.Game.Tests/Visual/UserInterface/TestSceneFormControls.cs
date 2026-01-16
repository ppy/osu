// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Game.Beatmaps;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Cursor;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Localisation;
using osu.Game.Overlays;
using osu.Game.Screens.Edit.Setup;
using osuTK;

namespace osu.Game.Tests.Visual.UserInterface
{
    public partial class TestSceneFormControls : ThemeComparisonTestScene
    {
        public TestSceneFormControls()
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
                            AutoSizeAxes = Axes.Both,
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Direction = FillDirection.Horizontal,
                            Children = new[]
                            {
                                new FillFlowContainer
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
                                        new FormTextBox
                                        {
                                            Caption = "Artist",
                                            HintText = "Poot artist here!",
                                            PlaceholderText = "Here is an artist",
                                            TabbableContentContainer = this,
                                        },
                                        new FormTextBox
                                        {
                                            Caption = "Artist",
                                            HintText = "Poot artist here!",
                                            PlaceholderText = "Here is an artist",
                                            Current = { Disabled = true },
                                            TabbableContentContainer = this,
                                        },
                                        new FormNumberBox(allowDecimals: true)
                                        {
                                            Caption = "Number",
                                            HintText = "Insert your favourite number",
                                            PlaceholderText = "Mine is 42!",
                                            TabbableContentContainer = this,
                                        },
                                        new FormCheckBox
                                        {
                                            Caption = EditorSetupStrings.LetterboxDuringBreaks,
                                            HintText = EditorSetupStrings.LetterboxDuringBreaksDescription,
                                        },
                                        new FormCheckBox
                                        {
                                            Caption = EditorSetupStrings.LetterboxDuringBreaks,
                                            HintText = EditorSetupStrings.LetterboxDuringBreaksDescription,
                                            Current = { Disabled = true },
                                        },
                                        new FormCheckBox
                                        {
                                            Caption = EditorSetupStrings.LetterboxDuringBreaks,
                                            HintText = EditorSetupStrings.LetterboxDuringBreaksDescription,
                                            Current = { Value = true, Disabled = true },
                                        },
                                        new FormSliderBar<float>
                                        {
                                            Caption = "Slider",
                                            HintText = "Slider hint",
                                            Current = new BindableFloat
                                            {
                                                MinValue = 0,
                                                MaxValue = 10,
                                                Value = 5,
                                                Precision = 0.1f,
                                            },
                                            TabbableContentContainer = this,
                                        },
                                        new FormSliderBar<float>
                                        {
                                            Caption = "Slider",
                                            HintText = "Slider hint",
                                            Current = new BindableFloat
                                            {
                                                MinValue = 0,
                                                MaxValue = 10,
                                                Value = 5,
                                                Precision = 0.1f,
                                                Disabled = true,
                                            },
                                            TransferValueOnCommit = true,
                                            TabbableContentContainer = this,
                                        },
                                        new FormSliderBar<float>
                                        {
                                            Caption = "Slider (percentage)",
                                            HintText = "Percentage slider hint",
                                            Current = new BindableFloat
                                            {
                                                MinValue = 0,
                                                MaxValue = 1,
                                                Value = 0.2f,
                                                Precision = 0.0001f,
                                            },
                                            DisplayAsPercentage = true,
                                            TabbableContentContainer = this,
                                        },
                                        new FormSliderBar<float>
                                        {
                                            Caption = "Slider (custom)",
                                            HintText = "Custom slider hint",
                                            Current = new BindableFloat
                                            {
                                                MinValue = 0,
                                                MaxValue = 1,
                                                Value = 0.2f,
                                                Precision = 0.0001f,
                                            },
                                            LabelFormat = v => $"{v * 100:0.00} funometer",
                                            TooltipFormat = v => $"This setting has the value set to {v * 100:0.00} funometer.",
                                            TabbableContentContainer = this,
                                        },
                                        new FormSliderBar<float>
                                        {
                                            Caption = "Slider (custom)",
                                            HintText = "Custom slider hint",
                                            Current = new BindableFloat
                                            {
                                                MinValue = 0,
                                                MaxValue = 1,
                                                Value = 0.2f,
                                                Precision = 0.0001f,
                                                Disabled = true,
                                            },
                                            TransferValueOnCommit = true,
                                            LabelFormat = v => $"{v * 100:0.00} funometer",
                                            TooltipFormat = v => $"This setting has the value set to {v * 100:0.00} funometer.",
                                            TabbableContentContainer = this,
                                        },
                                        new FormEnumDropdown<CountdownType>
                                        {
                                            Caption = EditorSetupStrings.EnableCountdown,
                                            HintText = EditorSetupStrings.CountdownDescription,
                                        },
                                        new FormEnumDropdown<CountdownType>
                                        {
                                            Caption = EditorSetupStrings.EnableCountdown,
                                            HintText = EditorSetupStrings.CountdownDescription,
                                            Current = { Disabled = true },
                                        },
                                        new FormFileSelector
                                        {
                                            Caption = "File selector",
                                            PlaceholderText = "Select a file",
                                        },
                                        new FormBeatmapFileSelector(true)
                                        {
                                            Caption = "File selector with intermediate choice dialog",
                                            PlaceholderText = "Select a file",
                                        },
                                        new FormColourPalette
                                        {
                                            Caption = "Combo colours",
                                            Colours =
                                            {
                                                Colour4.Red,
                                                Colour4.Green,
                                                Colour4.Blue,
                                                Colour4.Yellow,
                                            }
                                        },
                                        new FormButton
                                        {
                                            Caption = "No text in button",
                                            Action = () => { },
                                        },
                                    },
                                },
                                new FillFlowContainer
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
                                        new FormNumberBox(allowDecimals: true)
                                        {
                                            Caption = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua",
                                            HintText = "Insert your favourite number",
                                            PlaceholderText = "Mine is 42!",
                                            TabbableContentContainer = this,
                                        },
                                        new FormCheckBox
                                        {
                                            Caption = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua",
                                            HintText = EditorSetupStrings.LetterboxDuringBreaksDescription,
                                        },
                                        new FormSliderBar<float>
                                        {
                                            Caption = "Lorem ipsum dolor sit amet, conse adipiscing elit, sed do eiusmod",
                                            HintText = "Slider hint",
                                            Current = new BindableFloat
                                            {
                                                MinValue = 0,
                                                MaxValue = 10,
                                                Value = 5,
                                                Precision = 0.1f,
                                            },
                                            TabbableContentContainer = this,
                                        },
                                        new FormEnumDropdown<CountdownType>
                                        {
                                            Caption = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua",
                                            HintText = EditorSetupStrings.CountdownDescription,
                                        },
                                        new FormFileSelector
                                        {
                                            Caption = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua",
                                            HintText = EditorSetupStrings.CountdownDescription,
                                            PlaceholderText = "Select a file",
                                        },
                                        new FormColourPalette
                                        {
                                            Caption = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua",
                                            HintText = EditorSetupStrings.CountdownDescription,
                                            Colours =
                                            {
                                                Colour4.Red,
                                                Colour4.Green,
                                                Colour4.Blue,
                                                Colour4.Yellow,
                                            }
                                        },
                                    },
                                }
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
