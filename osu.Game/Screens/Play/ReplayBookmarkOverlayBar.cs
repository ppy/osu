// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Pooling;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Localisation;
using osu.Game.Extensions;
using osu.Game.Graphics;

namespace osu.Game.Screens.Play
{
    /// <summary>
    /// Draws bookmark markers positioned over a horizontal replay progress bar.
    /// </summary>
    public partial class ReplayBookmarkOverlayBar : Container
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

        private DrawablePool<ReplayBookmarkMarker> pool = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            AddInternal(pool = new DrawablePool<ReplayBookmarkMarker>(10));
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
                Add(pool.Get(v =>
                {
                    v.X = (float)((bookmark - startTime) / length);
                    v.BookmarkTime = bookmark;
                }));
            }
        }

        private partial class ReplayBookmarkMarker : PoolableDrawable, IHasTooltip
        {
            public int BookmarkTime { get; set; }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                RelativePositionAxes = Axes.X;
                RelativeSizeAxes = Axes.Y;

                Anchor = Anchor.BottomLeft;
                Origin = Anchor.BottomCentre;

                Width = 4;
                Height = 1.0f;

                InternalChild = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = colours.Red.Opacity(0.6f),
                };
            }

            public LocalisableString TooltipText => $"{((double)BookmarkTime).ToEditorFormattedString()} bookmark";
        }
    }
}
