using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.Game.Modes.Taiko.Objects.Drawables.Pieces
{
    public class DonRingPiece : RingPiece
    {
        protected override Color4 OverlayColour => new Color4(187, 17, 119, 255);
        protected override string Prefix => @"don";
    }

    public class KatsuRingPiece : RingPiece
    {
        protected override Color4 OverlayColour => new Color4(17, 136, 170, 255);
        protected override string Prefix => @"katsu";
    }

    public class SpinnerRingPiece : RingPiece
    {
        protected override Color4 OverlayColour => new Color4(237, 171, 0, 255);
        protected override string Prefix => @"spinner";
    }

    public abstract class RingPiece : Container
    {
        protected abstract Color4 OverlayColour { get; }
        protected abstract string Prefix { get; }

        private Sprite ringBase;
        private Sprite ringOverlay;
        private Sprite innerRingBase;
        private Sprite innerRingOverlay;

        public RingPiece()
        {
            Size = new Vector2(64);

            Masking = true;
            CornerRadius = Size.X / 2;

            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            Children = new[]
            {
                ringBase = new Sprite()
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                },
                ringOverlay = new Sprite()
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,

                    BlendingMode = BlendingMode.Additive,

                    Alpha = 0.5f
                },
                innerRingBase = new Sprite()
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(TextureStore textures)
        {
            ringBase.Texture = textures.Get(@"Play/Taiko/ring");
            ringOverlay.Texture = textures.Get(@"Play/Taiko/ring-overlay");

            innerRingBase.Texture = textures.Get($@"Play/Taiko/{Prefix}-inner-ring");
        }
    }
}
