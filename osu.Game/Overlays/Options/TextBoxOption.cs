using System;
using osu.Framework.Configuration;
using osu.Framework.Graphics.UserInterface;

namespace osu.Game.Overlays.Options
{
    public class TextBoxOption : TextBox
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
                    base.Text = bindable.Value;
                    bindable.ValueChanged += bindableValueChanged;
                }
            }
        }
        
        protected override string internalText
        {
            get { return base.internalText; }
            set
            {
                base.internalText = value;
                if (bindable != null)
                    bindable.Value = value;
            }
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