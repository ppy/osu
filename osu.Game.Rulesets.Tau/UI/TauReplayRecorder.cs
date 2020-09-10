using System.Collections.Generic;
using osu.Game.Replays;
using osu.Game.Rulesets.Replays;
using osu.Game.Rulesets.Tau.Replays;
using osu.Game.Rulesets.UI;
using osuTK;

namespace osu.Game.Rulesets.Tau.UI
{
    public class TauReplayRecorder : ReplayRecorder<TauAction>
    {
        public TauReplayRecorder(Replay replay)
            : base(replay)
        {
        }

        protected override ReplayFrame HandleFrame(Vector2 mousePosition, List<TauAction> actions, ReplayFrame previousFrame)
            => new TauReplayFrame(Time.Current, mousePosition, actions.ToArray());
    }
}
