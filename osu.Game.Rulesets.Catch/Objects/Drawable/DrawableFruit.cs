// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.MathUtils;
using osu.Game.Rulesets.Catch.Objects.Drawable.Pieces;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Rulesets.Catch.Objects.Drawable
{
    public class DrawableFruit : DrawableCatchHitObject<Fruit>
    {
        public DrawableFruit(Fruit h)
            : base(h)
        {
            Origin = Anchor.Centre;

            Size = new Vector2(Pulp.PULP_SIZE * 2.2f, Pulp.PULP_SIZE * 2.8f);
            AccentColour = HitObject.ComboColour;
            Masking = false;

            Rotation = (float)(RNG.NextDouble() - 0.5f) * 40;
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

            if (HitObject.HyperDash)
            {
                Add(new Pulp
                {
                    RelativePositionAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    AccentColour = Color4.Red,
                    Blending = BlendingMode.Additive,
                    Alpha = 0.5f,
                    Scale = new Vector2(2)
                });
            }
        }
    }
}
