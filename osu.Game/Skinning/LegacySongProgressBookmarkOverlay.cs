// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Pooling;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Localisation;
using osu.Game.Extensions;
using osu.Game.Graphics;
using osu.Game.Screens.Play;

namespace osu.Game.Skinning
{
    /// <summary>
    /// Draws bookmark markers as radial tick marks on the legacy circular progress ring.
    /// </summary>
    public partial class LegacySongProgressBookmarkOverlay : Container
    {
        private double startTime;

        public double StartTime
        {
            get => startTime;
            set
            {
                startTime = value;
                if (IsLoaded) redrawMarkers();
            }
        }

        private double endTime = 1;

        public double EndTime
        {
            get => endTime;
            set
            {
                endTime = value;
                if (IsLoaded) redrawMarkers();
            }
        }

        [Resolved]
        private ReplayBookmarkController bookmarkController { get; set; } = null!;

        private DrawablePool<BookmarkTick> pool = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            AddInternal(pool = new DrawablePool<BookmarkTick>(10));
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            bookmarkController.Bookmarks.BindCollectionChanged((_, _) => redrawMarkers(), true);
        }

        private void redrawMarkers()
        {
            Clear(disposeChildren: false);

            double length = endTime - startTime;
            if (length <= 0) return;

            foreach (int bookmark in bookmarkController.Bookmarks)
            {
                float fraction = (float)((bookmark - startTime) / length);

                // rotate a full-size container so the tick at 12 o'clock lands at the bookmark angle.
                // rotation clockwise, matching CircularProgress which fills from the top.
                Add(pool.Get(v =>
                {
                    v.Rotation = fraction * 360f;
                    v.BookmarkTime = bookmark;
                }));
            }
        }

        private partial class BookmarkTick : PoolableDrawable, IHasTooltip
        {
            public int BookmarkTime { get; set; }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                RelativeSizeAxes = Axes.Both;
                Anchor = Anchor.Centre;
                Origin = Anchor.Centre;

                InternalChild = new Box
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.Centre,
                    Colour = colours.Red,
                    Width = 3,
                    Height = 3,
                };
            }

            public LocalisableString TooltipText => $"{((double)BookmarkTime).ToEditorFormattedString()} bookmark";
        }
    }
}
