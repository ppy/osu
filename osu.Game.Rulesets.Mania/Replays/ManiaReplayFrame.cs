// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mania.Beatmaps;
using osu.Game.Rulesets.Replays;
using osu.Game.Rulesets.Replays.Legacy;
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

        public void ConvertFrom(LegacyReplayFrame legacyFrame, Beatmap beatmap)
        {
            // We don't need to fully convert, just create the converter
            var converter = new ManiaBeatmapConverter(beatmap.BeatmapInfo.RulesetID == 3, beatmap);

            // NB: Via co-op mod, osu-stable can have two stages with floor(col/2) and ceil(col/2) columns. This will need special handling
            // elsewhere in the game if we do choose to support the old co-op mod anyway. For now, assume that there is only one stage.

            var stage = new StageDefinition { Columns = converter.TargetColumns };

            var normalAction = ManiaAction.Key1;
            var specialAction = ManiaAction.Special1;

            int activeColumns = (int)(legacyFrame.MouseX ?? 0);
            int counter = 0;
            while (activeColumns > 0)
            {
                var isSpecial = stage.IsSpecialColumn(counter);

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
    }
}
