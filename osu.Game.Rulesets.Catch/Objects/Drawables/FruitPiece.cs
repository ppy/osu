// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.Objects.Drawables;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Catch.Objects.Drawables
{
    internal class FruitPiece : CompositeDrawable
    {
        /// <summary>
        /// Because we're adding a border around the fruit, we need to scale down some.
        /// </summary>
        public const float RADIUS_ADJUST = 1.1f;

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
                getFruitFor(drawableCatchObject.HitObject.VisualRepresentation),
                border = new Circle
                {
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    BorderColour = Color4.White,
                    BorderThickness = 6f * RADIUS_ADJUST,
                    Children = new Drawable[]
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
                AddInternal(new Circle
                {
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    BorderColour = Catcher.DEFAULT_HYPER_DASH_COLOUR,
                    BorderThickness = 12f * RADIUS_ADJUST,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            AlwaysPresent = true,
                            Alpha = 0.3f,
                            Blending = BlendingParameters.Additive,
                            RelativeSizeAxes = Axes.Both,
                            Colour = Catcher.DEFAULT_HYPER_DASH_COLOUR,
                        }
                    }
                });
            }
        }

        protected override void Update()
        {
            base.Update();
            border.Alpha = (float)Math.Clamp((hitObject.StartTime - Time.Current) / 500, 0, 1);
        }

        private Drawable getFruitFor(FruitVisualRepresentation representation)
        {
            switch (representation)
            {
                case FruitVisualRepresentation.Pear:
                    return new PearPiece();

                case FruitVisualRepresentation.Grape:
                    return new GrapePiece();

                case FruitVisualRepresentation.Pineapple:
                    return new PineapplePiece();

                case FruitVisualRepresentation.Banana:
                    return new BananaPiece();

                case FruitVisualRepresentation.Raspberry:
                    return new RaspberryPiece();
            }

            return Empty();
        }
    }
}
