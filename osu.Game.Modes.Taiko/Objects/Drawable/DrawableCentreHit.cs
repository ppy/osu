using System.Collections.Generic;
using OpenTK.Input;
using osu.Game.Modes.Taiko.Objects.Drawable.Pieces;

namespace osu.Game.Modes.Taiko.Objects.Drawable
{
    public class DrawableCentreHit : DrawableHit
    {
        protected override List<Key> HitKeys { get; } = new List<Key>(new Key[] { Key.F, Key.J });

        public DrawableCentreHit(Hit hit)
            : base(hit)
        {
            Add(new CentreHitCirclePiece(new CirclePiece()));
        }
    }
}
