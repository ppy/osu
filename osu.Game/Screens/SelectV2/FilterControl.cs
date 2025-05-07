// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Configuration;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Localisation;
using osu.Game.Resources.Localisation.Web;
using osu.Game.Screens.Select.Filter;
using osuTK;

namespace osu.Game.Screens.SelectV2
{
    public partial class FilterControl : OverlayContainer
    {
        // taken from draw visualiser. used for carousel alignment purposes.
        public const float HEIGHT_FROM_SCREEN_TOP = 141 - corner_radius;

        private const float corner_radius = 8;

        private ShearedToggleButton showConvertedBeatmapsButton = null!;
        private DifficultyRangeSlider difficultyRangeSlider = null!;

        [Resolved]
        private OsuConfigManager config { get; set; } = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            Shear = OsuGame.SHEAR;
            Margin = new MarginPadding { Top = -corner_radius, Right = -40 };

            InternalChildren = new Drawable[]
            {
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    CornerRadius = corner_radius,
                    Masking = true,
                    Child = new WedgeBackground
                    {
                        Anchor = Anchor.TopRight,
                        Scale = new Vector2(-1, 1),
                    }
                },
                new ReverseChildIDFillFlowContainer<Drawable>
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(0f, 5f),
                    Padding = new MarginPadding { Top = corner_radius + 5, Bottom = 2, Right = 40f, Left = 2f },
                    Children = new Drawable[]
                    {
                        new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Shear = -OsuGame.SHEAR,
                            Child = new SongSelectSearchTextBox
                            {
                                RelativeSizeAxes = Axes.X,
                                HoldFocus = true,
                                // TODO: pending implementation
                                FilterText = "12345 matches",
                            },
                        },
                        new GridContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Shear = -OsuGame.SHEAR,
                            RowDimensions = new[] { new Dimension(GridSizeMode.AutoSize) },
                            ColumnDimensions = new[]
                            {
                                new Dimension(),
                                new Dimension(GridSizeMode.Absolute), // can probably be removed?
                                new Dimension(GridSizeMode.AutoSize),
                            },
                            Content = new[]
                            {
                                new[]
                                {
                                    difficultyRangeSlider = new DifficultyRangeSlider
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        MinRange = 0.1f,
                                    },
                                    Empty(),
                                    showConvertedBeatmapsButton = new ShearedToggleButton
                                    {
                                        Anchor = Anchor.Centre,
                                        Origin = Anchor.Centre,
                                        Text = UserInterfaceStrings.ShowConverts,
                                        Height = 30f,
                                    },
                                },
                            }
                        },
                        new GridContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            Height = 30,
                            Shear = -OsuGame.SHEAR,
                            RowDimensions = new[] { new Dimension(GridSizeMode.AutoSize) },
                            ColumnDimensions = new[]
                            {
                                new Dimension(maxSize: 210),
                                new Dimension(GridSizeMode.Absolute, 5),
                                new Dimension(maxSize: 230),
                                new Dimension(GridSizeMode.Absolute, 5),
                                new Dimension(),
                            },
                            Content = new[]
                            {
                                new[]
                                {
                                    new ShearedDropdown<SortMode>(SortStrings.Default)
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        Items = Enum.GetValues<SortMode>(),
                                    },
                                    Empty(),
                                    // todo: pending localisation
                                    new ShearedDropdown<GroupMode>("Group by")
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        Items = Enum.GetValues<GroupMode>(),
                                    },
                                    Empty(),
                                    new CollectionDropdown
                                    {
                                        RelativeSizeAxes = Axes.X,
                                    },
                                }
                            }
                        },
                    },
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            difficultyRangeSlider.LowerBound = config.GetBindable<double>(OsuSetting.DisplayStarsMinimum);
            difficultyRangeSlider.UpperBound = config.GetBindable<double>(OsuSetting.DisplayStarsMaximum);
            config.BindWith(OsuSetting.ShowConvertedBeatmaps, showConvertedBeatmapsButton.Active);
        }

        protected override void PopIn()
        {
            this.MoveToX(0, SongSelect.ENTER_DURATION, Easing.OutQuint)
                .FadeIn(SongSelect.ENTER_DURATION / 3, Easing.In);
        }

        protected override void PopOut()
        {
            this.MoveToX(150, SongSelect.ENTER_DURATION, Easing.OutQuint)
                .FadeOut(SongSelect.ENTER_DURATION / 3, Easing.In);
        }

        private partial class SongSelectSearchTextBox : ShearedFilterTextBox
        {
            protected override InnerSearchTextBox CreateInnerTextBox() => new InnerTextBox();

            private partial class InnerTextBox : InnerFilterTextBox
            {
                public override bool HandleLeftRightArrows => false;
            }
        }
    }
}
