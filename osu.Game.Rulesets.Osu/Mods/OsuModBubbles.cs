// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Performance;
using osu.Framework.Graphics.Pooling;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Localisation;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Objects.Pooling;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Rulesets.Osu.Skinning.Default;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;
using osu.Game.Scoring;
using osuTK;

namespace osu.Game.Rulesets.Osu.Mods
{
    public partial class OsuModBubbles : ModWithVisibilityAdjustment, IApplicableToDrawableRuleset<OsuHitObject>, IApplicableToScoreProcessor
    {
        public override string Name => "Bubbles";

        public override string Acronym => "BB";

        public override LocalisableString Description => "Dont let their popping distract you!";

        public override double ScoreMultiplier => 1;

        public override ModType Type => ModType.Fun;

        // Compatibility with these seems potentially feasible in the future, blocked for now because they dont work as one would expect
        public override Type[] IncompatibleMods => new[] { typeof(OsuModBarrelRoll), typeof(OsuModMagnetised), typeof(OsuModRepel) };

        private PlayfieldAdjustmentContainer adjustmentContainer = null!;
        private BubbleContainer bubbleContainer = null!;

        private readonly Bindable<int> currentCombo = new BindableInt();

        private float maxSize;
        private float bubbleRadius;
        private double bubbleFade;

        public ScoreRank AdjustRank(ScoreRank rank, double accuracy) => rank;

        public void ApplyToScoreProcessor(ScoreProcessor scoreProcessor)
        {
            currentCombo.BindTo(scoreProcessor.Combo);
            currentCombo.BindValueChanged(combo =>
                maxSize = Math.Min(1.75f, (float)(1.25 + 0.005 * combo.NewValue)), true);
        }

        public void ApplyToDrawableRuleset(DrawableRuleset<OsuHitObject> drawableRuleset)
        {
            // Multiplying by 2 results in an initial size that is too large, hence 1.85 has been chosen
            bubbleRadius = (float)(drawableRuleset.Beatmap.HitObjects.OfType<HitCircle>().First().Radius * 1.85f);
            bubbleFade = drawableRuleset.Beatmap.HitObjects.OfType<HitCircle>().First().TimePreempt * 2;

            // We want to hide the judgements since they are obscured by the BubbleDrawable (due to layering)
            drawableRuleset.Playfield.DisplayJudgements.Value = false;

            adjustmentContainer = drawableRuleset.CreatePlayfieldAdjustmentContainer();

            adjustmentContainer.Add(bubbleContainer = new BubbleContainer());
            drawableRuleset.KeyBindingInputManager.Add(adjustmentContainer);
        }

        protected override void ApplyIncreasedVisibilityState(DrawableHitObject hitObject, ArmedState state) => applyBubbleState(hitObject);

        protected override void ApplyNormalVisibilityState(DrawableHitObject hitObject, ArmedState state) => applyBubbleState(hitObject);

        private void applyBubbleState(DrawableHitObject drawableObject)
        {
            if (drawableObject is DrawableSlider slider)
            {
                slider.Body.OnSkinChanged += () => applySliderState(slider);
                applySliderState(slider);
            }

            if (drawableObject is not DrawableOsuHitObject drawableOsuObject || !drawableObject.Judged) return;

            OsuHitObject hitObject = drawableOsuObject.HitObject;

            switch (drawableOsuObject)
            {
                //Needs to be done explicitly to avoid being handled by DrawableHitCircle below
                case DrawableSliderHead:
                    addBubbleContainer(hitObject.Position, drawableOsuObject);
                    break;

                //Stack leniency causes placement issues if this isn't handled as such.
                case DrawableHitCircle hitCircle:
                    addBubbleContainer(hitCircle.Position, drawableOsuObject);
                    break;

                case DrawableSlider:
                case DrawableSpinnerTick:
                    break;

                default:
                    addBubbleContainer(hitObject.Position, drawableOsuObject);
                    break;
            }
        }

        private void applySliderState(DrawableSlider slider) =>
            ((PlaySliderBody)slider.Body.Drawable).BorderColour = slider.AccentColour.Value;

        private void addBubbleContainer(Vector2 position, DrawableHitObject hitObject)
        {
            bubbleContainer.Add
            (
                new BubbleLifeTimeEntry
                {
                    LifetimeStart = bubbleContainer.Time.Current,
                    Colour = hitObject.AccentColour.Value,
                    Position = position,
                    InitialSize = new Vector2(bubbleRadius),
                    MaxSize = maxSize,
                    FadeTime = bubbleFade,
                    IsHit = hitObject.IsHit
                }
            );
        }

        #region Pooled Bubble drawable

        // LifetimeEntry flow is necessary to allow for correct rewind behaviour, can probably be made generic later if more mods are made requiring it
        // Todo: find solution to bubbles rewinding in "groups"
        private sealed partial class BubbleContainer : PooledDrawableWithLifetimeContainer<BubbleLifeTimeEntry, BubbleObject>
        {
            protected override bool RemoveRewoundEntry => true;

            private readonly DrawablePool<BubbleObject> pool;

            public BubbleContainer()
            {
                RelativeSizeAxes = Axes.Both;
                AddInternal(pool = new DrawablePool<BubbleObject>(10, 1000));
            }

            protected override BubbleObject GetDrawable(BubbleLifeTimeEntry entry) => pool.Get(d => d.Apply(entry));
        }

        private sealed partial class BubbleObject : PoolableDrawableWithLifetime<BubbleLifeTimeEntry>
        {
            private readonly BubbleDrawable bubbleDrawable;

            public BubbleObject()
            {
                InternalChild = bubbleDrawable = new BubbleDrawable();
            }

            protected override void OnApply(BubbleLifeTimeEntry entry)
            {
                base.OnApply(entry);
                if (IsLoaded)
                    apply(entry);
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();
                apply(Entry);
            }

            private void apply(BubbleLifeTimeEntry? entry)
            {
                if (entry == null) return;

                ApplyTransformsAt(float.MinValue, true);
                ClearTransforms(true);

                Position = entry.Position;

                bubbleDrawable.Animate(entry);

                LifetimeEnd = bubbleDrawable.LatestTransformEndTime;
            }
        }

        private partial class BubbleDrawable : CircularContainer
        {
            private readonly Circle innerCircle;
            private readonly Box colourBox;

            public BubbleDrawable()
            {
                Anchor = Anchor.Centre;
                Origin = Anchor.Centre;

                Masking = true;
                MaskingSmoothness = 2;
                BorderThickness = 0;
                BorderColour = Colour4.Transparent;
                EdgeEffect = new EdgeEffectParameters
                {
                    Type = EdgeEffectType.Shadow,
                    Radius = 3,
                    Colour = Colour4.Black.Opacity(0.05f)
                };

                Children = new Drawable[]
                {
                    colourBox = new Box { RelativeSizeAxes = Axes.Both, },
                    innerCircle = new Circle
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.Both,
                        Size = new Vector2(0.5f),
                    }
                };
            }

            public void Animate(BubbleLifeTimeEntry entry)
            {
                Size = entry.InitialSize;

                //We want to fade to a darker colour to avoid colours such as white hiding the "ripple" effect.
                var colourDarker = entry.Colour.Darken(0.1f);

                this.ScaleTo(entry.MaxSize, getAnimationDuration() * 0.8f)
                    .Then()
                    .ScaleTo(entry.MaxSize * 1.5f, getAnimationDuration() * 0.2f, Easing.OutQuint)
                    .FadeTo(0, getAnimationDuration() * 0.2f, Easing.OutQuint);

                innerCircle.ScaleTo(2f, getAnimationDuration() * 0.8f, Easing.OutQuint);

                if (!entry.IsHit)
                {
                    colourBox.Colour = Colour4.Black;
                    innerCircle.Colour = Colour4.Black;

                    return;
                }

                colourBox.FadeColour(colourDarker, getAnimationDuration() * 0.2f, Easing.OutQuint
                );
                innerCircle.FadeColour(colourDarker);

                // The absolute length of the bubble's animation, can be used in fractions for animations of partial length
                double getAnimationDuration() => 1700 + Math.Pow(entry.FadeTime, 1.07f);
            }
        }

        private class BubbleLifeTimeEntry : LifetimeEntry
        {
            public Vector2 InitialSize { get; set; }

            public float MaxSize { get; set; }

            public Vector2 Position { get; set; }

            public Colour4 Colour { get; set; }

            // FadeTime is based on the approach rate of the beatmap.
            public double FadeTime { get; set; }

            // Whether the corresponding HitObject was hit
            public bool IsHit { get; set; }
        }

        #endregion
    }
}
