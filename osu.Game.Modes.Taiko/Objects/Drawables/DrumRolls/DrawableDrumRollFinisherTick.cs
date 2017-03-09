// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Modes.Objects.Drawables;

namespace osu.Game.Modes.Taiko.Objects.Drawables.DrumRolls
{
    public class DrawableDrumRollFinisherTick : DrawableDrumRollTick
    {
        public DrawableDrumRollFinisherTick(DrumRollTick drumRollTick)
            : base(drumRollTick)
        {
        }

        protected override JudgementInfo CreateJudgementInfo() => new TaikoDrumRollTickJudgementInfo { MaxScore = TaikoScoreResult.Great, SecondHit = true };
    }
}
