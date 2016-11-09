using System;
using osu.Framework.Configuration;
using osu.Framework.Graphics.UserInterface;

namespace osu.Game.Overlays.Options
{
    public class CheckBoxOption : BasicCheckBox
    {
        private Bindable<bool> bindable;

        public Bindable<bool> Bindable
        {
            set
            {
                if (bindable != null)
                    bindable.ValueChanged -= bindableValueChanged;
                bindable = value;
                if (bindable != null)
                {
                    bool state = State == CheckBoxState.Checked;
                    if (state != bindable.Value)
                        State = bindable.Value ? CheckBoxState.Checked : CheckBoxState.Unchecked;
                    bindable.ValueChanged += bindableValueChanged;
                }
            }
        }

        private void bindableValueChanged(object sender, EventArgs e)
        {
            State = bindable.Value ? CheckBoxState.Checked : CheckBoxState.Unchecked;
        }

        protected override void Dispose(bool isDisposing)
        {
            if (bindable != null)
                bindable.ValueChanged -= bindableValueChanged;
            base.Dispose(isDisposing);
        }

        protected override void OnChecked()
        {
            if (bindable != null)
                bindable.Value = true;
            base.OnChecked();
        }

        protected override void OnUnchecked()
        {
            if (bindable != null)
                bindable.Value = false;
            base.OnUnchecked();
        }
    }
}
