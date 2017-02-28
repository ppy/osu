using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using osu.Framework.Input;

namespace osu.Game.Modes.Taiko.Objects.Drawables.Pieces
{
    class DonPiece : HitCirclePiece
    {
        protected override List<Key> Keys { get; } = new List<Key>(new[] { Key.F, Key.J });
        protected override Color4 InternalColour => new Color4(187, 17, 119, 255);

        protected override RingPiece CreateRing() => new DonRingPiece();
    }

    class KatsuPiece : HitCirclePiece
    {
        protected override List<Key> Keys { get; } = new List<Key>(new[] { Key.D, Key.K });
        protected override Color4 InternalColour => new Color4(17, 136, 170, 255);

        protected override RingPiece CreateRing() => new KatsuRingPiece();
    }

    class SpinnerPiece : HitCirclePiece
    {
        protected override List<Key> Keys => null;
        protected override Color4 InternalColour => new Color4(237, 171, 0, 255);

        protected override RingPiece CreateRing() => new SpinnerRingPiece();

        protected override bool OnKeyDown(InputState state, KeyDownEventArgs args)
        {
            return Hit?.Invoke() ?? false;
        }
    }

    class DonFinisherPiece : DonPiece
    {
        public DonFinisherPiece()
        {
            Scale *= new Vector2(1.5f);
        }
    }

    class KatsuFinisherPiece : KatsuPiece
    {
        public KatsuFinisherPiece()
        {
            Scale *= new Vector2(1.5f);
        }
    }

    public abstract class HitCirclePiece : Container
    {
        protected abstract List<Key> Keys { get; }
        protected abstract Color4 InternalColour { get; }

        private CirclePiece circle;
        private RingPiece ring;
        private GlowPiece glow;

        public Func<bool> Hit;

        public HitCirclePiece()
        {
            Size = new Vector2(128);

            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            Children = new Drawable[]
            {
                glow = new GlowPiece()
                {
                    Colour = InternalColour
                },
                circle = new CirclePiece()
                {
                    Colour = InternalColour,

                    EdgeEffect = new EdgeEffect()
                    {
                        Colour = new Color4(InternalColour.R, InternalColour.G, InternalColour.B, 0.75f),
                        Radius = 50,
                        Type = EdgeEffectType.Glow,
                    }
                },
                ring = CreateRing()
            };
        }

        protected abstract RingPiece CreateRing();

        protected override bool OnKeyDown(InputState state, KeyDownEventArgs args)
        {
            if (!Keys.Contains(args.Key))
                return false;

            Keys.RemoveAll(k => k == args.Key);

            return Hit?.Invoke() ?? false;
        }
    }
}
