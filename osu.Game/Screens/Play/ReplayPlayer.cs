// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Rulesets.Replays;

namespace osu.Game.Screens.Play
{
    public class ReplayPlayer : Player
    {
        public Replay Replay;

        public ReplayPlayer(Replay replay)
        {
            Replay = replay;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            RulesetContainer.SetReplay(Replay);
        }
    }
}
