// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Utils;
using osu.Game.Graphics;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Osu.Skinning.Default
{
    public partial class SpinnerCentreLayer : CompositeDrawable, IHasAccentColour
    {
        private DrawableSpinner spinner = null!;

        private CirclePiece circle = null!;
        private GlowPiece glow = null!;
        private SpriteIcon symbol = null!;

        [BackgroundDependencyLoader]
        private void load(DrawableHitObject drawableHitObject)
        {
            spinner = (DrawableSpinner)drawableHitObject;

            InternalChildren = new Drawable[]
            {
                glow = new GlowPiece(),
                circle = new CirclePiece
                {
                    Position = Vector2.Zero,
                    Anchor = Anchor.Centre,
                },
                new RingPiece(),
                symbol = new SpriteIcon
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(48),
                    Icon = FontAwesome.Solid.Asterisk,
                    Shadow = false,
                },
            };
        }

        protected override void Update()
        {
            base.Update();
            symbol.Rotation = (float)Interpolation.Lerp(symbol.Rotation, spinner.RotationTracker.Rotation / 2, Math.Clamp(Math.Abs(Time.Elapsed) / 40, 0, 1));
        }

        private Color4 accentColour;

        public Color4 AccentColour
        {
            get => accentColour;
            set
            {
                accentColour = value;

                circle.Colour = accentColour;
                glow.Colour = accentColour;
            }
        }
    }
}
