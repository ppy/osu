// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.UserInterface;

namespace osu.Game.Overlays.BeatmapListing
{
    public class BeatmapSearchSmallFilterRow<T> : BeatmapSearchFilterRow<T>
    {
        public BeatmapSearchSmallFilterRow(string headerName)
            : base(headerName)
        {
        }

        protected override BeatmapSearchFilter CreateFilter() => new SmallBeatmapSearchFilter();

        private class SmallBeatmapSearchFilter : BeatmapSearchFilter
        {
            protected override TabItem<T> CreateTabItem(T value) => new SmallTabItem(value);

            private class SmallTabItem : FilterTabItem
            {
                public SmallTabItem(T value)
                    : base(value)
                {
                }

                protected override float TextSize => 10;
            }
        }
    }
}
