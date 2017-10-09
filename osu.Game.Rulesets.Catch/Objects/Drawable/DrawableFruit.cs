// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Rulesets.Catch.Objects.Drawable
{
    public class DrawableFruit : DrawableCatchHitObject
    {
        private const float pulp_size = 20;

        public DrawableFruit(CatchBaseHit h)
            : base(h)
        {
            Size = new Vector2(pulp_size * 2.2f, pulp_size * 2.8f);
            AccentColour = HitObject.ComboColour;
            Masking = false;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Children = new Framework.Graphics.Drawable[]
            {
                //todo: share this more
                new BufferedContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    CacheDrawnFrameBuffer = true,
                    Children = new Framework.Graphics.Drawable[]
                    {
                        new Pulp
                        {
                            RelativePositionAxes = Axes.Both,
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            AccentColour = AccentColour,
                            Scale = new Vector2(0.6f),
                        },
                        new Pulp
                        {
                            RelativePositionAxes = Axes.Both,
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            AccentColour = AccentColour,
                            Y = -0.08f
                        },
                        new Pulp
                        {
                            RelativePositionAxes = Axes.Both,
                            Anchor = Anchor.CentreRight,
                            Origin = Anchor.CentreRight,
                            AccentColour = AccentColour,
                            Y = -0.08f
                        },
                        new Pulp
                        {
                            RelativePositionAxes = Axes.Both,
                            Anchor = Anchor.BottomCentre,
                            Origin = Anchor.BottomCentre,
                            AccentColour = AccentColour,
                        },
                    }
                }
            };
        }

        private class Pulp : Circle, IHasAccentColour
        {
            public Pulp()
            {
                Size = new Vector2(pulp_size);

                Blending = BlendingMode.Additive;
                Colour = Color4.White.Opacity(0.9f);
            }

            private Color4 accentColour;
            public Color4 AccentColour
            {
                get { return accentColour; }
                set
                {
                    accentColour = value;

                    EdgeEffect = new EdgeEffectParameters
                    {
                        Type = EdgeEffectType.Glow,
                        Radius = 5,
                        Colour = accentColour.Lighten(100),
                    };
                }
            }
        }
    }
}
