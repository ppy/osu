using System.Collections.Generic;
using osu.Framework.Input;
using osu.Game.Rulesets.Replays;
using OpenTK;

namespace osu.Game.Rulesets.Osu.Replays
{
    public class OsuReplayInputHandler : FramedReplayInputHandler
    {
        public OsuReplayInputHandler(Replay replay)
            : base(replay)
        {
        }

        public override List<InputState> GetPendingStates()
        {
            List<OsuAction> actions = new List<OsuAction>();

            if (CurrentFrame?.MouseLeft ?? false) actions.Add(OsuAction.LeftButton);
            if (CurrentFrame?.MouseRight ?? false) actions.Add(OsuAction.RightButton);

            return new List<InputState>
            {
                new ReplayState<OsuAction>
                {
                    Mouse = new ReplayMouseState(ToScreenSpace(Position ?? Vector2.Zero)),
                    PressedActions = actions
                }
            };
        }
    }
}
