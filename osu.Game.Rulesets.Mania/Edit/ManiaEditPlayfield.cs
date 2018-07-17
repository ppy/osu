// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Rulesets.Mania.Beatmaps;
using osu.Game.Rulesets.Mania.UI;
using System.Collections.Generic;
using osu.Game.Rulesets.UI.Scrolling;

namespace osu.Game.Rulesets.Mania.Edit
{
    public class ManiaEditPlayfield : ManiaPlayfield
    {
        protected override bool DisplayJudgements => false;

        public ManiaEditPlayfield(ScrollingDirection direction, List<StageDefinition> stages)
            : base(direction, stages)
        {
        }

        protected override ManiaStage CreateStage(ScrollingDirection direction, int firstColumnIndex, StageDefinition definition, ref ManiaAction normalColumnStartAction, ref ManiaAction specialColumnStartAction)
            => new ManiaEditStage(direction, firstColumnIndex, definition, ref normalColumnStartAction, ref specialColumnStartAction);
    }
}
