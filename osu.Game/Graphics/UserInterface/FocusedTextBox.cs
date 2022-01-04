// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

using osuTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Input.Events;
using osu.Framework.Platform;
using osu.Game.Input.Bindings;
using osuTK.Input;
using osu.Framework.Input.Bindings;
using osu.Game.Overlays;

namespace osu.Game.Graphics.UserInterface
{
    /// <summary>
    /// A textbox which holds focus eagerly.
    /// </summary>
    public class FocusedTextBox : OsuTextBox, IKeyBindingHandler<GlobalAction>
    {
        private bool focus;

        private bool allowImmediateFocus => host?.OnScreenKeyboardOverlapsGameWindow != true;

        public void TakeFocus()
        {
            if (!allowImmediateFocus)
                return;

            Scheduler.Add(() => GetContainingInputManager().ChangeFocus(this), false);
        }

        public new void KillFocus() => base.KillFocus();

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

        [Resolved]
        private GameHost? host { get; set; }

        [BackgroundDependencyLoader(true)]
        private void load(OverlayColourProvider? colourProvider)
        {
            BackgroundUnfocused = colourProvider?.Background5 ?? new Color4(10, 10, 10, 255);
            BackgroundFocused = colourProvider?.Background5 ?? new Color4(10, 10, 10, 255);
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
                return false; // disable the framework-level handling of escape key for conformity (we use GlobalAction.Back).

            return base.OnKeyDown(e);
        }

        public virtual bool OnPressed(KeyBindingPressEvent<GlobalAction> e)
        {
            if (e.Repeat)
                return false;

            if (!HasFocus) return false;

            if (e.Action == GlobalAction.Back)
            {
                if (Text.Length > 0)
                {
                    Text = string.Empty;
                    return true;
                }
            }

            return false;
        }

        public void OnReleased(KeyBindingReleaseEvent<GlobalAction> e)
        {
        }

        public override bool RequestsFocus => HoldFocus;
    }
}
