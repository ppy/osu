// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Rulesets.Mania.Beatmaps;
using osu.Game.Rulesets.Mania.UI;

namespace osu.Game.Rulesets.Mania.Edit
{
    public class ManiaEditStage : ManiaStage
    {
        public ManiaEditStage(int firstColumnIndex, StageDefinition definition, ref ManiaAction normalColumnStartAction, ref ManiaAction specialColumnStartAction)
            : base(firstColumnIndex, definition, ref normalColumnStartAction, ref specialColumnStartAction)
        {
        }
    }
}
