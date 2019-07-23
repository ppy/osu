// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input;
using osu.Framework.Input.Events;
using osuTK;
using osuTK.Input;

namespace osu.Game.Graphics.UserInterface
{
    public class SearchTextBox : FocusedTextBox
    {
        protected virtual bool AllowCommit => false;

        public override bool HandleLeftRightArrows => false;

        public SearchTextBox()
        {
            Height = 35;
            AddRange(new Drawable[]
            {
                new SpriteIcon
                {
                    Icon = FontAwesome.Solid.Search,
                    Origin = Anchor.CentreRight,
                    Anchor = Anchor.CentreRight,
                    Margin = new MarginPadding { Right = 10 },
                    Size = new Vector2(20),
                }
            });

            PlaceholderText = "type to search";
        }

        protected override bool HandleAction(PlatformAction action)
        {
            // Shift-delete is unnecessary for search inputs, so its propagated up the input queue.
            if (action.ActionType == PlatformActionType.CharNext && action.ActionMethod == PlatformActionMethod.Delete)
                return false;

            return base.HandleAction(action);
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
