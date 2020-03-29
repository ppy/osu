// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Replays.Legacy;
using osu.Game.Rulesets.Mania.Beatmaps;
using osu.Game.Rulesets.Replays;
using osu.Game.Rulesets.Replays.Types;

namespace osu.Game.Rulesets.Mania.Replays
{
    public class ManiaReplayFrame : ReplayFrame, IConvertibleReplayFrame
    {
        public List<ManiaAction> Actions = new List<ManiaAction>();

        public ManiaReplayFrame()
        {
        }

        public ManiaReplayFrame(double time, params ManiaAction[] actions)
            : base(time)
        {
            Actions.AddRange(actions);
        }

        public void FromLegacy(LegacyReplayFrame legacyFrame, IBeatmap beatmap, ReplayFrame lastFrame = null)
        {
            var maniaBeatmap = (ManiaBeatmap)beatmap;

            var normalAction = ManiaAction.Key1;
            var specialAction = ManiaAction.Special1;

            int activeColumns = (int)(legacyFrame.MouseX ?? 0);
            int counter = 0;

            while (activeColumns > 0)
            {
                var isSpecial = maniaBeatmap.Stages.First().IsSpecialColumn(counter);

                if ((activeColumns & 1) > 0)
                    Actions.Add(isSpecial ? specialAction : normalAction);

                if (isSpecial)
                    specialAction++;
                else
                    normalAction++;

                counter++;
                activeColumns >>= 1;
            }
        }

        public LegacyReplayFrame ToLegacy(IBeatmap beatmap)
        {
            var maniaBeatmap = (ManiaBeatmap)beatmap;

            int keys = 0;

            var specialColumns = new List<int>();

            for (int i = 0; i < maniaBeatmap.TotalColumns; i++)
            {
                if (maniaBeatmap.Stages.First().IsSpecialColumn(i))
                    specialColumns.Add(i);
            }

            foreach (var action in Actions)
            {
                switch (action)
                {
                    case ManiaAction.Special1:
                        keys |= 1 << specialColumns[0];
                        break;

                    case ManiaAction.Special2:
                        keys |= 1 << specialColumns[1];
                        break;

                    default:
                        keys |= 1 << (action - ManiaAction.Key1);
                        break;
                }
            }

            return new LegacyReplayFrame(Time, keys, null, ReplayButtonState.None);
        }
    }
}
