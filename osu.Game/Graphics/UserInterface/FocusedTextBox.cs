using OpenTK.Graphics;
using OpenTK.Input;
using osu.Framework.Input;
using System;
using System.Linq;

namespace osu.Game.Graphics.UserInterface
{
    public class FocusedTextBox : OsuTextBox
    {
        protected override Color4 BackgroundUnfocused => new Color4(10, 10, 10, 255);
        protected override Color4 BackgroundFocused => new Color4(10, 10, 10, 255);

        public Action Exit;

        private bool focus;
        public bool HoldFocus
        {
            get { return focus; }
            set
            {
                focus = value;
                if (!focus)
                    TriggerFocusLost();
            }
        }

        protected override bool OnFocus(InputState state)
        {
            var result = base.OnFocus(state);
            BorderThickness = 0;
            return result;
        }

        protected override void OnFocusLost(InputState state)
        {
            if (state.Keyboard.Keys.Any(key => key == Key.Escape))
            {
                if (Text.Length > 0)
                    Text = string.Empty;
                else
                    Exit?.Invoke();
            }
            base.OnFocusLost(state);
        }

        public override bool RequestingFocus => HoldFocus;
    }
}
