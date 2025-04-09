// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Collections;
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
    public partial class BeatmapFilterControl : OverlayContainer
    {
        public const float HEIGHT = 142;

        private ShearedToggleButton showConvertedBeatmapsButton = null!;
        private ShearedDifficultyRangeSlider difficultyRangeSlider = null!;

        [Resolved]
        private OsuConfigManager config { get; set; } = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            Height = HEIGHT;

            Shear = new Vector2(OsuGame.SHEAR, 0);
            Margin = new MarginPadding { Right = -30 };

            InternalChildren = new Drawable[]
            {
                new WedgeBackground
                {
                    Anchor = Anchor.TopRight,
                    Scale = new Vector2(-1, 1),
                    FinalAlpha = 0,
                },
                new ReverseChildIDFillFlowContainer<Drawable>
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(0f, 8f),
                    Padding = new MarginPadding { Vertical = 5f, Right = 30f },
                    Children = new Drawable[]
                    {
                        new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Shear = -new Vector2(OsuGame.SHEAR, 0),
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
                            Shear = -new Vector2(OsuGame.SHEAR, 0),
                            RowDimensions = new[] { new Dimension(GridSizeMode.AutoSize) },
                            ColumnDimensions = new[]
                            {
                                new Dimension(),
                                new Dimension(GridSizeMode.Absolute, 10),
                                new Dimension(GridSizeMode.AutoSize),
                            },
                            Content = new[]
                            {
                                new[]
                                {
                                    difficultyRangeSlider = new ShearedDifficultyRangeSlider
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        MinRange = 0.1f,
                                    },
                                    Empty(),
                                    new Container
                                    {
                                        Size = new Vector2(210, 30),
                                        Child = showConvertedBeatmapsButton = new ShearedToggleButton
                                        {
                                            Anchor = Anchor.Centre,
                                            Origin = Anchor.Centre,
                                            Text = UserInterfaceStrings.ShowConvertedBeatmaps,
                                            Height = 30f,
                                        },
                                    },
                                },
                            }
                        },
                        new GridContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Shear = -new Vector2(OsuGame.SHEAR, 0),
                            RowDimensions = new[] { new Dimension(GridSizeMode.AutoSize) },
                            ColumnDimensions = new[]
                            {
                                new Dimension(maxSize: 210),
                                new Dimension(GridSizeMode.Absolute, 10),
                                new Dimension(maxSize: 230),
                                new Dimension(GridSizeMode.Absolute, 10),
                                new Dimension(),
                            },
                            Content = new[]
                            {
                                new[]
                                {
                                    new Container
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        Height = 32f,
                                        Child = new ShearedDropdown<SortMode>(SortStrings.Default)
                                        {
                                            RelativeSizeAxes = Axes.X,
                                            Items = Enum.GetValues<SortMode>(),
                                        },
                                    },
                                    Empty(),
                                    new Container
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        Height = 32f,
                                        // todo: pending localisation
                                        Child = new ShearedDropdown<GroupMode>("Group by")
                                        {
                                            RelativeSizeAxes = Axes.X,
                                            Items = Enum.GetValues<GroupMode>(),
                                        },
                                    },
                                    Empty(),
                                    new Container
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        Height = 32f,
                                        Child = new ShearedCollectionDropdown
                                        {
                                            RelativeSizeAxes = Axes.X,
                                        },
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
