using OpenTK;
using osu.Game.Modes.Taiko.Objects.Drawables.Pieces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.Game.Modes.Taiko.Objects.Drawables
{
    public class HitCircleDonFinisher : HitCircleFinisher
    {
        protected override HitCirclePiece CreateBody() => new DonFinisherPiece();
    }

    public class HitCircleKatsuFinisher : HitCircleFinisher
    {
        protected override HitCirclePiece CreateBody() => new KatsuFinisherPiece();
    }

    public abstract class HitCircleFinisher : HitCircle
    {
        public HitCircleFinisher()
        {
            Size *= 1.6f;
        }

        // Todo: Hit finisher handling
    }
}
