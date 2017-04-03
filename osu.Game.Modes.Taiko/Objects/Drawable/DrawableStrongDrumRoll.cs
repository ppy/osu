// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Modes.Taiko.Judgements;
using osu.Game.Modes.Taiko.Objects.Drawable.Pieces;

namespace osu.Game.Modes.Taiko.Objects.Drawable
{
    public class DrawableStrongDrumRoll : DrawableDrumRoll
    {
        public DrawableStrongDrumRoll(DrumRoll drumRoll)
            : base(drumRoll)
        {
        }

        protected override TaikoJudgement CreateJudgement() => new TaikoJudgement { SecondHit = true };

        protected override CirclePiece CreateCirclePiece() => new StrongCirclePiece();
    }
}
