// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Replays;
using osuTK;

namespace osu.Game.Replays.Legacy
{
    public class LegacyReplayFrame : ReplayFrame
    {
        public Vector2 Position => new Vector2(MouseX ?? 0, MouseY ?? 0);

        public float? MouseX;
        public float? MouseY;

        public bool MouseLeft => MouseLeft1 || MouseLeft2;
        public bool MouseRight => MouseRight1 || MouseRight2;

        public bool MouseLeft1 => ButtonState.HasFlag(ReplayButtonState.Left1);
        public bool MouseRight1 => ButtonState.HasFlag(ReplayButtonState.Right1);
        public bool MouseLeft2 => ButtonState.HasFlag(ReplayButtonState.Left2);
        public bool MouseRight2 => ButtonState.HasFlag(ReplayButtonState.Right2);

        public ReplayButtonState ButtonState;

        public LegacyReplayFrame(double time, float? mouseX, float? mouseY, ReplayButtonState buttonState)
            : base(time)
        {
            MouseX = mouseX;
            MouseY = mouseY;
            ButtonState = buttonState;
        }

        public override string ToString()
        {
            return $"{Time}\t({MouseX},{MouseY})\t{MouseLeft}\t{MouseRight}\t{MouseLeft1}\t{MouseRight1}\t{MouseLeft2}\t{MouseRight2}\t{ButtonState}";
        }
    }
}
