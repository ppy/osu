// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Graphics.Containers.Draggable
{
    /// <summary>
    /// A CompositeDrawable that can be dragged between <see cref="OsuDraggableItemContainer{TModel}"/>s.
    /// </summary>
    public partial class OsuDraggableItem<TModel> : DraggableItem<TModel>
    {
        protected OsuDraggableItem(TModel item) : base(item)
        {
        }
    }
}
