// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osuTK;
using osuTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;

namespace osu.Game.Overlays.SearchableList
{
    public abstract class SearchableListHeader<T> : Container
    {
        public readonly HeaderTabControl<T> Tabs;

        protected abstract Color4 BackgroundColour { get; }
        protected abstract T DefaultTab { get; }
        protected abstract Drawable CreateHeaderText();
        protected abstract IconUsage Icon { get; }

        protected SearchableListHeader()
        {
            if (!typeof(T).IsEnum)
                throw new InvalidOperationException("BrowseHeader only supports enums as the generic type argument");

            RelativeSizeAxes = Axes.X;
            Height = 90;

            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = BackgroundColour,
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding { Left = SearchableListOverlay.WIDTH_PADDING, Right = SearchableListOverlay.WIDTH_PADDING },
                    Children = new Drawable[]
                    {
                        new FillFlowContainer
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.BottomLeft,
                            Position = new Vector2(-35f, 5f),
                            AutoSizeAxes = Axes.Both,
                            Direction = FillDirection.Horizontal,
                            Spacing = new Vector2(10f, 0f),
                            Children = new[]
                            {
                                new SpriteIcon
                                {
                                    Size = new Vector2(25),
                                    Icon = Icon,
                                },
                                CreateHeaderText(),
                            },
                        },
                        Tabs = new HeaderTabControl<T>
                        {
                            Anchor = Anchor.BottomLeft,
                            Origin = Anchor.BottomLeft,
                            RelativeSizeAxes = Axes.X,
                        },
                    },
                },
            };

            Tabs.Current.Value = DefaultTab;
            Tabs.Current.TriggerChange();
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Tabs.StripColour = colours.Green;
        }
    }
}
