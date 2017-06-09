// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Game.Rulesets.Mania.UI;
using osu.Game.Rulesets.Timing;

namespace osu.Game.Rulesets.Mania.Mods
{
    internal interface IGenerateSpeedAdjustments
    {
        void ApplyToHitRenderer(ManiaHitRenderer hitRenderer, ref List<SpeedAdjustmentContainer>[] hitObjectTimingChanges, ref List<SpeedAdjustmentContainer> barlineTimingChanges);
    }
}
