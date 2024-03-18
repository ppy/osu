// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Game.Beatmaps.Drawables.Cards;
using osu.Game.Graphics.UserInterface;
using osuTK;

namespace osu.Game.Overlays.BeatmapListing
{
    public partial class BeatmapListingCardSizeTabControl : OsuTabControl<BeatmapCardSize>
    {
        public BeatmapListingCardSizeTabControl()
        {
            AutoSizeAxes = Axes.Both;

            Items = new[] { BeatmapCardSize.Normal, BeatmapCardSize.Extra };
        }

        protected override bool AddEnumEntriesAutomatically => false;

        protected override TabFillFlowContainer CreateTabFlow() => new TabFillFlowContainer
        {
            AutoSizeAxes = Axes.Both,
            Direction = FillDirection.Horizontal,
            Spacing = new Vector2(10, 0),
        };

        protected override Dropdown<BeatmapCardSize> CreateDropdown() => null;

        protected override TabItem<BeatmapCardSize> CreateTabItem(BeatmapCardSize value) => new TabItem(value);

        private partial class TabItem : TabItem<BeatmapCardSize>
        {
            private Box background;
            private SpriteIcon icon;

            [Resolved]
            private OverlayColourProvider colourProvider { get; set; }

            public TabItem(BeatmapCardSize value)
                : base(value)
            {
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                AutoSizeAxes = Axes.Both;
                Masking = true;
                CornerRadius = 4;
                Children = new Drawable[]
                {
                    background = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = colourProvider.Background3
                    },
                    new Container
                    {
                        AutoSizeAxes = Axes.Both,
                        Padding = new MarginPadding
                        {
                            Horizontal = 10,
                            Vertical = 5,
                        },
                        Child = icon = new SpriteIcon
                        {
                            Size = new Vector2(12),
                            Icon = getIconForCardSize(Value)
                        }
                    },
                    new HoverSounds(HoverSampleSet.TabSelect)
                };
            }

            private static IconUsage getIconForCardSize(BeatmapCardSize cardSize)
            {
                switch (cardSize)
                {
                    case BeatmapCardSize.Normal:
                        return FontAwesome.Solid.Th;

                    case BeatmapCardSize.Extra:
                        return FontAwesome.Solid.ThLarge;

                    default:
                        throw new ArgumentOutOfRangeException(nameof(cardSize), cardSize, "Unsupported card size");
                }
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();
                updateState();
                FinishTransforms(true);
            }

            protected override void OnActivated()
            {
                if (IsLoaded)
                    updateState();
            }

            protected override void OnDeactivated()
            {
                if (IsLoaded)
                    updateState();
            }

            protected override bool OnHover(HoverEvent e)
            {
                updateState();
                return base.OnHover(e);
            }

            protected override void OnHoverLost(HoverLostEvent e)
            {
                updateState();
                base.OnHoverLost(e);
            }

            private const double fade_time = 200;

            private void updateState()
            {
                background.FadeTo(IsHovered || Active.Value ? 1 : 0, fade_time, Easing.OutQuint);
                icon.FadeColour(Active.Value && !IsHovered ? colourProvider.Light1 : colourProvider.Content1, fade_time, Easing.OutQuint);
            }
        }
    }
}
