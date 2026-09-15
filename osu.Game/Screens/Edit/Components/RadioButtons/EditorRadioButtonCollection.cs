// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osuTK;

namespace osu.Game.Screens.Edit.Components.RadioButtons
{
    public partial class EditorRadioButtonCollection : CompositeDrawable
    {
        public IEnumerable<EditorRadioButton> Items => itemsContainer;
        private readonly FlowContainer<EditorRadioButton> itemsContainer;

        public EditorRadioButtonCollection()
        {
            AutoSizeAxes = Axes.Y;

            InternalChild = itemsContainer = new FillFlowContainer<EditorRadioButton>
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(0, 5)
            };
        }

        private EditorRadioButton? currentlySelected;

        public void AddButton(EditorRadioButton button)
        {
            button.Selected.ValueChanged += selected =>
            {
                if (selected.NewValue)
                {
                    currentlySelected?.Deselect();
                    currentlySelected = button;
                }
                else
                    currentlySelected = null;
            };

            itemsContainer.Add(button);
        }
    }
}
