// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays.BeatmapListing
{
    public class BeatmapSearchFilterRow<T> : CompositeDrawable, IHasCurrentValue<T>
    {
        private readonly BindableWithCurrent<T> current = new BindableWithCurrent<T>();

        public Bindable<T> Current
        {
            get => current.Current;
            set => current.Current = value;
        }

        public BeatmapSearchFilterRow(string headerName)
        {
            AutoSizeAxes = Axes.Y;
            RelativeSizeAxes = Axes.X;
            AddInternal(new GridContainer
            {
                AutoSizeAxes = Axes.Y,
                RelativeSizeAxes = Axes.X,
                ColumnDimensions = new[]
                {
                    new Dimension(GridSizeMode.Absolute, size: 100),
                    new Dimension()
                },
                RowDimensions = new[]
                {
                    new Dimension(GridSizeMode.AutoSize)
                },
                Content = new[]
                {
                    new Drawable[]
                    {
                        new OsuSpriteText
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Font = OsuFont.GetFont(size: 10),
                            Text = headerName.ToUpper()
                        },
                        CreateFilter().With(f =>
                        {
                            f.Anchor = Anchor.CentreLeft;
                            f.Origin = Anchor.CentreLeft;
                            f.Current = current;
                        })
                    }
                }
            });
        }

        [NotNull]
        protected virtual BeatmapSearchFilter CreateFilter() => new BeatmapSearchFilter();

        protected class BeatmapSearchFilter : TabControl<T>
        {
            public BeatmapSearchFilter()
            {
                AutoSizeAxes = Axes.Both;

                if (typeof(T).IsEnum)
                {
                    foreach (var val in (T[])Enum.GetValues(typeof(T)))
                        AddItem(val);
                }
            }

            protected override Dropdown<T> CreateDropdown() => null;

            protected override TabItem<T> CreateTabItem(T value) => new FilterTabItem(value);

            protected override TabFillFlowContainer CreateTabFlow() => new TabFillFlowContainer
            {
                AutoSizeAxes = Axes.Both,
                Spacing = new Vector2(10),
            };

            protected class FilterTabItem : TabItem<T>
            {
                protected virtual float TextSize => 13;

                [Resolved]
                private OverlayColourProvider colourProvider { get; set; }

                private readonly OsuSpriteText text;

                public FilterTabItem(T value)
                    : base(value)
                {
                    AutoSizeAxes = Axes.Both;
                    AddRangeInternal(new Drawable[]
                    {
                        text = new OsuSpriteText
                        {
                            Font = OsuFont.GetFont(size: TextSize, weight: FontWeight.Regular),
                            Text = GetText(value)
                        },
                        new HoverClickSounds()
                    });

                    Enabled.Value = true;
                }

                [BackgroundDependencyLoader]
                private void load()
                {
                    updateState();
                }

                protected virtual string GetText(T value) => (value as Enum)?.GetDescription() ?? value.ToString();

                protected override bool OnHover(HoverEvent e)
                {
                    base.OnHover(e);
                    updateState();
                    return true;
                }

                protected override void OnHoverLost(HoverLostEvent e)
                {
                    base.OnHoverLost(e);
                    updateState();
                }

                protected override void OnActivated() => updateState();

                protected override void OnDeactivated() => updateState();

                private void updateState() => text.FadeColour(Active.Value ? Color4.White : getStateColour(), 200, Easing.OutQuint);

                private Color4 getStateColour() => IsHovered ? colourProvider.Light1 : colourProvider.Light3;
            }
        }
    }
}
