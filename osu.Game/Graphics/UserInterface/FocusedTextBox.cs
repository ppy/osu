// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osuTK.Graphics;
using System;
using osu.Framework.Allocation;
using osu.Framework.Input.Events;
using osu.Framework.Platform;
using osu.Game.Input.Bindings;
using osuTK.Input;

namespace osu.Game.Graphics.UserInterface
{
    /// <summary>
    /// A textbox which holds focus eagerly.
    /// </summary>
    public class FocusedTextBox : OsuTextBox
    {
        public Action Exit;

        private bool focus;

        private bool allowImmediateFocus => host?.OnScreenKeyboardOverlapsGameWindow != true;

        public void TakeFocus()
        {
            if (allowImmediateFocus) GetContainingInputManager().ChangeFocus(this);
        }

        public bool HoldFocus
        {
            get => allowImmediateFocus && focus;
            set
            {
                focus = value;
                if (!focus && HasFocus)
                    base.KillFocus();
            }
        }

        private GameHost host;

        [BackgroundDependencyLoader]
        private void load(GameHost host)
        {
            this.host = host;

            BackgroundUnfocused = new Color4(10, 10, 10, 255);
            BackgroundFocused = new Color4(10, 10, 10, 255);
        }

        // We may not be focused yet, but we need to handle keyboard input to be able to request focus
        public override bool HandleNonPositionalInput => HoldFocus || base.HandleNonPositionalInput;

        protected override void OnFocus(FocusEvent e)
        {
            base.OnFocus(e);
            BorderThickness = 0;
        }

        protected override bool OnKeyDown(KeyDownEvent e)
        {
            if (!HasFocus) return false;

            if (e.Key == Key.Escape)
                return false; // disable the framework-level handling of escape key for confority (we use GlobalAction.Back).

            return base.OnKeyDown(e);
        }

        public override bool OnPressed(GlobalAction action)
        {
            if (action == GlobalAction.Back)
            {
                if (Text.Length > 0)
                {
                    Text = string.Empty;
                    return true;
                }
            }

            return base.OnPressed(action);
        }

        protected override void KillFocus()
        {
            base.KillFocus();
            Exit?.Invoke();
        }

        public override bool RequestsFocus => HoldFocus;
    }
}
