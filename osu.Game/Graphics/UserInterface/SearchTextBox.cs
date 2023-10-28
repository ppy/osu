// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Input;
using osu.Framework.Input.Events;
using osu.Game.Resources.Localisation.Web;
using osuTK.Input;

namespace osu.Game.Graphics.UserInterface
{
    public partial class SearchTextBox : FocusedTextBox
    {
        protected virtual bool AllowCommit => false;

        public SearchTextBox()
        {
            Height = 35;
            PlaceholderText = HomeStrings.SearchPlaceholder;
        }

        protected override void OnFocus(FocusEvent e)
        {
            base.OnFocus(e);
            SelectAll();
        }

        public override bool OnPressed(KeyBindingPressEvent<PlatformAction> e)
        {
            switch (e.Action)
            {
                case PlatformAction.MoveBackwardLine:
                case PlatformAction.MoveForwardLine:
                // Shift+delete is handled via PlatformAction on macOS. this is not so useful in the context of a SearchTextBox
                // as we do not allow arrow key navigation in the first place (ie. the caret should always be at the end of text)
                // Avoid handling it here to allow other components to potentially consume the shortcut.
                case PlatformAction.DeleteForwardChar:
                    return false;
            }

            return base.OnPressed(e);
        }

        protected override bool OnKeyDown(KeyDownEvent e)
        {
            if (!e.ControlPressed && !e.ShiftPressed)
            {
                switch (e.Key)
                {
                    case Key.Left:
                    case Key.Right:
                    case Key.Up:
                    case Key.Down:
                        return false;
                }
            }

            if (!AllowCommit)
            {
                switch (e.Key)
                {
                    case Key.KeypadEnter:
                    case Key.Enter:
                        return false;
                }
            }

            if (e.ShiftPressed)
            {
                switch (e.Key)
                {
                    case Key.Delete:
                        return false;
                }
            }

            return base.OnKeyDown(e);
        }
    }
}
