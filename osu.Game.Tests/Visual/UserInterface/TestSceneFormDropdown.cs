// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
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
using osuTK;

namespace osu.Game.Tests.Visual.UserInterface
{
    public partial class TestSceneFormDropdown : ThemeComparisonTestScene
    {
        public TestSceneFormDropdown()
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
                                new FormDropdown<string>
                                {
                                    Caption = "Custom dropdown",
                                    HintText = "Custom dropdown hint",
                                    Items = new[]
                                    {
                                        "A verrry looooongggg thiiiinngggggg toooooo fittttt iiinnnn thhiisssss droooppdddoowwwnn",
                                        "B verrry looooongggg thiiiinngggggg toooooo fittttt iiinnnn thhiisssss droooppdddoowwwnn",
                                        "C verrry looooongggg thiiiinngggggg toooooo fittttt iiinnnn thhiisssss droooppdddoowwwnn",
                                        "D verrry looooongggg thiiiinngggggg toooooo fittttt iiinnnn thhiisssss droooppdddoowwwnn",
                                    },
                                },
                                new FormDropdown<string>
                                {
                                    Caption = "Custom dropdown",
                                    HintText = "Custom dropdown hint",
                                    AlwaysShowSearchBar = true,
                                    Items = new[]
                                    {
                                        "A verrry looooongggg thiiiinngggggg toooooo fittttt iiinnnn thhiisssss droooppdddoowwwnn",
                                        "B verrry looooongggg thiiiinngggggg toooooo fittttt iiinnnn thhiisssss droooppdddoowwwnn",
                                        "C verrry looooongggg thiiiinngggggg toooooo fittttt iiinnnn thhiisssss droooppdddoowwwnn",
                                        "D verrry looooongggg thiiiinngggggg toooooo fittttt iiinnnn thhiisssss droooppdddoowwwnn",
                                    },
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
