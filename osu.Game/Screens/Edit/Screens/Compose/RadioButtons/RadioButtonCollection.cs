// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using System.Linq;
using OpenTK;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;

namespace osu.Game.Screens.Edit.Screens.Compose.RadioButtons
{
    public class RadioButtonCollection : CompositeDrawable
    {
        public IReadOnlyList<RadioButton> Items
        {
            set
            {
                buttonContainer.Clear();
                value.ForEach(addButton);
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

        private void addButton(RadioButton button) => buttonContainer.Add(new DrawableRadioButton(button) { Selected = buttonSelected });

        private DrawableRadioButton currentlySelected;
        private void buttonSelected(DrawableRadioButton drawableButton)
        {
            currentlySelected?.Deselect();
            currentlySelected = drawableButton;
        }
    }
}
