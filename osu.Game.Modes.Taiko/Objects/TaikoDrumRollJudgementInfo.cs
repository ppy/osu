// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Modes.Objects.Drawables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.Game.Modes.Taiko.Objects
{
    public class TaikoDrumRollTickJudgementInfo : TaikoJudgementInfo
    {
        protected override int ScoreToInt(TaikoScoreResult result)
        {
            switch (result)
            {
                default:
                    return 0;
                case TaikoScoreResult.Great:
                    return 200;
            }
        }

        protected override int AccuracyScoreToInt(TaikoScoreResult result)
        {
            return 0;
        }
    }
}
