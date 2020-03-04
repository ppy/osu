// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;

namespace osu.Game.Graphics.Containers
{
    public abstract class OsuRearrangeableListContainer<TModel> : RearrangeableListContainer<TModel>
    {
        /// <summary>
        /// Whether any item is currently being dragged. Used to hide other items' drag handles.
        /// </summary>
        private readonly BindableBool playlistDragActive = new BindableBool();

        protected override ScrollContainer<Drawable> CreateScrollContainer() => new OsuScrollContainer();

        protected sealed override RearrangeableListItem<TModel> CreateDrawable(TModel item) => CreateOsuDrawable(item).With(d =>
        {
            d.PlaylistDragActive.BindTo(playlistDragActive);
        });

        protected abstract OsuRearrangeableListItem<TModel> CreateOsuDrawable(TModel item);
    }
}
