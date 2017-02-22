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
    public class HitCircleDon : HitCircle
    {
        protected override HitCirclePiece CreateBody() => new DonPiece();
    }

    public class HitCircleKatsu : HitCircle
    {
        protected override HitCirclePiece CreateBody() => new KatsuPiece();
    }

    public abstract class HitCircle : Container
    {
        public HitCircle()
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
