// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using System.Collections.Generic;
using System.Linq;

namespace osu.Game.Screens.Edit.Screens.Setup.Components
{
    public class SetupRadioButtonCollection : FillFlowContainer, IHasCurrentValue<SetupRadioButton>
    {
        private SetupRadioButton lastSelection;

        public SetupRadioButton SelectedRadioButton
        {
            get => Current.Value;
            set => Current.Value = value;
        }

        public Bindable<SetupRadioButton> Current { get; } = new Bindable<SetupRadioButton>();

        public IEnumerable<SetupRadioButton> Items
        {
            get => Children.Cast<SetupRadioButton>();
            set
            {
                Children = (IReadOnlyList<Drawable>)value;
                if (value.Any())
                    Current.Value = value.ElementAt(0);
                foreach (var r in value)
                    r.RadioButtonClicked += a => Current.Value = a;
            }
        }

        public SetupRadioButtonCollection()
        {
            Direction = FillDirection.Horizontal;
            Spacing = new Vector2(10);
            Height = SetupRadioButton.BUTTON_SIZE;

            Current.ValueChanged += a =>
            {
                if (lastSelection == a)
                    return;

                selectNewRadioButton(a);
            };
        }

        private void selectNewRadioButton(SetupRadioButton newSelection)
        {
            if (lastSelection != null)
                lastSelection.Current.Value = false;
            lastSelection = newSelection;
            newSelection.Current.Value = true;
        }
    }
}
