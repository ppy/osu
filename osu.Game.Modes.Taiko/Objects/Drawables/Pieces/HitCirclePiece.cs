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

        protected override EdgeEffect CreateBackingGlow()
        {
            return new EdgeEffect()
            {
                Type = EdgeEffectType.Glow,
                Colour = new Color4(187, 17, 119, 255),
                Radius = 4f
            };
        }
    }

    class KatsuPiece : HitCirclePiece
    {
        protected override List<Key> Keys { get; } = new List<Key>(new[] { Key.D, Key.K });
        protected override Color4 InternalColour => new Color4(17, 136, 170, 255);

        protected override RingPiece CreateRing() => new KatsuRingPiece();

        protected override EdgeEffect CreateBackingGlow()
        {
            return new EdgeEffect()
            {
                Type = EdgeEffectType.Glow,
                Colour = new Color4(17, 136, 170, 255),
                Radius = 4f
            };
        }
    }

    class SpinnerPiece : HitCirclePiece
    {
        protected override List<Key> Keys { get; } = new List<Key>(new[] { Key.D, Key.F, Key.J, Key.K });
        protected override Color4 InternalColour => new Color4(237, 171, 0, 255);

        protected override bool OnKeyDown(InputState state, KeyDownEventArgs args)
        {
            if (!Keys.Contains(args.Key))
                return false;

            return Hit?.Invoke(true) ?? false;
        }

        protected override RingPiece CreateRing() => new SpinnerRingPiece();

        protected override EdgeEffect CreateBackingGlow()
        {
            return new EdgeEffect()
            {
                Type = EdgeEffectType.Glow,
                Colour = new Color4(237, 171, 0, 255),
                Radius = 4f
            };
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
        public Func<bool, bool> Hit;
        public bool Kiai;

        protected abstract List<Key> Keys { get; }

        protected abstract Color4 InternalColour { get; }

        private List<Key> validKeys = new List<Key>(new[] { Key.D, Key.F, Key.J, Key.K });
        private List<Key> pressedKeys = new List<Key>();

        private CirclePiece circle;
        private RingPiece ring;

        public HitCirclePiece()
        {
            Size = new Vector2(128);

            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            Children = new Drawable[]
            {
                new CircularContainer()
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,

                    RelativeSizeAxes = Axes.Both,
                    EdgeEffect = CreateBackingGlow()
                },
                circle = new CirclePiece()
                {
                    Colour = InternalColour
                },
                ring = CreateRing(),
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            if (Kiai)
            {
                circle.EdgeEffect = new EdgeEffect()
                {
                    Colour = new Color4(InternalColour.R, InternalColour.G, InternalColour.B, 0.75f),
                    Radius = 50,
                    Type = EdgeEffectType.Glow,
                };
            }
        }

        protected abstract EdgeEffect CreateBackingGlow();
        protected abstract RingPiece CreateRing();

        protected override bool OnKeyDown(InputState state, KeyDownEventArgs args)
        {
            if (args.Repeat)
                return false;

            // Check if we've pressed a valid taiko key
            if (!validKeys.Contains(args.Key))
                return false;

            // Don't handle re-presses of the same key
            if (pressedKeys.Contains(args.Key))
                return false;

            bool handled = Hit?.Invoke(Keys.Contains(args.Key)) ?? false;

            if (handled)
                pressedKeys.Add(args.Key);

            return handled;
        }
    }
}
