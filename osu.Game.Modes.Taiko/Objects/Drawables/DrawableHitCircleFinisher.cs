using OpenTK;
using osu.Game.Modes.Taiko.Objects.Drawables.Pieces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.Game.Modes.Taiko.Objects.Drawables
{
    public class DrawableHitCircleDonFinisher : DrawableHitCircleFinisher
    {
        public DrawableHitCircleDonFinisher(HitCircle hitCircle)
            : base(hitCircle)
        {
        }

        protected override HitCirclePiece CreateBody() => new DonFinisherPiece();
    }

    public class DrawableHitCircleKatsuFinisher : DrawableHitCircleFinisher
    {
        public DrawableHitCircleKatsuFinisher(HitCircle hitCircle)
            : base(hitCircle)
        {
        }

        protected override HitCirclePiece CreateBody() => new KatsuFinisherPiece();
    }

    public abstract class DrawableHitCircleFinisher : DrawableHitCircle
    {
        public DrawableHitCircleFinisher(HitCircle hitCircle)
            : base(hitCircle)
        {
            Size *= 1.5f;
        }

        // Todo: Hit finisher handling
    }
}
