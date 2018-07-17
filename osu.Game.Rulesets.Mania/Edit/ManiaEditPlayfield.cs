// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Rulesets.Mania.Beatmaps;
using osu.Game.Rulesets.Mania.UI;
using System.Collections.Generic;

namespace osu.Game.Rulesets.Mania.Edit
{
    public class ManiaEditPlayfield : ManiaPlayfield
    {
        protected override bool DisplayJudgements => false;

        public ManiaEditPlayfield(List<StageDefinition> stages)
            : base(stages)
        {
        }

        protected override ManiaStage CreateStage(int firstColumnIndex, StageDefinition definition, ref ManiaAction normalColumnStartAction, ref ManiaAction specialColumnStartAction)
            => new ManiaEditStage(firstColumnIndex, definition, ref normalColumnStartAction, ref specialColumnStartAction);
    }
}
