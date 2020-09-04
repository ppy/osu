// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.Containers;
using osuTK;

namespace osu.Game.Collections
{
    public class CollectionList : OsuRearrangeableListContainer<BeatmapCollection>
    {
        protected override ScrollContainer<Drawable> CreateScrollContainer() => base.CreateScrollContainer().With(d =>
        {
            d.ScrollbarVisible = false;
        });

        protected override FillFlowContainer<RearrangeableListItem<BeatmapCollection>> CreateListFillFlowContainer() => new FillFlowContainer<RearrangeableListItem<BeatmapCollection>>
        {
            LayoutDuration = 200,
            LayoutEasing = Easing.OutQuint,
            Spacing = new Vector2(0, 2)
        };

        protected override OsuRearrangeableListItem<BeatmapCollection> CreateOsuDrawable(BeatmapCollection item) => new CollectionListItem(item);
    }
}
