// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Input;
using osu.Game.Rulesets.Mania.UI;
using osu.Game.Rulesets.Replays;

namespace osu.Game.Rulesets.Mania.Replays
{
    internal class ManiaFramedReplayInputHandler : FramedReplayInputHandler
    {
        private readonly ManiaRulesetContainer container;

        public ManiaFramedReplayInputHandler(Replay replay, ManiaRulesetContainer container)
            : base(replay)
        {
            this.container = container;
        }

        private ManiaPlayfield playfield;
        public override List<InputState> GetPendingStates()
        {
            var actions = new List<ManiaAction>();

            if (playfield == null)
                playfield = (ManiaPlayfield)container.Playfield;

            int activeColumns = (int)(CurrentFrame.MouseX ?? 0);
            int counter = 0;
            while (activeColumns > 0)
            {
                if ((activeColumns & 1) > 0)
                    actions.Add(playfield.Columns.ElementAt(counter).Action);
                counter++;
                activeColumns >>= 1;
            }

            return new List<InputState> { new ReplayState<ManiaAction> { PressedActions = actions } };
        }
    }
}
