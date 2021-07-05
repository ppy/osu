// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Graphics.Containers
{
    /// <summary>
    /// A FillFlowContainer that provides functionality to cycle selection between children
    /// The selection wraps around when overflowing past the first or last child.
    /// </summary>
    public class SelectionCycleFillFlowContainer<T> : FillFlowContainer<T> where T : Drawable, ISelectable
    {
        private int selectedIndex = -1;

        private void setSelected(int value)
        {
            if (selectedIndex == value)
                return;

            // Deselect the previously-selected button
            if (selectedIndex != -1)
                this[selectedIndex].Selected = false;

            selectedIndex = value;

            // Select the newly-selected button
            if (selectedIndex != -1)
                this[selectedIndex].Selected = true;
        }

        public void SelectNext()
        {
            if (selectedIndex == -1 || selectedIndex == Count - 1)
                setSelected(0);
            else
                setSelected(selectedIndex + 1);
        }

        public void SelectPrevious()
        {
            if (selectedIndex == -1 || selectedIndex == 0)
                setSelected(Count - 1);
            else
                setSelected(selectedIndex - 1);
        }

        public void Deselect() => setSelected(-1);
        public void Select(T item) => setSelected(IndexOf(item));

        public T Selected => (selectedIndex >= 0 && selectedIndex < Count) ? this[selectedIndex] : null;
    }
}
