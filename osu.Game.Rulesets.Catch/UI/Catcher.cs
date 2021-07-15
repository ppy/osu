// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Pooling;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Rulesets.Catch.Judgements;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.Objects.Drawables;
using osu.Game.Rulesets.Catch.Skinning;
using osu.Game.Rulesets.Judgements;
using osu.Game.Skinning;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Catch.UI
{
    public class Catcher : SkinReloadableDrawable
    {
        /// <summary>
        /// The default colour used to tint hyper-dash fruit, along with the moving catcher, its trail
        /// and end glow/after-image during a hyper-dash.
        /// </summary>
        public static readonly Color4 DEFAULT_HYPER_DASH_COLOUR = Color4.Red;

        /// <summary>
        /// The duration between transitioning to hyper-dash state.
        /// </summary>
        public const double HYPER_DASH_TRANSITION_DURATION = 180;

        /// <summary>
        /// Whether we are hyper-dashing or not.
        /// </summary>
        public bool HyperDashing => hyperDashModifier != 1;

        /// <summary>
        /// Whether <see cref="DrawablePalpableCatchHitObject"/> fruit should appear on the plate.
        /// </summary>
        public bool CatchFruitOnPlate { get; set; } = true;

        /// <summary>
        /// The relative space to cover in 1 millisecond. based on 1 game pixel per millisecond as in osu-stable.
        /// </summary>
        public const double BASE_SPEED = 1.0;

        /// <summary>
        /// The current speed of the catcher.
        /// </summary>
        public double Speed => (Dashing ? 1 : 0.5) * BASE_SPEED * hyperDashModifier;

        /// <summary>
        /// The amount by which caught fruit should be scaled down to fit on the plate.
        /// </summary>
        private const float caught_fruit_scale_adjust = 0.5f;

        [NotNull]
        private readonly Container trailsTarget;

        private CatcherTrailDisplay trails;

        /// <summary>
        /// Contains caught objects on the plate.
        /// </summary>
        private readonly Container<CaughtObject> caughtObjectContainer;

        /// <summary>
        /// Contains objects dropped from the plate.
        /// </summary>
        [Resolved]
        private DroppedObjectContainer droppedObjectTarget { get; set; }

        public CatcherAnimationState CurrentState
        {
            get => Body.AnimationState.Value;
            private set => Body.AnimationState.Value = value;
        }

        /// <summary>
        /// The width of the catcher which can receive fruit. Equivalent to "catchMargin" in osu-stable.
        /// </summary>
        public const float ALLOWED_CATCH_RANGE = 0.8f;

        private bool dashing;

        public bool Dashing
        {
            get => dashing;
            set
            {
                if (value == dashing) return;

                dashing = value;

                updateTrailVisibility();
            }
        }

        /// <summary>
        /// The currently facing direction.
        /// </summary>
        public Direction VisualDirection { get; set; } = Direction.Right;

        /// <summary>
        /// Whether the contents of the catcher plate should be visually flipped when the catcher direction is changed.
        /// </summary>
        private bool flipCatcherPlate;

        /// <summary>
        /// Width of the area that can be used to attempt catches during gameplay.
        /// </summary>
        private readonly float catchWidth;

        internal readonly SkinnableCatcher Body;

        private Color4 hyperDashColour = DEFAULT_HYPER_DASH_COLOUR;
        private Color4 hyperDashEndGlowColour = DEFAULT_HYPER_DASH_COLOUR;

        private double hyperDashModifier = 1;
        private int hyperDashDirection;
        private float hyperDashTargetPosition;
        private Bindable<bool> hitLighting;

        private readonly HitExplosionContainer hitExplosionContainer;

        private readonly DrawablePool<CaughtFruit> caughtFruitPool;
        private readonly DrawablePool<CaughtBanana> caughtBananaPool;
        private readonly DrawablePool<CaughtDroplet> caughtDropletPool;

        public Catcher([NotNull] Container trailsTarget, BeatmapDifficulty difficulty = null)
        {
            this.trailsTarget = trailsTarget;

            Origin = Anchor.TopCentre;

            Size = new Vector2(CatcherArea.CATCHER_SIZE);
            if (difficulty != null)
                Scale = calculateScale(difficulty);

            catchWidth = CalculateCatchWidth(Scale);

            InternalChildren = new Drawable[]
            {
                caughtFruitPool = new DrawablePool<CaughtFruit>(50),
                caughtBananaPool = new DrawablePool<CaughtBanana>(100),
                // less capacity is needed compared to fruit because droplet is not stacked
                caughtDropletPool = new DrawablePool<CaughtDroplet>(25),
                caughtObjectContainer = new Container<CaughtObject>
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.BottomCentre,
                    // offset fruit vertically to better place "above" the plate.
                    Y = -5
                },
                Body = new SkinnableCatcher(),
                hitExplosionContainer = new HitExplosionContainer
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.BottomCentre,
                },
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            hitLighting = config.GetBindable<bool>(OsuSetting.HitLighting);
            trails = new CatcherTrailDisplay(this);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            // don't add in above load as we may potentially modify a parent in an unsafe manner.
            trailsTarget.Add(trails);
        }

        /// <summary>
        /// Creates proxied content to be displayed beneath hitobjects.
        /// </summary>
        public Drawable CreateProxiedContent() => caughtObjectContainer.CreateProxy();

        /// <summary>
        /// Calculates the scale of the catcher based off the provided beatmap difficulty.
        /// </summary>
        private static Vector2 calculateScale(BeatmapDifficulty difficulty) => new Vector2(1.0f - 0.7f * (difficulty.CircleSize - 5) / 5);

        /// <summary>
        /// Calculates the width of the area used for attempting catches in gameplay.
        /// </summary>
        /// <param name="scale">The scale of the catcher.</param>
        public static float CalculateCatchWidth(Vector2 scale) => CatcherArea.CATCHER_SIZE * Math.Abs(scale.X) * ALLOWED_CATCH_RANGE;

        /// <summary>
        /// Calculates the width of the area used for attempting catches in gameplay.
        /// </summary>
        /// <param name="difficulty">The beatmap difficulty.</param>
        public static float CalculateCatchWidth(BeatmapDifficulty difficulty) => CalculateCatchWidth(calculateScale(difficulty));

        /// <summary>
        /// Determine if this catcher can catch a <see cref="CatchHitObject"/> in the current position.
        /// </summary>
        public bool CanCatch(CatchHitObject hitObject)
        {
            if (!(hitObject is PalpableCatchHitObject fruit))
                return false;

            var halfCatchWidth = catchWidth * 0.5f;

            // this stuff wil disappear once we move fruit to non-relative coordinate space in the future.
            var catchObjectPosition = fruit.EffectiveX;
            var catcherPosition = Position.X;

            return catchObjectPosition >= catcherPosition - halfCatchWidth &&
                   catchObjectPosition <= catcherPosition + halfCatchWidth;
        }

        public void OnNewResult(DrawableCatchHitObject drawableObject, JudgementResult result)
        {
            var catchResult = (CatchJudgementResult)result;
            catchResult.CatcherAnimationState = CurrentState;
            catchResult.CatcherHyperDash = HyperDashing;

            if (!(drawableObject is DrawablePalpableCatchHitObject palpableObject)) return;

            var hitObject = palpableObject.HitObject;

            if (result.IsHit)
            {
                var positionInStack = computePositionInStack(new Vector2(palpableObject.X - X, 0), palpableObject.DisplaySize.X);

                if (CatchFruitOnPlate)
                    placeCaughtObject(palpableObject, positionInStack);

                if (hitLighting.Value)
                    addLighting(hitObject, positionInStack.X, drawableObject.AccentColour.Value);
            }

            // droplet doesn't affect the catcher state
            if (hitObject is TinyDroplet) return;

            if (result.IsHit && hitObject.HyperDash)
            {
                var target = hitObject.HyperDashTarget;
                var timeDifference = target.StartTime - hitObject.StartTime;
                double positionDifference = target.EffectiveX - X;
                var velocity = positionDifference / Math.Max(1.0, timeDifference - 1000.0 / 60.0);

                SetHyperDashState(Math.Abs(velocity), target.EffectiveX);
            }
            else
                SetHyperDashState();

            if (result.IsHit)
                CurrentState = hitObject.Kiai ? CatcherAnimationState.Kiai : CatcherAnimationState.Idle;
            else if (!(hitObject is Banana))
                CurrentState = CatcherAnimationState.Fail;
        }

        public void OnRevertResult(DrawableCatchHitObject drawableObject, JudgementResult result)
        {
            var catchResult = (CatchJudgementResult)result;

            CurrentState = catchResult.CatcherAnimationState;

            if (HyperDashing != catchResult.CatcherHyperDash)
            {
                if (catchResult.CatcherHyperDash)
                    SetHyperDashState(2);
                else
                    SetHyperDashState();
            }

            caughtObjectContainer.RemoveAll(d => d.HitObject == drawableObject.HitObject);
            droppedObjectTarget.RemoveAll(d => d.HitObject == drawableObject.HitObject);
        }

        /// <summary>
        /// Set hyper-dash state.
        /// </summary>
        /// <param name="modifier">The speed multiplier. If this is less or equals to 1, this catcher will be non-hyper-dashing state.</param>
        /// <param name="targetPosition">When this catcher crosses this position, this catcher ends hyper-dashing.</param>
        public void SetHyperDashState(double modifier = 1, float targetPosition = -1)
        {
            var wasHyperDashing = HyperDashing;

            if (modifier <= 1 || X == targetPosition)
            {
                hyperDashModifier = 1;
                hyperDashDirection = 0;

                if (wasHyperDashing)
                    runHyperDashStateTransition(false);
            }
            else
            {
                hyperDashModifier = modifier;
                hyperDashDirection = Math.Sign(targetPosition - X);
                hyperDashTargetPosition = targetPosition;

                if (!wasHyperDashing)
                {
                    trails.DisplayEndGlow();
                    runHyperDashStateTransition(true);
                }
            }
        }

        /// <summary>
        /// Drop any fruit off the plate.
        /// </summary>
        public void Drop() => clearPlate(DroppedObjectAnimation.Drop);

        /// <summary>
        /// Explode all fruit off the plate.
        /// </summary>
        public void Explode() => clearPlate(DroppedObjectAnimation.Explode);

        private void runHyperDashStateTransition(bool hyperDashing)
        {
            updateTrailVisibility();

            this.FadeColour(hyperDashing ? hyperDashColour : Color4.White, HYPER_DASH_TRANSITION_DURATION, Easing.OutQuint);
        }

        private void updateTrailVisibility() => trails.DisplayTrail = Dashing || HyperDashing;

        protected override void SkinChanged(ISkinSource skin)
        {
            base.SkinChanged(skin);

            hyperDashColour =
                skin.GetConfig<CatchSkinColour, Color4>(CatchSkinColour.HyperDash)?.Value ??
                DEFAULT_HYPER_DASH_COLOUR;

            hyperDashEndGlowColour =
                skin.GetConfig<CatchSkinColour, Color4>(CatchSkinColour.HyperDashAfterImage)?.Value ??
                hyperDashColour;

            trails.HyperDashTrailsColour = hyperDashColour;
            trails.EndGlowSpritesColour = hyperDashEndGlowColour;

            flipCatcherPlate = skin.GetConfig<CatchSkinConfiguration, bool>(CatchSkinConfiguration.FlipCatcherPlate)?.Value ?? true;

            runHyperDashStateTransition(HyperDashing);
        }

        protected override void Update()
        {
            base.Update();

            var scaleFromDirection = new Vector2((int)VisualDirection, 1);
            Body.Scale = scaleFromDirection;
            caughtObjectContainer.Scale = hitExplosionContainer.Scale = flipCatcherPlate ? scaleFromDirection : Vector2.One;

            // Correct overshooting.
            if ((hyperDashDirection > 0 && hyperDashTargetPosition < X) ||
                (hyperDashDirection < 0 && hyperDashTargetPosition > X))
            {
                X = hyperDashTargetPosition;
                SetHyperDashState();
            }
        }

        private void placeCaughtObject(DrawablePalpableCatchHitObject drawableObject, Vector2 position)
        {
            var caughtObject = getCaughtObject(drawableObject.HitObject);

            if (caughtObject == null) return;

            caughtObject.CopyStateFrom(drawableObject);
            caughtObject.Anchor = Anchor.TopCentre;
            caughtObject.Position = position;
            caughtObject.Scale *= caught_fruit_scale_adjust;

            caughtObjectContainer.Add(caughtObject);

            if (!caughtObject.StaysOnPlate)
                removeFromPlate(caughtObject, DroppedObjectAnimation.Explode);
        }

        private Vector2 computePositionInStack(Vector2 position, float displayRadius)
        {
            // this is taken from osu-stable (lenience should be 10 * 10 at standard scale).
            const float lenience_adjust = 10 / CatchHitObject.OBJECT_RADIUS;

            float adjustedRadius = displayRadius * lenience_adjust;
            float checkDistance = MathF.Pow(adjustedRadius, 2);

            while (caughtObjectContainer.Any(f => Vector2Extensions.DistanceSquared(f.Position, position) < checkDistance))
            {
                position.X += RNG.NextSingle(-adjustedRadius, adjustedRadius);
                position.Y -= RNG.NextSingle(0, 5);
            }

            return position;
        }

        private void addLighting(CatchHitObject hitObject, float x, Color4 colour) =>
            hitExplosionContainer.Add(new HitExplosionEntry(Time.Current, x, hitObject.Scale, colour, hitObject.RandomSeed));

        private CaughtObject getCaughtObject(PalpableCatchHitObject source)
        {
            switch (source)
            {
                case Fruit _:
                    return caughtFruitPool.Get();

                case Banana _:
                    return caughtBananaPool.Get();

                case Droplet _:
                    return caughtDropletPool.Get();

                default:
                    return null;
            }
        }

        private CaughtObject getDroppedObject(CaughtObject caughtObject)
        {
            var droppedObject = getCaughtObject(caughtObject.HitObject);

            droppedObject.CopyStateFrom(caughtObject);
            droppedObject.Anchor = Anchor.TopLeft;
            droppedObject.Position = caughtObjectContainer.ToSpaceOfOtherDrawable(caughtObject.DrawPosition, droppedObjectTarget);

            return droppedObject;
        }

        private void clearPlate(DroppedObjectAnimation animation)
        {
            var droppedObjects = caughtObjectContainer.Children.Select(getDroppedObject).ToArray();

            caughtObjectContainer.Clear(false);

            droppedObjectTarget.AddRange(droppedObjects);

            foreach (var droppedObject in droppedObjects)
                applyDropAnimation(droppedObject, animation);
        }

        private void removeFromPlate(CaughtObject caughtObject, DroppedObjectAnimation animation)
        {
            var droppedObject = getDroppedObject(caughtObject);

            caughtObjectContainer.Remove(caughtObject);

            droppedObjectTarget.Add(droppedObject);

            applyDropAnimation(droppedObject, animation);
        }

        private void applyDropAnimation(Drawable d, DroppedObjectAnimation animation)
        {
            switch (animation)
            {
                case DroppedObjectAnimation.Drop:
                    d.MoveToY(d.Y + 75, 750, Easing.InSine);
                    d.FadeOut(750);
                    break;

                case DroppedObjectAnimation.Explode:
                    float originalX = droppedObjectTarget.ToSpaceOfOtherDrawable(d.DrawPosition, caughtObjectContainer).X * caughtObjectContainer.Scale.X;
                    d.MoveToY(d.Y - 50, 250, Easing.OutSine).Then().MoveToY(d.Y + 50, 500, Easing.InSine);
                    d.MoveToX(d.X + originalX * 6, 1000);
                    d.FadeOut(750);
                    break;
            }

            d.Expire();
        }

        private enum DroppedObjectAnimation
        {
            Drop,
            Explode
        }
    }
}
