// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Input;
using osu.Framework.MathUtils;
using osu.Game.Rulesets.Replays;
using OpenTK;

namespace osu.Game.Rulesets.Osu.Replays
{
    public class OsuReplayInputHandler : FramedReplayInputHandler<OsuReplayFrame>
    {
        public OsuReplayInputHandler(Replay replay)
            : base(replay)
        {
        }

        protected override bool IsImportant(OsuReplayFrame frame) => frame.Actions.Any();

        protected Vector2? Position
        {
            get
            {
                if (!HasFrames)
                    return null;

                return Interpolation.ValueAt(CurrentTime, CurrentFrame.Position, NextFrame.Position, CurrentFrame.Time, NextFrame.Time);
            }
        }

        public override List<InputState> GetPendingStates()
        {
            return new List<InputState>
            {
                new ReplayState<OsuAction>
                {
                    Mouse = new ReplayMouseState(GamefieldToScreenSpace(Position ?? Vector2.Zero)),
                    PressedActions = CurrentFrame.Actions
                }
            };
        }
    }
}
