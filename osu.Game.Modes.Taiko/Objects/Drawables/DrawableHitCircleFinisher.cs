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
        public DrawableHitCircleDonFinisher(TaikoHitObject hitObject)
            : base(hitObject)
        {
        }

        protected override HitCirclePiece CreateBody() => new DonFinisherPiece();
    }

    public class DrawableHitCircleKatsuFinisher : DrawableHitCircleFinisher
    {
        public DrawableHitCircleKatsuFinisher(TaikoHitObject hitObject)
            : base(hitObject)
        {
        }

        protected override HitCirclePiece CreateBody() => new KatsuFinisherPiece();
    }

    public abstract class DrawableHitCircleFinisher : DrawableHitCircle
    {
        public DrawableHitCircleFinisher(TaikoHitObject hitObject)
            : base(hitObject)
        {
            Size *= 1.5f;
        }

        // Todo: Hit finisher handling
    }
}
