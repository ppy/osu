// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Screens.Edit.List
{
    public interface IDrawableListRepresetedItem<out T>
    {
        T? RepresentedItem { get; }
    }

    public class DrawableListRepresetedItem<T> : IDrawableListRepresetedItem<T>
    {
        public T RepresentedItem { get; }
        public DrawableListEntryType Type { get; }

        public DrawableListRepresetedItem(T item, DrawableListEntryType type)
        {
            RepresentedItem = item;
            Type = type;
        }
    }

    public enum DrawableListEntryType
    {
        Item,
        MinimisableList,
    }
}
