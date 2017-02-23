using OpenTK;
using osu.Framework.Graphics.Containers;
using osu.Game.Modes.Taiko.Objects.Drawables.Pieces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using osu.Game.Modes.Objects.Drawables;

namespace osu.Game.Modes.Taiko.Objects.Drawables
{
    public class DrawableHitCircleDon : DrawableHitCircle
    {
        public DrawableHitCircleDon(HitCircle hitCircle)
            : base(hitCircle)
        {
        }

        protected override HitCirclePiece CreateBody() => new DonPiece();
    }

    public class DrawableHitCircleKatsu : DrawableHitCircle
    {
        public DrawableHitCircleKatsu(HitCircle hitCircle)
            : base(hitCircle)
        {
        }

        protected override HitCirclePiece CreateBody() => new KatsuPiece();
    }

    public abstract class DrawableHitCircle : DrawableTaikoHitObject
    {
        public DrawableHitCircle(HitCircle hitCircle)
            : base(hitCircle)
        {
            Size = new Vector2(128);

            Children = new[]
            {
                CreateBody()
            };
        }

        protected abstract HitCirclePiece CreateBody();

        protected override void UpdateState(ArmedState state)
        {
            if (!IsLoaded)
                return;

            base.UpdateState(state);


        }

        // Todo: Hit handling
    }
}
