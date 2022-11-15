// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Containers;

namespace osu.Game.Screens.Edit.List
{
    public interface IRearrangableDrawableListItem<T> : IDrawableListItem<T>
    {
        public RearrangeableListItem<T> GetRearrangeableListItem();
    }
}
