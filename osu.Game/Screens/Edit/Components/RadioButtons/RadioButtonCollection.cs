// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osuTK;

namespace osu.Game.Screens.Edit.Components.RadioButtons
{
    public class RadioButtonCollection : CompositeDrawable
    {
        private IReadOnlyList<RadioButton> items;
        public IReadOnlyList<RadioButton> Items
        {
            get { return items; }
            set
            {
                if (ReferenceEquals(items, value))
                    return;
                items = value;

                buttonContainer.Clear();
                items.ForEach(addButton);
            }
        }

        private readonly FlowContainer<DrawableRadioButton> buttonContainer;

        public RadioButtonCollection()
        {
            AutoSizeAxes = Axes.Y;

            InternalChild = buttonContainer = new FillFlowContainer<DrawableRadioButton>
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(0, 5)
            };
        }

        private RadioButton currentlySelected;
        private void addButton(RadioButton button)
        {
            button.Selected.ValueChanged += v =>
            {
                if (v)
                {
                    currentlySelected?.Deselect();
                    currentlySelected = button;
                }
                else
                    currentlySelected = null;
            };

            buttonContainer.Add(new DrawableRadioButton(button));
        }
    }
}
