// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using OpenTK;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;

namespace osu.Game.Screens.Edit.Screens.Compose.RadioButtons
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
