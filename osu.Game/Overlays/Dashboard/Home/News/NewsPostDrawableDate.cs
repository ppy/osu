// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Game.Graphics;
using osu.Framework.Graphics;

namespace osu.Game.Overlays.Dashboard.Home.News
{
    public abstract class NewsPostDrawableDate : CompositeDrawable, IHasCustomTooltip
    {
        public ITooltip GetCustomTooltip() => new DateTooltip();

        public object TooltipContent => date;

        private readonly DateTimeOffset date;

        protected NewsPostDrawableDate(DateTimeOffset date)
        {
            this.date = date;
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            AutoSizeAxes = Axes.Both;
            Anchor = Anchor.TopRight;
            Origin = Anchor.TopRight;
            InternalChild = CreateDate(date, colourProvider);
        }

        protected abstract Drawable CreateDate(DateTimeOffset date, OverlayColourProvider colourProvider);
    }
}
