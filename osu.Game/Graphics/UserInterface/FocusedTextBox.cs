﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Graphics;
using OpenTK.Input;
using osu.Framework.Allocation;
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
                if (!focus && HasFocus)
                    inputManager.ChangeFocus(null);
            }
        }

        private InputManager inputManager;

        [BackgroundDependencyLoader]
        private void load(UserInputManager inputManager)
        {
            this.inputManager = inputManager;
        }

        protected override void OnFocus(InputState state)
        {
            base.OnFocus(state);
            BorderThickness = 0;
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

        public override bool RequestsFocus => HoldFocus;
    }
}
