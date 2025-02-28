// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays;
using osu.Game.Resources.Localisation.Web;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.SelectV2.Wedge
{
    public abstract partial class LengthAndBPMStatisticPill : CompositeDrawable
    {
        protected PillStatistic LengthStatistic = null!;
        protected PillStatistic BPMStatistic = null!;

        protected LengthAndBPMStatisticPill()
        {
            AutoSizeAxes = Axes.X;
            Height = 20;
            Masking = true;
            CornerRadius = 10;
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            InternalChildren = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = colourProvider.Background4
                },
                new FillFlowContainer
                {
                    AutoSizeAxes = Axes.X,
                    RelativeSizeAxes = Axes.Y,
                    Direction = FillDirection.Horizontal,
                    Spacing = new Vector2(10),
                    Padding = new MarginPadding { Horizontal = 20 },
                    Children = new Drawable[]
                    {
                        LengthStatistic = new PillStatistic(new BeatmapStatistic { Name = "Length" }),
                        BPMStatistic = new PillStatistic(new BeatmapStatistic { Name = BeatmapsetsStrings.ShowStatsBpm }),
                    }
                }
            };
        }

        public partial class PillStatistic : CompositeDrawable, IHasTooltip
        {
            private readonly BeatmapStatistic statistic;
            private OsuSpriteText valueSpriteText = null!;

            private LocalisableString valueText;

            public LocalisableString Value
            {
                get => valueText;
                set
                {
                    valueText = value;

                    if (IsLoaded)
                        updateValueText();
                }
            }

            private Color4 valueColour;

            public Color4 ValueColour
            {
                get => valueColour;
                set
                {
                    valueColour = value;

                    if (IsLoaded)
                        updateValueText();
                }
            }

            public PillStatistic(BeatmapStatistic statistic)
            {
                this.statistic = statistic;

                Anchor = Anchor.CentreLeft;
                Origin = Anchor.CentreLeft;
                AutoSizeAxes = Axes.Both;
            }

            [BackgroundDependencyLoader]
            private void load(OverlayColourProvider colourProvider)
            {
                valueColour = colourProvider.Content2;

                InternalChild = new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Horizontal,
                    Spacing = new Vector2(5),
                    Children = new Drawable[]
                    {
                        new OsuSpriteText
                        {
                            Text = statistic.Name,
                            Font = OsuFont.GetFont(weight: FontWeight.SemiBold, size: 14),
                        },
                        valueSpriteText = new OsuSpriteText
                        {
                            Text = statistic.Content,
                            Font = OsuFont.GetFont(weight: FontWeight.SemiBold, size: 14),
                        }
                    }
                };
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                updateValueText();
            }

            private void updateValueText()
            {
                valueSpriteText.Text = LocalisableString.IsNullOrEmpty(valueText) ? "-" : valueText;
                valueSpriteText.Colour = valueColour;
            }

            public LocalisableString TooltipText { get; set; }
        }
    }
}
