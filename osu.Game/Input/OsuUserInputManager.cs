// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Bindables;
using osu.Framework.Input;
using osu.Framework.Input.StateChanges.Events;
using osuTK.Input;

namespace osu.Game.Input
{
    public class OsuUserInputManager : UserInputManager
    {
        /// <summary>
        /// Whether the last input applied to the game is sourced from mouse.
        /// </summary>
        public IBindable<bool> LastInputWasMouseSource => lastInputWasMouseSource;

        private readonly Bindable<bool> lastInputWasMouseSource = new Bindable<bool>();

        internal OsuUserInputManager()
        {
        }

        public override void HandleInputStateChange(InputStateChangeEvent inputStateChange)
        {
            switch (inputStateChange)
            {
                case ButtonStateChangeEvent<MouseButton>:
                case MousePositionChangeEvent:
                    lastInputWasMouseSource.Value = true;
                    break;

                default:
                    lastInputWasMouseSource.Value = false;
                    break;
            }

            base.HandleInputStateChange(inputStateChange);
        }

        protected override MouseButtonEventManager CreateButtonEventManagerFor(MouseButton button)
        {
            switch (button)
            {
                case MouseButton.Right:
                    return new RightMouseManager(button);
            }

            return base.CreateButtonEventManagerFor(button);
        }

        private class RightMouseManager : MouseButtonEventManager
        {
            public RightMouseManager(MouseButton button)
                : base(button)
            {
            }

            public override bool EnableDrag => true; // allow right-mouse dragging for absolute scroll in scroll containers.
            public override bool EnableClick => false;
            public override bool ChangeFocusOnClick => false;
        }
    }
}
