// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE


using osu.Game.Modes.Objects.Drawables;
using osu.Game.Modes.Taiko.Objects.Drawables.Pieces.DrumRoll;
using OpenTK;

namespace osu.Game.Modes.Taiko.Objects.Drawables.DrumRolls
{
    public class DrawableDrumRollFinisher : DrawableDrumRoll
    {
        public DrawableDrumRollFinisher(DrumRoll drumRoll)
            : base(drumRoll)
        {
            Size *= new Vector2(1, 1.5f);
        }

        protected override JudgementInfo CreateJudgementInfo() => new TaikoJudgementInfo { MaxScore = TaikoScoreResult.Great, SecondHit = true };

        protected override DrawableDrumRollTick CreateTick(DrumRoll drumRoll, DrumRollTick tick) => new DrawableDrumRollFinisherTick(drumRoll, tick);

        protected override DrumRollBodyPiece CreateBody(float length) => new DrumRollFinisherBodyPiece(length);
    }
}
