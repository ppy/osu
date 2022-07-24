// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using MessagePack;
using Newtonsoft.Json;
using osu.Framework.Extensions.EnumExtensions;
using osu.Game.Rulesets.Replays;
using osuTK;

namespace osu.Game.Replays.Legacy
{
    [MessagePackObject]
    public class LegacyReplayFrame : ReplayFrame
    {
        [JsonIgnore]
        [IgnoreMember]
        public Vector2 Position => new Vector2(MouseX ?? 0, MouseY ?? 0);

        [Key(1)]
        public float? MouseX;

        [Key(2)]
        public float? MouseY;

        [JsonIgnore]
        [IgnoreMember]
        public bool MouseLeft => MouseLeft1 || MouseLeft2;

        [JsonIgnore]
        [IgnoreMember]
        public bool MouseRight => MouseRight1 || MouseRight2;

        [JsonIgnore]
        [IgnoreMember]
        public bool MouseLeft1 => ButtonState.HasFlagFast(ReplayButtonState.Left1);

        [JsonIgnore]
        [IgnoreMember]
        public bool MouseRight1 => ButtonState.HasFlagFast(ReplayButtonState.Right1);

        [JsonIgnore]
        [IgnoreMember]
        public bool MouseLeft2 => ButtonState.HasFlagFast(ReplayButtonState.Left2);

        [JsonIgnore]
        [IgnoreMember]
        public bool MouseRight2 => ButtonState.HasFlagFast(ReplayButtonState.Right2);

        [Key(3)]
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
