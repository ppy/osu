// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Linq;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Overlays.Browse
{
    public abstract class BrowseHeader<T> : Container
    {
        public static readonly float HEIGHT = 90;

        private readonly Box tabStrip;

        public readonly HeaderTabControl<T> Tabs;

        protected abstract Color4 BackgroundColour { get; }
        protected abstract float TabStripWidth { get; } //can be removed once (if?) TabControl support auto sizing
        protected abstract T DefaultTab { get; }
        protected abstract Drawable CreateHeaderText();

        public BrowseHeader()
        {
            if (!typeof(T).IsEnum)
                throw new InvalidOperationException("BrowseHeader only supports enums as the generic type argument");

            RelativeSizeAxes = Axes.X;
            Height = HEIGHT;

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
                    Padding = new MarginPadding { Left = BrowseOverlay.WIDTH_PADDING, Right = BrowseOverlay.WIDTH_PADDING }, 
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
                            Children = new Drawable[]
                            {
                                new TextAwesome
                                {
                                    TextSize = 25,
                                    Icon = FontAwesome.fa_osu_chevron_down_o,
                                },
                                CreateHeaderText(),
                            },
                        },
                        tabStrip = new Box
                        {
                            Anchor = Anchor.BottomLeft,
                            Origin = Anchor.BottomLeft,
                            Width = TabStripWidth,
                            Height = 1,
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
            tabStrip.Colour = colours.Green;
        }
    }
}
