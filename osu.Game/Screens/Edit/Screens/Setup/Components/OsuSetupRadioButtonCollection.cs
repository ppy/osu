// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace osu.Game.Screens.Edit.Screens.Setup.Components
{
    public class OsuSetupRadioButtonCollection : FillFlowContainer
    {
        private OsuSetupRadioButton selectedRadioButton;
        public OsuSetupRadioButton SelectedRadioButton
        {
            get => selectedRadioButton;
            set
            {
                if (selectedRadioButton == value)
                    return;

                // Uncomment if necessary
                //if (!Buttons.Contains(value))
                //    throw new InvalidOperationException("Cannot select a radio button that does not belong in the collection.");

                selectNewRadioButton(value);
                TriggerSelectedRadioButtonChanged(value);
            }
        }

        public event Action<OsuSetupRadioButton> SelectedRadioButtonChanged;

        public void TriggerSelectedRadioButtonChanged(OsuSetupRadioButton newSelection)
        {
            SelectedRadioButtonChanged?.Invoke(newSelection);
        }

        public IEnumerable<OsuSetupRadioButton> Items
        {
            get => Children.Cast<OsuSetupRadioButton>();
            set
            {
                Children = (IReadOnlyList<Drawable>)value;
                if (value.Any())
                    SelectedRadioButton = value.ElementAt(0);
                foreach (var r in value)
                    r.RadioButtonClicked += a => SelectedRadioButton = a;
            }
        }

        public OsuSetupRadioButtonCollection()
        {
            Direction = FillDirection.Horizontal;
            Spacing = new Vector2(10);
            Height = OsuSetupRadioButton.BUTTON_SIZE;
        }

        private void selectNewRadioButton(OsuSetupRadioButton newSelection)
        {
            if (selectedRadioButton != null)
                selectedRadioButton.Current.Value = false;
            selectedRadioButton = newSelection;
            newSelection.Current.Value = true;
        }
    }
}
