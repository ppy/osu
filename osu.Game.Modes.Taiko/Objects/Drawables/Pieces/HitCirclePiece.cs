using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.Game.Modes.Taiko.Objects.Drawables.Pieces
{
    class DonPiece : HitCirclePiece
    {
        protected override Color4 InternalColour => new Color4(187, 17, 119, 255);

        protected override RingPiece CreateRing() => new DonRingPiece();
    }

    class KatsuPiece : HitCirclePiece
    {
        protected override Color4 InternalColour => new Color4(17, 136, 170, 255);

        protected override RingPiece CreateRing() => new KatsuRingPiece();
    }

    class DonFinisherPiece : DonPiece
    {
        public DonFinisherPiece()
        {
            Scale *= new Vector2(1.6f);
        }
    }

    class KatsuFinisherPiece : KatsuPiece
    {
        public KatsuFinisherPiece()
        {
            Scale *= new Vector2(1.6f);
        }
    }

    public abstract class HitCirclePiece : Container
    {
        protected abstract Color4 InternalColour { get; }

        private CirclePiece circle;
        private RingPiece ring;
        private GlowPiece glow;

        public HitCirclePiece()
        {
            Size = new Vector2(128);

            Children = new Drawable[]
            {
                glow = new GlowPiece()
                {
                    Colour = InternalColour
                },
                circle = new CirclePiece()
                {
                    Colour = InternalColour
                },
                ring = CreateRing()
            };
        }

        protected abstract RingPiece CreateRing();
    }


}
