// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osuTK;
using osuTK.Graphics;
using FontWeight = osu.Game.Graphics.FontWeight;

namespace osu.Game.Overlays.BeatmapListing
{
    public partial class BeatmapSearchMultipleSelectionFilterRow<T> : BeatmapSearchFilterRow<List<T>>
        where T : Enum
    {
        public new readonly BindableList<T> Current = new BindableList<T>();

        private MultipleSelectionFilter filter = null!;

        public BeatmapSearchMultipleSelectionFilterRow(LocalisableString header)
            : base(header)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            filter.Current.BindTo(Current);
        }

        protected sealed override Drawable CreateFilter() => filter = CreateMultipleSelectionFilter();

        /// <summary>
        /// Creates a filter control that can be used to simultaneously select multiple values of type <typeparamref name="T"/>.
        /// </summary>
        protected virtual MultipleSelectionFilter CreateMultipleSelectionFilter() => new MultipleSelectionFilter();

        protected partial class MultipleSelectionFilter : FillFlowContainer<MultipleSelectionFilterTabItem>
        {
            public readonly BindableList<T> Current = new BindableList<T>();

            [BackgroundDependencyLoader]
            private void load()
            {
                RelativeSizeAxes = Axes.X;
                AutoSizeAxes = Axes.Y;
                Spacing = new Vector2(10, 5);

                AddRange(GetValues().Select(CreateTabItem));
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                foreach (var item in Children)
                    item.Active.BindValueChanged(active => toggleItem(item.Value, active.NewValue));

                Current.BindCollectionChanged(currentChanged, true);
            }

            private void currentChanged(object? sender, NotifyCollectionChangedEventArgs e)
            {
                foreach (var c in Children)
                {
                    if (!c.Active.Disabled)
                        c.Active.Value = Current.Contains(c.Value);
                }
            }

            /// <summary>
            /// Returns all values to be displayed in this filter row.
            /// </summary>
            protected virtual IEnumerable<T> GetValues() => Enum.GetValues(typeof(T)).Cast<T>();

            /// <summary>
            /// Creates a <see cref="MultipleSelectionFilterTabItem"/> representing the supplied <paramref name="value"/>.
            /// </summary>
            protected virtual MultipleSelectionFilterTabItem CreateTabItem(T value) => new MultipleSelectionFilterTabItem(value);

            private void toggleItem(T value, bool active)
            {
                if (active)
                {
                    if (!Current.Contains(value))
                        Current.Add(value);
                }
                else
                    Current.Remove(value);
            }
        }

        protected partial class MultipleSelectionFilterTabItem : FilterTabItem<T>
        {
            private Container activeContent = null!;
            private Circle background = null!;

            public MultipleSelectionFilterTabItem(T value)
                : base(value)
            {
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                AutoSizeDuration = 100;
                AutoSizeEasing = Easing.OutQuint;

                // This doesn't match any actual design, but should make it easier for the user to understand
                // that filters are applied until we settle on a final design.
                AddInternal(activeContent = new Container
                {
                    Depth = float.MaxValue,
                    RelativeSizeAxes = Axes.Both,
                    Alpha = 0,
                    Padding = new MarginPadding
                    {
                        Left = -16,
                        Right = -4,
                        Vertical = -2
                    },
                    Children = new Drawable[]
                    {
                        background = new Circle
                        {
                            Colour = Color4.White,
                            RelativeSizeAxes = Axes.Both,
                        },
                        new SpriteIcon
                        {
                            Icon = FontAwesome.Solid.TimesCircle,
                            Size = new Vector2(10),
                            Colour = ColourProvider.Background4,
                            Position = new Vector2(3, 0.5f),
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                        }
                    }
                });
            }

            protected override Color4 ColourActive => ColourProvider.Light1;

            public override bool ReceivePositionalInputAt(Vector2 screenSpacePos)
            {
                return Active.Value
                    ? background.ReceivePositionalInputAt(screenSpacePos)
                    : base.ReceivePositionalInputAt(screenSpacePos);
            }

            protected override void UpdateState()
            {
                Color4 colour = Active.Value ? ColourActive : ColourNormal;

                if (!Enabled.Value)
                    colour = colour.Darken(1f);
                else if (IsHovered)
                    colour = Active.Value ? colour.Darken(0.2f) : colour.Lighten(0.2f);

                if (Active.Value)
                {
                    // This just allows enough spacing for adjacent tab items to show the "x".
                    Padding = new MarginPadding { Left = 12 };

                    activeContent.FadeIn(200, Easing.OutQuint);
                    background.FadeColour(colour, 200, Easing.OutQuint);

                    // flipping colours
                    Text.FadeColour(ColourProvider.Background4, 200, Easing.OutQuint);
                    Text.Font = Text.Font.With(weight: FontWeight.SemiBold);
                }
                else
                {
                    Padding = new MarginPadding();

                    activeContent.FadeOut();

                    background.FadeColour(colour, 200, Easing.OutQuint);
                    Text.FadeColour(colour, 200, Easing.OutQuint);
                    Text.Font = Text.Font.With(weight: FontWeight.Regular);
                }
            }

            protected override bool OnClick(ClickEvent e)
            {
                base.OnClick(e);

                // this tab item implementation is not managed by a TabControl,
                // therefore we have to manually update Active state and play select sound when this tab item is clicked.
                Active.Toggle();
                SelectSample.Play();
                return true;
            }
        }
    }
}
