using OpenTK;
using osu.Framework.Graphics.Containers;
using osu.Game.Modes.Taiko.Objects.Drawables.Pieces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.Game.Modes.Taiko.Objects.Drawables
{
    public class DrawableHitCircleDon : DrawableHitCircle
    {
        public DrawableHitCircleDon(TaikoHitObject hitObject)
            : base(hitObject)
        {
        }

        protected override HitCirclePiece CreateBody() => new DonPiece();
    }

    public class DrawableHitCircleKatsu : DrawableHitCircle
    {
        public DrawableHitCircleKatsu(TaikoHitObject hitObject)
            : base(hitObject)
        {
        }

        protected override HitCirclePiece CreateBody() => new KatsuPiece();
    }

    public abstract class DrawableHitCircle : DrawableTaikoHitObject
    {
        public DrawableHitCircle(TaikoHitObject hitObject)
            : base(hitObject)
        {
            Size = new Vector2(128);

            Children = new[]
            {
                CreateBody()
            };
        }

        protected abstract HitCirclePiece CreateBody();

        // Todo: Hit handling
    }
}
