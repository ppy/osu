// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using System.Linq;
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
using osu.Game.Rulesets.Objects.Legacy;
using osu.Game.Skinning;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Catch.UI
{
    [Cached]
    public partial class Catcher : SkinReloadableDrawable
    {
        /// <summary>
        /// The size of the catcher at 1x scale.
        /// </summary>
        /// <remarks>
        /// This is mainly used to compute catching range, the actual catcher size may differ based on skin implementation and sprite textures.
        /// This is also equivalent to the "catcherWidth" property in osu-stable when the game field and beatmap difficulty are set to default values.
        /// </remarks>
        /// <seealso cref="CatchPlayfield.WIDTH"/>
        /// <seealso cref="CatchPlayfield.HEIGHT"/>
        /// <seealso cref="IBeatmapDifficultyInfo.DEFAULT_DIFFICULTY"/>
        public const float BASE_SIZE = 106.75f;

        /// <summary>
        /// The width of the catcher which can receive fruit. Equivalent to "catchMargin" in osu-stable.
        /// </summary>
        public const float ALLOWED_CATCH_RANGE = 0.8f;

        /// <summary>
        /// The default colour used to tint hyper-dash fruit, along with the moving catcher, its trail and after-image during a hyper-dash.
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
        /// The speed of the catcher when the catcher is dashing.
        /// </summary>
        public const double BASE_DASH_SPEED = 1.0;

        /// <summary>
        /// The speed of the catcher when the catcher is not dashing.
        /// </summary>
        public const double BASE_WALK_SPEED = 0.5;

        /// <summary>
        /// The current speed of the catcher with the hyper-dash modifier applied.
        /// </summary>
        public double Speed => (Dashing ? BASE_DASH_SPEED : BASE_WALK_SPEED) * hyperDashModifier;

        /// <summary>
        /// The amount by which caught fruit should be scaled down to fit on the plate.
        /// </summary>
        private const float caught_fruit_scale_adjust = 0.5f;

        /// <summary>
        /// Contains caught objects on the plate.
        /// </summary>
        private readonly Container<CaughtObject> caughtObjectContainer;

        /// <summary>
        /// Contains objects dropped from the plate.
        /// </summary>
        private readonly DroppedObjectContainer droppedObjectTarget;

        public CatcherAnimationState CurrentState
        {
            get => body.AnimationState.Value;
            private set => body.AnimationState.Value = value;
        }

        /// <summary>
        /// Whether the catcher is currently dashing.
        /// </summary>
        public bool Dashing { get; set; }

        /// <summary>
        /// The currently facing direction.
        /// </summary>
        public Direction VisualDirection { get; set; } = Direction.Right;

        public Vector2 BodyScale => Scale * body.Scale;

        /// <summary>
        /// Whether the contents of the catcher plate should be visually flipped when the catcher direction is changed.
        /// </summary>
        private bool flipCatcherPlate;

        /// <summary>
        /// Width of the area that can be used to attempt catches during gameplay.
        /// </summary>
        public readonly float CatchWidth;

        private readonly SkinnableCatcher body;

        private Color4 hyperDashColour = DEFAULT_HYPER_DASH_COLOUR;

        private double? lastHyperDashStartTime;
        private double hyperDashModifier = 1;
        private int hyperDashDirection;
        private float hyperDashTargetPosition;
        private Bindable<bool> hitLighting = null!;

        private readonly HitExplosionContainer hitExplosionContainer;

        private readonly DrawablePool<CaughtFruit> caughtFruitPool;
        private readonly DrawablePool<CaughtBanana> caughtBananaPool;
        private readonly DrawablePool<CaughtDroplet> caughtDropletPool;

        public Catcher(DroppedObjectContainer droppedObjectTarget, IBeatmapDifficultyInfo? difficulty = null)
        {
            this.droppedObjectTarget = droppedObjectTarget;

            Origin = Anchor.TopCentre;

            Size = new Vector2(BASE_SIZE);

            if (difficulty != null)
                Scale = calculateScale(difficulty);

            CatchWidth = CalculateCatchWidth(Scale);

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
                body = new SkinnableCatcher(),
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
        }

        /// <summary>
        /// Creates proxied content to be displayed beneath hitobjects.
        /// </summary>
        public Drawable CreateProxiedContent() => caughtObjectContainer.CreateProxy();

        /// <summary>
        /// Calculates the width of the area used for attempting catches in gameplay.
        /// </summary>
        /// <param name="scale">The scale of the catcher.</param>
        public static float CalculateCatchWidth(Vector2 scale) => BASE_SIZE * Math.Abs(scale.X) * ALLOWED_CATCH_RANGE;

        /// <summary>
        /// Calculates the width of the area used for attempting catches in gameplay.
        /// </summary>
        /// <param name="difficulty">The beatmap difficulty.</param>
        public static float CalculateCatchWidth(IBeatmapDifficultyInfo difficulty) => CalculateCatchWidth(calculateScale(difficulty));

        /// <summary>
        /// Determine if this catcher can catch a <see cref="CatchHitObject"/> in the current position.
        /// </summary>
        public bool CanCatch(CatchHitObject hitObject)
        {
            if (!(hitObject is PalpableCatchHitObject fruit))
                return false;

            float halfCatchWidth = CatchWidth * 0.5f;
            return fruit.EffectiveX >= X - halfCatchWidth &&
                   fruit.EffectiveX <= X + halfCatchWidth;
        }

        public void OnNewResult(DrawableCatchHitObject drawableObject, Judgement result)
        {
            var catchResult = (CatchJudgement)result;
            catchResult.CatcherAnimationState = CurrentState;
            catchResult.CatcherHyperDash = HyperDashing;

            // Ignore JuiceStreams and BananaShowers
            if (!(drawableObject is DrawablePalpableCatchHitObject palpableObject)) return;

            var hitObject = palpableObject.HitObject;

            if (result.IsHit)
            {
                var positionInStack = computePositionInStack(new Vector2(palpableObject.X - X, 0), palpableObject.DisplaySize.X);

                if (CatchFruitOnPlate)
                    placeCaughtObject(palpableObject, positionInStack);

                if (hitLighting.Value)
                    addLighting(result, drawableObject.AccentColour.Value, positionInStack.X);
            }

            // droplet doesn't affect the catcher state
            if (hitObject is TinyDroplet) return;

            // if a hyper fruit was already handled this frame, just go where it says to go.
            // this special-cases some aspire maps that have doubled-up objects (one hyper, one not) at the same time instant.
            // handling this "properly" elsewhere is impossible as there is no feasible way to ensure
            // that the hyperfruit gets judged second (especially if it coincides with a last fruit in a juice stream).
            if (lastHyperDashStartTime != Time.Current)
            {
                if (result.IsHit && hitObject.HyperDashTarget is CatchHitObject target)
                {
                    double timeDifference = target.StartTime - hitObject.StartTime;
                    double positionDifference = target.EffectiveX - X;
                    double velocity = positionDifference / Math.Max(1.0, timeDifference - 1000.0 / 60.0);

                    SetHyperDashState(Math.Abs(velocity) / BASE_DASH_SPEED, target.EffectiveX);
                }
                else
                    SetHyperDashState();
            }

            if (result.IsHit)
                CurrentState = hitObject.Kiai ? CatcherAnimationState.Kiai : CatcherAnimationState.Idle;
            else if (!(hitObject is Banana))
                CurrentState = CatcherAnimationState.Fail;

            if (palpableObject.HitObject.LastInCombo)
            {
                if (result.JudgementInfo is CatchJudgementInfo catchJudgement && catchJudgement.ShouldExplodeFor(result))
                    Explode();
                else
                    Drop();
            }
        }

        public void OnRevertResult(Judgement result)
        {
            var catchResult = (CatchJudgement)result;

            CurrentState = catchResult.CatcherAnimationState;

            if (HyperDashing != catchResult.CatcherHyperDash)
            {
                if (catchResult.CatcherHyperDash)
                    SetHyperDashState(2);
                else
                    SetHyperDashState();
            }

            caughtObjectContainer.RemoveAll(d => d.HitObject == result.HitObject, false);
            droppedObjectTarget.RemoveAll(d => d.HitObject == result.HitObject, false);
        }

        /// <summary>
        /// Set hyper-dash state.
        /// </summary>
        /// <param name="modifier">The speed multiplier. If this is less or equals to 1, this catcher will be non-hyper-dashing state.</param>
        /// <param name="targetPosition">When this catcher crosses this position, this catcher ends hyper-dashing.</param>
        public void SetHyperDashState(double modifier = 1, float targetPosition = -1)
        {
            bool wasHyperDashing = HyperDashing;

            if (modifier <= 1 || X == targetPosition)
            {
                hyperDashModifier = 1;
                hyperDashDirection = 0;

                if (wasHyperDashing)
                    runHyperDashStateTransition(false);

                lastHyperDashStartTime = null;
            }
            else
            {
                hyperDashModifier = modifier;
                hyperDashDirection = Math.Sign(targetPosition - X);
                hyperDashTargetPosition = targetPosition;

                if (!wasHyperDashing)
                    runHyperDashStateTransition(true);

                lastHyperDashStartTime = Time.Current;
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
            this.FadeColour(hyperDashing ? hyperDashColour : Color4.White, HYPER_DASH_TRANSITION_DURATION, Easing.OutQuint);
        }

        protected override void SkinChanged(ISkinSource skin)
        {
            base.SkinChanged(skin);

            hyperDashColour =
                skin.GetConfig<CatchSkinColour, Color4>(CatchSkinColour.HyperDash)?.Value ??
                DEFAULT_HYPER_DASH_COLOUR;

            flipCatcherPlate = skin.GetConfig<CatchSkinConfiguration, bool>(CatchSkinConfiguration.FlipCatcherPlate)?.Value ?? true;

            runHyperDashStateTransition(HyperDashing);
        }

        protected override void Update()
        {
            base.Update();

            var scaleFromDirection = new Vector2((int)VisualDirection, 1);

            body.Scale = scaleFromDirection;
            // Inverse of catcher scale is applied here, as catcher gets scaled by circle size and so do the incoming fruit.
            caughtObjectContainer.Scale = (1 / Scale.X) * (flipCatcherPlate ? scaleFromDirection : Vector2.One);
            hitExplosionContainer.Scale = flipCatcherPlate ? scaleFromDirection : Vector2.One;

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

        private void addLighting(Judgement judgement, Color4 colour, float x) =>
            hitExplosionContainer.Add(new HitExplosionEntry(Time.Current, judgement, colour, x));

        private CaughtObject? getCaughtObject(PalpableCatchHitObject source)
        {
            switch (source)
            {
                case Fruit:
                    return caughtFruitPool.Get();

                case Banana:
                    return caughtBananaPool.Get();

                case Droplet:
                    return caughtDropletPool.Get();

                default:
                    return null;
            }
        }

        private CaughtObject getDroppedObject(CaughtObject caughtObject)
        {
            var droppedObject = getCaughtObject(caughtObject.HitObject);
            Debug.Assert(droppedObject != null);

            droppedObject.CopyStateFrom(caughtObject);
            droppedObject.Anchor = Anchor.TopLeft;
            droppedObject.Position = caughtObjectContainer.ToSpaceOfOtherDrawable(caughtObject.DrawPosition, droppedObjectTarget);

            return droppedObject;
        }

        private void clearPlate(DroppedObjectAnimation animation)
        {
            var caughtObjects = caughtObjectContainer.Children.ToArray();

            caughtObjectContainer.Clear(false);

            // Use the already returned PoolableDrawables for new objects
            var droppedObjects = caughtObjects.Select(getDroppedObject).ToArray();

            droppedObjectTarget.AddRange(droppedObjects);

            foreach (var droppedObject in droppedObjects)
                applyDropAnimation(droppedObject, animation);
        }

        private void removeFromPlate(CaughtObject caughtObject, DroppedObjectAnimation animation)
        {
            caughtObjectContainer.Remove(caughtObject, false);

            var droppedObject = getDroppedObject(caughtObject);

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

            // Define lifetime start for dropped objects to be disposed correctly when rewinding replay
            d.LifetimeStart = Clock.CurrentTime;
            d.Expire();
        }

        /// <summary>
        /// Calculates the scale of the catcher based off the provided beatmap difficulty.
        /// </summary>
        private static Vector2 calculateScale(IBeatmapDifficultyInfo difficulty) => new Vector2(LegacyRulesetExtensions.CalculateScaleFromCircleSize(difficulty.CircleSize) * 2);

        private enum DroppedObjectAnimation
        {
            Drop,
            Explode
        }
    }
}
