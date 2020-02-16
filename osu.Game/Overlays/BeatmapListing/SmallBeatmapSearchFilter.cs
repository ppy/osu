// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.UserInterface;

namespace osu.Game.Overlays.BeatmapListing
{
    public class SmallBeatmapSearchFilter<T> : BeatmapSearchFilter<T>
    {
        protected override TabItem<T> CreateTabItem(T value) => new SmallTabItem(value);

        protected class SmallTabItem : FilterTabItem
        {
            public SmallTabItem(T value)
                : base(value)
            {
            }

            protected override float TextSize() => 10;
        }
    }
}
