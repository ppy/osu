﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Graphics;
using OpenTK.Input;
using osu.Framework.Input;
using System;

namespace osu.Game.Graphics.UserInterface
{
    /// <summary>
    /// A textbox which holds focus eagerly.
    /// </summary>
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
                if (!focus && HasFocus)
                    GetContainingInputManager().ChangeFocus(null);
            }
        }

        protected override void OnFocus(InputState state)
        {
            base.OnFocus(state);
            BorderThickness = 0;
        }

        protected override bool OnKeyDown(InputState state, KeyDownEventArgs args)
        {
            if (args.Key == Key.Escape)
            {
                if (Text.Length > 0)
                    Text = string.Empty;
                else
                    Exit?.Invoke();
                return true;
            }

            return base.OnKeyDown(state, args);
        }

        public override bool RequestsFocus => HoldFocus;
    }
}
