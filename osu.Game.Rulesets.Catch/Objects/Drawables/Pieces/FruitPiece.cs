// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Objects.Drawables;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Catch.Objects.Drawables.Pieces
{
    internal class FruitPiece : CompositeDrawable
    {
        /// <summary>
        /// Because we're adding a border around the fruit, we need to scale down some.
        /// </summary>
        public const float RADIUS_ADJUST = 1.1f;

        public readonly Bindable<FruitVisualRepresentation> VisualRepresentation = new Bindable<FruitVisualRepresentation>();
        public readonly Bindable<bool> HyperDash = new Bindable<bool>();
        public readonly Bindable<Color4> AccentColour = new Bindable<Color4>();

        [CanBeNull]
        private DrawableCatchHitObject drawableHitObject;

        [CanBeNull]
        private BorderPiece borderPiece;

        public FruitPiece()
        {
            RelativeSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load([CanBeNull] DrawableHitObject drawable)
        {
            drawableHitObject = (DrawableCatchHitObject)drawable;

            if (drawable != null)
                AccentColour.BindTo(drawable.AccentColour);

            AddRangeInternal(new Drawable[]
            {
                new FruitPulpFormation
                {
                    VisualRepresentation = { BindTarget = VisualRepresentation },
                    AccentColour = { BindTarget = AccentColour },
                },
                borderPiece = new BorderPiece(),
            });

            // if it is not part of a DHO, the border is always invisible.
            if (drawableHitObject != null)
                AddInternal(borderPiece = new BorderPiece());

            if (HyperDash.Value)
                AddInternal(new HyperBorderPiece());
        }

        protected override void Update()
        {
            if (borderPiece != null && drawableHitObject?.HitObject != null)
                borderPiece.Alpha = (float)Math.Clamp((drawableHitObject.HitObject.StartTime - Time.Current) / 500, 0, 1);
        }
    }

    internal class FruitPulpFormation : PulpFormation
    {
        public readonly Bindable<FruitVisualRepresentation> VisualRepresentation = new Bindable<FruitVisualRepresentation>();

        protected override void LoadComplete()
        {
            base.LoadComplete();
            VisualRepresentation.BindValueChanged(setFormation, true);
        }

        private void setFormation(ValueChangedEvent<FruitVisualRepresentation> visualRepresentation)
        {
            Clear();

            switch (visualRepresentation.NewValue)
            {
                case FruitVisualRepresentation.Pear:
                    Add(new Vector2(0, -0.33f), new Vector2(SMALL_PULP));
                    Add(PositionAt(60, DISTANCE_FROM_CENTRE_3), new Vector2(LARGE_PULP_3));
                    Add(PositionAt(180, DISTANCE_FROM_CENTRE_3), new Vector2(LARGE_PULP_3));
                    Add(PositionAt(300, DISTANCE_FROM_CENTRE_3), new Vector2(LARGE_PULP_3));
                    break;

                case FruitVisualRepresentation.Grape:
                    Add(new Vector2(0, -0.25f), new Vector2(SMALL_PULP));
                    Add(PositionAt(0, DISTANCE_FROM_CENTRE_3), new Vector2(LARGE_PULP_3));
                    Add(PositionAt(120, DISTANCE_FROM_CENTRE_3), new Vector2(LARGE_PULP_3));
                    Add(PositionAt(240, DISTANCE_FROM_CENTRE_3), new Vector2(LARGE_PULP_3));
                    break;

                case FruitVisualRepresentation.Pineapple:
                    Add(new Vector2(0, -0.3f), new Vector2(SMALL_PULP));
                    Add(PositionAt(45, DISTANCE_FROM_CENTRE_4), new Vector2(LARGE_PULP_4));
                    Add(PositionAt(135, DISTANCE_FROM_CENTRE_4), new Vector2(LARGE_PULP_4));
                    Add(PositionAt(225, DISTANCE_FROM_CENTRE_4), new Vector2(LARGE_PULP_4));
                    Add(PositionAt(315, DISTANCE_FROM_CENTRE_4), new Vector2(LARGE_PULP_4));
                    break;

                case FruitVisualRepresentation.Raspberry:
                    Add(new Vector2(0, -0.34f), new Vector2(SMALL_PULP));
                    Add(PositionAt(0, DISTANCE_FROM_CENTRE_4), new Vector2(LARGE_PULP_4));
                    Add(PositionAt(90, DISTANCE_FROM_CENTRE_4), new Vector2(LARGE_PULP_4));
                    Add(PositionAt(180, DISTANCE_FROM_CENTRE_4), new Vector2(LARGE_PULP_4));
                    Add(PositionAt(270, DISTANCE_FROM_CENTRE_4), new Vector2(LARGE_PULP_4));
                    break;

                case FruitVisualRepresentation.Banana:
                    Add(new Vector2(0, -0.3f), new Vector2(SMALL_PULP));
                    Add(new Vector2(0, 0.05f), new Vector2(LARGE_PULP_4 * 0.8f, LARGE_PULP_4 * 2.5f));
                    break;
            }
        }
    }
}
