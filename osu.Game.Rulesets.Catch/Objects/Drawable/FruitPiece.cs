// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Rulesets.Catch.Objects.Drawable.Pieces;
using osu.Game.Rulesets.Objects.Drawables;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Catch.Objects.Drawable
{
    internal class FruitPiece : CompositeDrawable
    {
        /// <summary>
        /// Because we're adding a border around the fruit, we need to scale down some.
        /// </summary>
        private const float radius_adjust = 1.1f;

        private Circle border;

        private CatchHitObject hitObject;

        private readonly IBindable<Color4> accentColour = new Bindable<Color4>();

        public FruitPiece()
        {
            RelativeSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load(DrawableHitObject drawableObject)
        {
            DrawableCatchHitObject drawableCatchObject = (DrawableCatchHitObject)drawableObject;
            hitObject = drawableCatchObject.HitObject;

            accentColour.BindTo(drawableCatchObject.AccentColour);

            AddRangeInternal(new[]
            {
                createPulp(drawableCatchObject.HitObject.VisualRepresentation),
                border = new Circle
                {
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    BorderColour = Color4.White,
                    BorderThickness = 6f * radius_adjust,
                    Children = new Framework.Graphics.Drawable[]
                    {
                        new Box
                        {
                            AlwaysPresent = true,
                            Alpha = 0,
                            RelativeSizeAxes = Axes.Both
                        }
                    }
                },
            });

            if (hitObject.HyperDash)
            {
                AddInternal(new Pulp
                {
                    RelativePositionAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    AccentColour = { Value = Color4.Red },
                    Blending = BlendingParameters.Additive,
                    Alpha = 0.5f,
                    Scale = new Vector2(1.333f)
                });
            }
        }

        protected override void Update()
        {
            base.Update();

            border.Alpha = (float)Math.Clamp((hitObject.StartTime - Time.Current) / 500, 0, 1);
        }

        private Framework.Graphics.Drawable createPulp(FruitVisualRepresentation representation)
        {
            const float large_pulp_3 = 16f * radius_adjust;
            const float distance_from_centre_3 = 0.15f;

            const float large_pulp_4 = large_pulp_3 * 0.925f;
            const float distance_from_centre_4 = distance_from_centre_3 / 0.925f;

            const float small_pulp = large_pulp_3 / 2;

            static Vector2 positionAt(float angle, float distance) => new Vector2(
                distance * MathF.Sin(angle * MathF.PI / 180),
                distance * MathF.Cos(angle * MathF.PI / 180));

            switch (representation)
            {
                default:
                    return new Container();

                case FruitVisualRepresentation.Raspberry:
                    return new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Children = new Framework.Graphics.Drawable[]
                        {
                            new Pulp
                            {
                                AccentColour = { BindTarget = accentColour },
                                Size = new Vector2(small_pulp),
                                Y = -0.34f,
                            },
                            new Pulp
                            {
                                AccentColour = { BindTarget = accentColour },
                                Size = new Vector2(large_pulp_4),
                                Position = positionAt(0, distance_from_centre_4),
                            },
                            new Pulp
                            {
                                AccentColour = { BindTarget = accentColour },
                                Size = new Vector2(large_pulp_4),
                                Position = positionAt(90, distance_from_centre_4),
                            },
                            new Pulp
                            {
                                AccentColour = { BindTarget = accentColour },
                                Size = new Vector2(large_pulp_4),
                                Position = positionAt(180, distance_from_centre_4),
                            },
                            new Pulp
                            {
                                Size = new Vector2(large_pulp_4),
                                AccentColour = { BindTarget = accentColour },
                                Position = positionAt(270, distance_from_centre_4),
                            },
                        }
                    };

                case FruitVisualRepresentation.Pineapple:
                    return new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Children = new Framework.Graphics.Drawable[]
                        {
                            new Pulp
                            {
                                AccentColour = { BindTarget = accentColour },
                                Size = new Vector2(small_pulp),
                                Y = -0.3f,
                            },
                            new Pulp
                            {
                                AccentColour = { BindTarget = accentColour },
                                Size = new Vector2(large_pulp_4),
                                Position = positionAt(45, distance_from_centre_4),
                            },
                            new Pulp
                            {
                                AccentColour = { BindTarget = accentColour },
                                Size = new Vector2(large_pulp_4),
                                Position = positionAt(135, distance_from_centre_4),
                            },
                            new Pulp
                            {
                                AccentColour = { BindTarget = accentColour },
                                Size = new Vector2(large_pulp_4),
                                Position = positionAt(225, distance_from_centre_4),
                            },
                            new Pulp
                            {
                                Size = new Vector2(large_pulp_4),
                                AccentColour = { BindTarget = accentColour },
                                Position = positionAt(315, distance_from_centre_4),
                            },
                        }
                    };

                case FruitVisualRepresentation.Pear:
                    return new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Children = new Framework.Graphics.Drawable[]
                        {
                            new Pulp
                            {
                                AccentColour = { BindTarget = accentColour },
                                Size = new Vector2(small_pulp),
                                Y = -0.33f,
                            },
                            new Pulp
                            {
                                AccentColour = { BindTarget = accentColour },
                                Size = new Vector2(large_pulp_3),
                                Position = positionAt(60, distance_from_centre_3),
                            },
                            new Pulp
                            {
                                AccentColour = { BindTarget = accentColour },
                                Size = new Vector2(large_pulp_3),
                                Position = positionAt(180, distance_from_centre_3),
                            },
                            new Pulp
                            {
                                Size = new Vector2(large_pulp_3),
                                AccentColour = { BindTarget = accentColour },
                                Position = positionAt(300, distance_from_centre_3),
                            },
                        }
                    };

                case FruitVisualRepresentation.Grape:
                    return new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Children = new Framework.Graphics.Drawable[]
                        {
                            new Pulp
                            {
                                AccentColour = { BindTarget = accentColour },
                                Size = new Vector2(small_pulp),
                                Y = -0.25f,
                            },
                            new Pulp
                            {
                                AccentColour = { BindTarget = accentColour },
                                Size = new Vector2(large_pulp_3),
                                Position = positionAt(0, distance_from_centre_3),
                            },
                            new Pulp
                            {
                                AccentColour = { BindTarget = accentColour },
                                Size = new Vector2(large_pulp_3),
                                Position = positionAt(120, distance_from_centre_3),
                            },
                            new Pulp
                            {
                                Size = new Vector2(large_pulp_3),
                                AccentColour = { BindTarget = accentColour },
                                Position = positionAt(240, distance_from_centre_3),
                            },
                        }
                    };

                case FruitVisualRepresentation.Banana:

                    return new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Children = new Framework.Graphics.Drawable[]
                        {
                            new Pulp
                            {
                                AccentColour = { BindTarget = accentColour },
                                Size = new Vector2(small_pulp),
                                Y = -0.3f
                            },
                            new Pulp
                            {
                                AccentColour = { BindTarget = accentColour },
                                Size = new Vector2(large_pulp_4 * 0.8f, large_pulp_4 * 2.5f),
                                Y = 0.05f,
                            },
                        }
                    };
            }
        }
    }
}
