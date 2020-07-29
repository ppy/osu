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
using osu.Game.Rulesets.Osu.Objects.Drawables.Pieces;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Osu.Objects.Drawables
{
    public class DefaultSpinnerCentre : CompositeDrawable
    {
        private DrawableSpinner spinner;

        private CirclePiece circle;
        private GlowPiece glow;
        private SpriteIcon symbol;

        [BackgroundDependencyLoader]
        private void load(OsuColour colours, DrawableHitObject drawableHitObject)
        {
            spinner = (DrawableSpinner)drawableHitObject;

            AutoSizeAxes = Axes.Both;
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
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

            drawableHitObject.State.BindValueChanged(val =>
            {
                Color4 colour;

                switch (val.NewValue)
                {
                    default:
                        colour = colours.BlueDark;
                        break;

                    case ArmedState.Hit:
                        colour = colours.YellowLight;
                        break;
                }

                circle.FadeColour(colour, 200);
                glow.FadeColour(colour, 200);
            }, true);

            FinishTransforms(true);
        }

        protected override void Update()
        {
            base.Update();

            circle.Rotation = spinner.Disc.Rotation;
            symbol.Rotation = (float)Interpolation.Lerp(symbol.Rotation, spinner.Disc.Rotation / 2, Math.Clamp(Math.Abs(Time.Elapsed) / 40, 0, 1));
        }
    }
}
