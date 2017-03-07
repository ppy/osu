// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE


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
