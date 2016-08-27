//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using OpenTK;
using OpenTK.Input;
using osu.Framework.Lists;
using System.Collections.Generic;
using System.Linq;

namespace osu.Framework.Input
{
    public class MouseState
    {
        public MouseState LastState;

        public ReadOnlyList<ButtonState> ButtonStates = new ReadOnlyList<ButtonState>(new []
        {
            new ButtonState(MouseButton.Left),
            new ButtonState(MouseButton.Middle),
            new ButtonState(MouseButton.Right),
            new ButtonState(MouseButton.Button1),
            new ButtonState(MouseButton.Button2)
        });

        public bool LeftButton => ButtonStates.Find(b => b.Button == MouseButton.Left).State;
        public bool RightButton => ButtonStates.Find(b => b.Button == MouseButton.Right).State;
        public bool MiddleButton => ButtonStates.Find(b => b.Button == MouseButton.Middle).State;
        public bool BackButton => ButtonStates.Find(b => b.Button == MouseButton.Button1).State;
        public bool ForwardButton => ButtonStates.Find(b => b.Button == MouseButton.Button2).State;

        public bool WheelUp;
        public bool WheelDown;

        public bool HasMainButtonPressed => LeftButton || RightButton;

        public Vector2 PositionDelta => Position - (LastState?.Position ?? Vector2.Zero);

        public Vector2 Position;

        public Vector2? PositionMouseDown;

        public MouseState(MouseState last = null)
        {
            LastState = last;
            PositionMouseDown = last?.PositionMouseDown;
        }

        public class ButtonState
        {
            public MouseButton Button;
            public bool State;

            public ButtonState(MouseButton button)
            {
                Button = button;
                State = false;
            }
        }
    }
}
