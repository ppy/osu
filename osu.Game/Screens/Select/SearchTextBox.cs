// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Linq;
using OpenTK.Graphics;
using OpenTK.Input;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Input;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Screens.Select
{
    /// <summary>
    /// A textbox which holds focus eagerly.
    /// </summary>
    public class SearchTextBox : FocusedTextBox
    {
        public SearchTextBox()
        {
            Height = 35;
            Add(new Drawable[]
            {
                new TextAwesome
                {
                    Icon = FontAwesome.fa_search,
                    Origin = Anchor.CentreRight,
                    Anchor = Anchor.CentreRight,
                    Margin = new MarginPadding { Right = 10 },
                }
            });

            PlaceholderText = "type to search";
        }

        protected override bool OnKeyDown(InputState state, KeyDownEventArgs args)
        {
            if (HandlePendingText(state)) return true;

            if (!state.Keyboard.ControlPressed && !state.Keyboard.ShiftPressed)
            {
                switch (args.Key)
                {
                    case Key.Left:
                    case Key.Right:
                    case Key.Up:
                    case Key.Down:
                    case Key.Enter:
                        return false;
                }
            }

            return base.OnKeyDown(state, args);
        }
    }
}