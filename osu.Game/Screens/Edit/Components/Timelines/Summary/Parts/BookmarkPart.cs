// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Pooling;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Localisation;
using osu.Game.Extensions;
using osu.Game.Graphics;
using osu.Game.Screens.Edit.Components.Timelines.Summary.Visualisations;

namespace osu.Game.Screens.Edit.Components.Timelines.Summary.Parts
{
    /// <summary>
    /// The part of the timeline that displays bookmarks.
    /// </summary>
    public partial class BookmarkPart : TimelinePart
    {
        private readonly BindableList<int> bookmarks = new BindableList<int>();

        private DrawablePool<BookmarkVisualisation> pool = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            AddInternal(pool = new DrawablePool<BookmarkVisualisation>(10));
        }

        protected override void LoadBeatmap(EditorBeatmap beatmap)
        {
            base.LoadBeatmap(beatmap);

            bookmarks.UnbindAll();
            bookmarks.BindTo(beatmap.Bookmarks);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            bookmarks.BindCollectionChanged((_, _) =>
            {
                Clear(disposeChildren: false);
                foreach (int bookmark in bookmarks)
                    Add(pool.Get(v => v.StartTime = bookmark));
            }, true);
        }

        private partial class BookmarkVisualisation : PoolableDrawable, IHasTooltip
        {
            private int startTime;

            public int StartTime
            {
                get => startTime;
                set
                {
                    if (startTime == value)
                        return;

                    startTime = value;
                    X = startTime;
                }
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                RelativePositionAxes = Axes.Both;
                RelativeSizeAxes = Axes.Y;

                Anchor = Anchor.CentreLeft;
                Origin = Anchor.Centre;

                Width = PointVisualisation.MAX_WIDTH;
                Height = 0.4f;

                Colour = colours.Blue;
                InternalChild = new FastCircle { RelativeSizeAxes = Axes.Both };
            }

            public LocalisableString TooltipText => $"{((double)StartTime).ToEditorFormattedString()} bookmark";
        }
    }
}
