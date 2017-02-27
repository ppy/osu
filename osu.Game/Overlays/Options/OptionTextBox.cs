// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Configuration;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Overlays.Options
{
    public class OptionTextBox : OsuTextBox
    {
        private Bindable<string> bindable;

        public Bindable<string> Bindable
        {
            set
            {
                if (bindable != null)
                    bindable.ValueChanged -= bindableValueChanged;
                bindable = value;
                if (bindable != null)
                {
                    Text = bindable.Value;
                    bindable.ValueChanged += bindableValueChanged;
                }

                if (bindable?.Disabled ?? true)
                    Alpha = 0.3f;
            }
        }

        public OptionTextBox()
        {
            OnChange += onChange;
        }

        private void onChange(TextBox sender, bool newText)
        {
            if (bindable != null)
                bindable.Value = Text;
        }

        private void bindableValueChanged(object sender, EventArgs e)
        {
            Text = bindable.Value;
        }

        protected override void Dispose(bool isDisposing)
        {
            if (bindable != null)
                bindable.ValueChanged -= bindableValueChanged;
            base.Dispose(isDisposing);
        }
    }
}