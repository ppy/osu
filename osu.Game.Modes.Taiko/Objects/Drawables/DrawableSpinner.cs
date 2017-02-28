using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Graphics;
using osu.Game.Modes.Taiko.Objects.Drawables.Pieces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.Game.Modes.Taiko.Objects.Drawables
{
    public class DrawableSpinner : DrawableTaikoHitObject
    {
        private HitCirclePiece bodyPiece;
        private ExplodePiece explodePiece;

        public DrawableSpinner(TaikoHitObject hitObject)
            : base(hitObject)
        {
            Size = new Vector2(128);

            Children = new Drawable[]
            {
                bodyPiece = new SpinnerPiece(),
                explodePiece = new ExplodePiece()
                {
                    Colour = new Color4(237, 171, 0, 255)
                }
            };
        }

        protected override void Update()
        {
            MoveToOffset(Math.Min(Time.Current, HitObject.StartTime));
        }
    }
}
