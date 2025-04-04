// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Graphics.Containers.Draggable
{
    /// <summary>
    /// Holds the <see cref="OsuDraggableItemContainer{TModel}"/>s that can have their <see cref="OsuDraggableItem{TModel}"/>s shared between them.
    /// </summary>
    public partial class OsuDraggableSharingContainer<TModel> : DraggableSharingContainer<TModel>
        where TModel : notnull
    {
    }
}
