// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;

namespace osu.Game.Rulesets.Replays
{
    public class ReplayFrame
    {
        public Vector2 Position => new Vector2(MouseX ?? 0, MouseY ?? 0);

        public bool IsImportant => MouseX.HasValue && MouseY.HasValue && (MouseLeft || MouseRight);

        public float? MouseX;
        public float? MouseY;

        public bool MouseLeft => MouseLeft1 || MouseLeft2;
        public bool MouseRight => MouseRight1 || MouseRight2;

        public bool MouseLeft1
        {
            get { return (ButtonState & ReplayButtonState.Left1) > 0; }
            set { setButtonState(ReplayButtonState.Left1, value); }
        }
        public bool MouseRight1
        {
            get { return (ButtonState & ReplayButtonState.Right1) > 0; }
            set { setButtonState(ReplayButtonState.Right1, value); }
        }
        public bool MouseLeft2
        {
            get { return (ButtonState & ReplayButtonState.Left2) > 0; }
            set { setButtonState(ReplayButtonState.Left2, value); }
        }
        public bool MouseRight2
        {
            get { return (ButtonState & ReplayButtonState.Right2) > 0; }
            set { setButtonState(ReplayButtonState.Right2, value); }
        }

        private void setButtonState(ReplayButtonState singleButton, bool pressed)
        {
            if (pressed)
                ButtonState |= singleButton;
            else
                ButtonState &= ~singleButton;
        }

        public double Time;

        public ReplayButtonState ButtonState;

        protected ReplayFrame()
        {

        }

        public ReplayFrame(double time, float? mouseX, float? mouseY, ReplayButtonState buttonState)
        {
            MouseX = mouseX;
            MouseY = mouseY;
            ButtonState = buttonState;
            Time = time;
        }

        public override string ToString()
        {
            return $"{Time}\t({MouseX},{MouseY})\t{MouseLeft}\t{MouseRight}\t{MouseLeft1}\t{MouseRight1}\t{MouseLeft2}\t{MouseRight2}\t{ButtonState}";
        }
    }
}