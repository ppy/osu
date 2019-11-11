// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.MathUtils;
using osu.Game.Rulesets.Catch.Objects.Drawable.Pieces;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Catch.Objects.Drawable
{
    public class DrawableFruit : PalpableCatchHitObject<Fruit>
    {
        private Circle border;

        private const float drawable_radius = (float)CatchHitObject.OBJECT_RADIUS * radius_adjust;

        /// <summary>
        /// Because we're adding a border around the fruit, we need to scale down some.
        /// </summary>
        private const float radius_adjust = 1.1f;

        public DrawableFruit(Fruit h)
            : base(h)
        {
            Origin = Anchor.Centre;

            Size = new Vector2(drawable_radius);
            Masking = false;

            Rotation = (float)(RNG.NextDouble() - 0.5f) * 40;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            // todo: this should come from the skin.
            AccentColour.Value = colourForRepresentation(HitObject.VisualRepresentation);

            AddRangeInternal(new[]
            {
                createPulp(HitObject.VisualRepresentation),
                border = new Circle
                {
                    EdgeEffect = new EdgeEffectParameters
                    {
                        Hollow = !HitObject.HyperDash,
                        Type = EdgeEffectType.Glow,
                        Radius = 4 * radius_adjust,
                        Colour = HitObject.HyperDash ? Color4.Red : AccentColour.Value.Darken(1).Opacity(0.6f)
                    },
                    Size = new Vector2(Height),
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    BorderColour = Color4.White,
                    BorderThickness = 3f * radius_adjust,
                    Children = new Framework.Graphics.Drawable[]
                    {
                        new Box
                        {
                            AlwaysPresent = true,
                            Colour = AccentColour.Value,
                            Alpha = 0,
                            RelativeSizeAxes = Axes.Both
                        }
                    }
                },
            });

            if (HitObject.HyperDash)
            {
                AddInternal(new Pulp
                {
                    RelativePositionAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    AccentColour = Color4.Red,
                    Blending = BlendingParameters.Additive,
                    Alpha = 0.5f,
                    Scale = new Vector2(1.333f)
                });
            }
        }

        private Framework.Graphics.Drawable createPulp(FruitVisualRepresentation representation)
        {
            const float large_pulp_3 = 8f * radius_adjust;
            const float distance_from_centre_3 = 0.15f;

            const float large_pulp_4 = large_pulp_3 * 0.925f;
            const float distance_from_centre_4 = distance_from_centre_3 / 0.925f;

            const float small_pulp = large_pulp_3 / 2;

            static Vector2 positionAt(float angle, float distance) => new Vector2(
                distance * MathF.Sin(angle * MathF.PI / 180),
                distance * MathF.Cos(angle * MathF.PI / 180));

            return representation switch
            {
                FruitVisualRepresentation.Raspberry => new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Children = new Framework.Graphics.Drawable[]
                    {
                        new Pulp
                        {
                            AccentColour = AccentColour.Value,
                            Size = new Vector2(small_pulp),
                            Y = -0.34f,
                        },
                        new Pulp
                        {
                            AccentColour = AccentColour.Value,
                            Size = new Vector2(large_pulp_4),
                            Position = positionAt(0, distance_from_centre_4),
                        },
                        new Pulp
                        {
                            AccentColour = AccentColour.Value,
                            Size = new Vector2(large_pulp_4),
                            Position = positionAt(90, distance_from_centre_4),
                        },
                        new Pulp
                        {
                            AccentColour = AccentColour.Value,
                            Size = new Vector2(large_pulp_4),
                            Position = positionAt(180, distance_from_centre_4),
                        },
                        new Pulp
                        {
                            Size = new Vector2(large_pulp_4),
                            AccentColour = AccentColour.Value,
                            Position = positionAt(270, distance_from_centre_4),
                        },
                    }
                },

                FruitVisualRepresentation.Pineapple => new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Children = new Framework.Graphics.Drawable[]
                    {
                        new Pulp
                        {
                            AccentColour = AccentColour.Value,
                            Size = new Vector2(small_pulp),
                            Y = -0.3f,
                        },
                        new Pulp
                        {
                            AccentColour = AccentColour.Value,
                            Size = new Vector2(large_pulp_4),
                            Position = positionAt(45, distance_from_centre_4),
                        },
                        new Pulp
                        {
                            AccentColour = AccentColour.Value,
                            Size = new Vector2(large_pulp_4),
                            Position = positionAt(135, distance_from_centre_4),
                        },
                        new Pulp
                        {
                            AccentColour = AccentColour.Value,
                            Size = new Vector2(large_pulp_4),
                            Position = positionAt(225, distance_from_centre_4),
                        },
                        new Pulp
                        {
                            Size = new Vector2(large_pulp_4),
                            AccentColour = AccentColour.Value,
                            Position = positionAt(315, distance_from_centre_4),
                        },
                    }
                },

                FruitVisualRepresentation.Pear => new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Children = new Framework.Graphics.Drawable[]
                    {
                        new Pulp
                        {
                            AccentColour = AccentColour.Value,
                            Size = new Vector2(small_pulp),
                            Y = -0.33f,
                        },
                        new Pulp
                        {
                            AccentColour = AccentColour.Value,
                            Size = new Vector2(large_pulp_3),
                            Position = positionAt(60, distance_from_centre_3),
                        },
                        new Pulp
                        {
                            AccentColour = AccentColour.Value,
                            Size = new Vector2(large_pulp_3),
                            Position = positionAt(180, distance_from_centre_3),
                        },
                        new Pulp
                        {
                            Size = new Vector2(large_pulp_3),
                            AccentColour = AccentColour.Value,
                            Position = positionAt(300, distance_from_centre_3),
                        },
                    }
                },

                FruitVisualRepresentation.Grape => new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Children = new Framework.Graphics.Drawable[]
                    {
                        new Pulp
                        {
                            AccentColour = AccentColour.Value,
                            Size = new Vector2(small_pulp),
                            Y = -0.25f,
                        },
                        new Pulp
                        {
                            AccentColour = AccentColour.Value,
                            Size = new Vector2(large_pulp_3),
                            Position = positionAt(0, distance_from_centre_3),
                        },
                        new Pulp
                        {
                            AccentColour = AccentColour.Value,
                            Size = new Vector2(large_pulp_3),
                            Position = positionAt(120, distance_from_centre_3),
                        },
                        new Pulp
                        {
                            Size = new Vector2(large_pulp_3),
                            AccentColour = AccentColour.Value,
                            Position = positionAt(240, distance_from_centre_3),
                        },
                    }
                },

                FruitVisualRepresentation.Banana => new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Children = new Framework.Graphics.Drawable[]
                    {
                        new Pulp
                        {
                            AccentColour = AccentColour.Value,
                            Size = new Vector2(small_pulp),
                            Y = -0.3f
                        },
                        new Pulp
                        {
                            AccentColour = AccentColour.Value,
                            Size = new Vector2(large_pulp_4 * 0.8f, large_pulp_4 * 2.5f),
                            Y = 0.05f,
                        },
                    }
                },
                _ => new Container(),
            };
        }

        protected override void Update()
        {
            base.Update();

            border.Alpha = (float)Math.Clamp((HitObject.StartTime - Time.Current) / 500, 0, 1);
        }

        private Color4 colourForRepresentation(FruitVisualRepresentation representation)
            => representation switch
            {
                FruitVisualRepresentation.Grape => new Color4(204, 102, 0, 255),
                FruitVisualRepresentation.Raspberry => new Color4(121, 9, 13, 255),
                FruitVisualRepresentation.Pineapple => new Color4(102, 136, 0, 255),
                FruitVisualRepresentation.Banana => (RNG.Next(0, 3)) switch
                {
                    1 => new Color4(255, 192, 0, 255),
                    2 => new Color4(214, 221, 28, 255),
                    _ => new Color4(255, 240, 0, 255),
                },
                // defaults to Pear
                _ => new Color4(17, 136, 170, 255),
            };
    }
}
