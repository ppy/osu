// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Game.Beatmaps;
using osu.Game.Graphics.Cursor;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Localisation;
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
            Child = new PopoverContainer
            {
                RelativeSizeAxes = Axes.Both,
                Child = new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.Y,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
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
                        new FormNumberBox
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
                        new FormSliderBar<float>
                        {
                            Caption = "Slider",
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
                            Caption = EditorSetupStrings.EnableCountdown,
                            HintText = EditorSetupStrings.CountdownDescription,
                        },
                        new FormFileSelector
                        {
                            Caption = "Audio file",
                            PlaceholderText = "Select an audio file",
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
                    },
                },
            }
        };
    }
}
