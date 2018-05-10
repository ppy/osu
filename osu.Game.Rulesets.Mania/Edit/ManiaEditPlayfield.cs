// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Game.Rulesets.Mania.Beatmaps;
using osu.Game.Rulesets.Mania.UI;

namespace osu.Game.Rulesets.Mania.Edit
{
    public class ManiaEditPlayfield : ManiaPlayfield
    {
        protected override bool DisplayJudgements => false;

        public ManiaEditPlayfield(List<StageDefinition> stages)
            : base(stages)
        {
        }
    }
}
