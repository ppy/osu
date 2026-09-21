// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Cursor;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Localisation;
using osu.Game.Overlays;
using osu.Game.Overlays.Settings;
using osu.Game.Tests.Visual.UserInterface;
using osuTK;

namespace osu.Game.Tests.Visual.Settings
{
    public partial class TestSceneSettingsItemV2 : ThemeComparisonTestScene
    {
        private readonly Bindable<SettingsNote.Data?> note = new Bindable<SettingsNote.Data?>();

        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Purple);

        private FormSliderBar<float> sliderBar = null!;

        private SearchContainer searchContainer = null!;

        public TestSceneSettingsItemV2()
            : base(false)
        {
        }

        protected override Drawable CreateContent()
        {
            return new Container
            {
                RelativeSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    new BackgroundBox
                    {
                        RelativeSizeAxes = Axes.Both,
                    },
                    new OsuContextMenuContainer
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Width = 400,
                        RelativeSizeAxes = Axes.Y,
                        Child = new PopoverContainer
                        {
                            RelativeSizeAxes = Axes.Both,
                            Child = new OsuScrollContainer
                            {
                                RelativeSizeAxes = Axes.Both,
                                ScrollbarVisible = false,
                                Child = searchContainer = new SearchContainer
                                {
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Direction = FillDirection.Vertical,
                                    Spacing = new Vector2(7),
                                    Padding = new MarginPadding { Vertical = 10 },
                                    Children = new[]
                                    {
                                        new SettingsItemV2(new FormTextBox
                                        {
                                            Caption = "Artist",
                                            HintText = "Poot artist here!",
                                            PlaceholderText = "Here is an artist",
                                            Current = { Value = string.Empty, Default = string.Empty }
                                        }),
                                        new SettingsItemV2(new FormTextBox
                                        {
                                            Caption = "Artist",
                                            HintText = "Poot artist here!",
                                            PlaceholderText = "Here is an artist",
                                            Current = { Value = string.Empty, Default = string.Empty, Disabled = true }
                                        }),
                                        new SettingsItemV2(new FormNumberBox(allowDecimals: true)
                                        {
                                            Caption = "Number",
                                            HintText = "Insert your favourite number",
                                            PlaceholderText = "Mine is 42!",
                                            Current = { Value = string.Empty, Default = string.Empty }
                                        }),
                                        new SettingsItemV2(new FormCheckBox
                                        {
                                            Caption = EditorSetupStrings.LetterboxDuringBreaks,
                                            HintText = EditorSetupStrings.LetterboxDuringBreaksDescription,
                                        })
                                        {
                                            Note = { BindTarget = note },
                                        },
                                        new SettingsItemV2(new FormCheckBox
                                        {
                                            Caption = EditorSetupStrings.LetterboxDuringBreaks,
                                            HintText = EditorSetupStrings.LetterboxDuringBreaksDescription,
                                            Current = { Disabled = true },
                                        }),
                                        new SettingsItemV2(new FormCheckBox
                                        {
                                            Caption = EditorSetupStrings.LetterboxDuringBreaks,
                                            HintText = EditorSetupStrings.LetterboxDuringBreaksDescription,
                                            Current = { Value = true, Disabled = true },
                                        }),
                                        new SettingsItemV2(new FormEnumDropdown<CountdownType>
                                        {
                                            Caption = EditorSetupStrings.EnableCountdown,
                                            HintText = EditorSetupStrings.CountdownDescription,
                                        }),
                                        new SettingsItemV2(new FormEnumDropdown<CountdownType>
                                        {
                                            Caption = EditorSetupStrings.EnableCountdown,
                                            HintText = EditorSetupStrings.CountdownDescription,
                                            Current = { Disabled = true },
                                        }),
                                        new SettingsItemV2(new FormEnumDropdown<Language>
                                        {
                                            Caption = "Dropdown with many items",
                                            HintText = EditorSetupStrings.CountdownDescription,
                                        })
                                        {
                                            Note = { BindTarget = note },
                                        },
                                        new SettingsItemV2(sliderBar = new FormSliderBar<float>
                                        {
                                            Caption = "Slider",
                                            Current = new BindableFloat
                                            {
                                                MinValue = 0,
                                                MaxValue = 10,
                                                Value = 5,
                                                Precision = 0.1f,
                                            },
                                        }),
                                        new SettingsItemV2(new FormSliderBar<float>
                                        {
                                            Caption = "Slider",
                                            Current = new BindableFloat
                                            {
                                                MinValue = 0,
                                                MaxValue = 10,
                                                Value = 5,
                                                Precision = 0.1f,
                                                Disabled = true,
                                            },
                                            TransferValueOnCommit = true,
                                        }),
                                        new SettingsItemV2(new FormSliderBar<float>
                                        {
                                            Caption = "Slider without revert button",
                                            Current = new BindableFloat
                                            {
                                                MinValue = 0,
                                                MaxValue = 10,
                                                Value = 5,
                                                Precision = 0.1f,
                                            },
                                        })
                                        {
                                            ShowRevertToDefaultButton = false
                                        },
                                        new SettingsItemV2(new FormSliderBar<float>
                                        {
                                            Caption = "Slider with classic default",
                                            Current = new BindableFloat
                                            {
                                                MinValue = 0,
                                                MaxValue = 10,
                                                Value = 5,
                                                Precision = 0.1f,
                                            },
                                        })
                                        {
                                            ApplyClassicDefault = c => ((IHasCurrentValue<float>)c).Current.Value = 2,
                                        },
                                    },
                                },
                            },
                        }
                    },
                },
            };
        }

        [Test]
        public void TestDisplay()
        {
            AddStep("display", () => CreateThemedContent(OverlayColourScheme.Purple));
        }

        [Test]
        public void TestNote()
        {
            AddStep("set informational note", () => note.Value = new SettingsNote.Data(LayoutSettingsStrings.OsuIsRunningExclusiveFullscreen.ToString(), SettingsNote.Type.Informational));
            AddStep("set warning note",
                () => note.Value = new SettingsNote.Data(
                    "Using unlimited frame limiter can lead to stutters, bad performance and overheating. It will not improve perceived latency. “2x refresh rate” is recommended.",
                    SettingsNote.Type.Warning));
            AddStep("set critical note",
                () => note.Value = new SettingsNote.Data(
                    "You have done something so horrible in the game settings to the point we have invented a new note type for this. Look at it, it's in red. It's worse than yellow.",
                    SettingsNote.Type.Critical));
            AddStep("clear note", () => note.Value = null);
        }

        [Test]
        public void TestClassicDefault()
        {
            AddStep("modify irrelevant setting", () => sliderBar.Current.Value = 4);
            AddStep("apply classic defaults", () => this.ChildrenOfType<ISettingsItem>().Where(i => i.HasClassicDefault).ForEach(s => s.ApplyClassicDefault()));
            AddStep("apply regular defaults", () => this.ChildrenOfType<ISettingsItem>().Where(i => i.HasClassicDefault).ForEach(s => s.ApplyDefault()));
            AddStep("set classic filter", () => searchContainer.SearchTerm = SettingsItemV2.CLASSIC_DEFAULT_SEARCH_TERM);
            AddStep("apply classic defaults", () => this.ChildrenOfType<ISettingsItem>().Where(i => i.HasClassicDefault).ForEach(s => s.ApplyClassicDefault()));
            AddStep("apply regular defaults", () => this.ChildrenOfType<ISettingsItem>().Where(i => i.HasClassicDefault).ForEach(s => s.ApplyDefault()));
            AddStep("set no filter", () => searchContainer.SearchTerm = string.Empty);
            AddAssert("irrelevant setting left out", () => sliderBar.Current.Value, () => Is.EqualTo(4));
        }

        /// <summary>
        /// Ensures that the reset to default button uses the correct implementation of IsDefault to determine whether it should be shown or not.
        /// Values have been chosen so that after being set, Value != Default (but they are close enough that the difference is negligible compared to Precision).
        /// </summary>
        [TestCase(4.2f)]
        [TestCase(9.9f)]
        public void TestRestoreDefaultValueButtonPrecision(float initialValue)
        {
            BindableFloat current = null!;
            SettingsRevertToDefaultButton revertToDefaultButton = null!;

            AddStep("set current bindable", () => sliderBar.Current = current = new BindableFloat(initialValue)
            {
                MinValue = 0,
                MaxValue = 10,
                Precision = 0.1f,
            });

            AddStep("retrieve restore default button", () => revertToDefaultButton = sliderBar.FindClosestParent<SettingsItemV2>().ChildrenOfType<SettingsRevertToDefaultButton>().Single());

            AddAssert("restore button hidden", () => revertToDefaultButton.X == 0);

            AddStep("change value to next closest", () => sliderBar.Current.Value += current.Precision * 0.6f);
            AddUntilStep("restore button shown", () => revertToDefaultButton.X > 0);

            AddStep("restore default", () => sliderBar.Current.SetDefault());
            AddUntilStep("restore button hidden", () => revertToDefaultButton.X == 0);
        }

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
