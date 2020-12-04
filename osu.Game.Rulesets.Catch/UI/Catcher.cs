// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Animations;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Pooling;
using osu.Framework.Input.Bindings;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.Objects.Drawables;
using osu.Game.Rulesets.Catch.Skinning;
using osu.Game.Skinning;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Catch.UI
{
    public class Catcher : SkinReloadableDrawable, IKeyBindingHandler<CatchAction>
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
        /// The relative space to cover in 1 millisecond. based on 1 game pixel per millisecond as in osu-stable.
        /// </summary>
        public const double BASE_SPEED = 1.0;

        [NotNull]
        private readonly Container trailsTarget;

        private CatcherTrailDisplay trails;

        private readonly Container droppedObjectTarget;

        private readonly Container<DrawablePalpableCatchHitObject> caughtFruitContainer;

        public CatcherAnimationState CurrentState { get; private set; }

        /// <summary>
        /// The width of the catcher which can receive fruit. Equivalent to "catchMargin" in osu-stable.
        /// </summary>
        public const float ALLOWED_CATCH_RANGE = 0.8f;

        /// <summary>
        /// The drawable catcher for <see cref="CurrentState"/>.
        /// </summary>
        internal Drawable CurrentDrawableCatcher => currentCatcher.Drawable;

        private bool dashing;

        public bool Dashing
        {
            get => dashing;
            protected set
            {
                if (value == dashing) return;

                dashing = value;

                updateTrailVisibility();
            }
        }

        /// <summary>
        /// Width of the area that can be used to attempt catches during gameplay.
        /// </summary>
        private readonly float catchWidth;

        private readonly CatcherSprite catcherIdle;
        private readonly CatcherSprite catcherKiai;
        private readonly CatcherSprite catcherFail;

        private CatcherSprite currentCatcher;

        private Color4 hyperDashColour = DEFAULT_HYPER_DASH_COLOUR;
        private Color4 hyperDashEndGlowColour = DEFAULT_HYPER_DASH_COLOUR;

        private int currentDirection;

        private double hyperDashModifier = 1;
        private int hyperDashDirection;
        private float hyperDashTargetPosition;
        private Bindable<bool> hitLighting;

        private readonly DrawablePool<HitExplosion> hitExplosionPool;
        private readonly Container<HitExplosion> hitExplosionContainer;

        public Catcher([NotNull] Container trailsTarget, [NotNull] Container droppedObjectTarget, BeatmapDifficulty difficulty = null)
        {
            this.trailsTarget = trailsTarget;
            this.droppedObjectTarget = droppedObjectTarget;

            Origin = Anchor.TopCentre;

            Size = new Vector2(CatcherArea.CATCHER_SIZE);
            if (difficulty != null)
                Scale = calculateScale(difficulty);

            catchWidth = CalculateCatchWidth(Scale);

            InternalChildren = new Drawable[]
            {
                hitExplosionPool = new DrawablePool<HitExplosion>(10),
                caughtFruitContainer = new Container<DrawablePalpableCatchHitObject>
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.BottomCentre,
                },
                catcherIdle = new CatcherSprite(CatcherAnimationState.Idle)
                {
                    Anchor = Anchor.TopCentre,
                    Alpha = 0,
                },
                catcherKiai = new CatcherSprite(CatcherAnimationState.Kiai)
                {
                    Anchor = Anchor.TopCentre,
                    Alpha = 0,
                },
                catcherFail = new CatcherSprite(CatcherAnimationState.Fail)
                {
                    Anchor = Anchor.TopCentre,
                    Alpha = 0,
                },
                hitExplosionContainer = new Container<HitExplosion>
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

            updateCatcher();
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
        public Drawable CreateProxiedContent() => caughtFruitContainer.CreateProxy();

        /// <summary>
        /// Calculates the scale of the catcher based off the provided beatmap difficulty.
        /// </summary>
        private static Vector2 calculateScale(BeatmapDifficulty difficulty) => new Vector2(1.0f - 0.7f * (difficulty.CircleSize - 5) / 5);

        /// <summary>
        /// Calculates the width of the area used for attempting catches in gameplay.
        /// </summary>
        /// <param name="scale">The scale of the catcher.</param>
        internal static float CalculateCatchWidth(Vector2 scale) => CatcherArea.CATCHER_SIZE * Math.Abs(scale.X) * ALLOWED_CATCH_RANGE;

        /// <summary>
        /// Calculates the width of the area used for attempting catches in gameplay.
        /// </summary>
        /// <param name="difficulty">The beatmap difficulty.</param>
        internal static float CalculateCatchWidth(BeatmapDifficulty difficulty) => CalculateCatchWidth(calculateScale(difficulty));

        /// <summary>
        /// Let the catcher attempt to catch a fruit.
        /// </summary>
        /// <param name="hitObject">The fruit to catch.</param>
        /// <returns>Whether the catch is possible.</returns>
        public bool AttemptCatch(CatchHitObject hitObject)
        {
            if (!(hitObject is PalpableCatchHitObject fruit))
                return false;

            var halfCatchWidth = catchWidth * 0.5f;

            // this stuff wil disappear once we move fruit to non-relative coordinate space in the future.
            var catchObjectPosition = fruit.X;
            var catcherPosition = Position.X;

            var validCatch =
                catchObjectPosition >= catcherPosition - halfCatchWidth &&
                catchObjectPosition <= catcherPosition + halfCatchWidth;

            if (validCatch)
                placeCaughtObject(fruit);

            // droplet doesn't affect the catcher state
            if (fruit is TinyDroplet) return validCatch;

            if (validCatch && fruit.HyperDash)
            {
                var target = fruit.HyperDashTarget;
                var timeDifference = target.StartTime - fruit.StartTime;
                double positionDifference = target.X - catcherPosition;
                var velocity = positionDifference / Math.Max(1.0, timeDifference - 1000.0 / 60.0);

                SetHyperDashState(Math.Abs(velocity), target.X);
            }
            else
                SetHyperDashState();

            if (validCatch)
                updateState(fruit.Kiai ? CatcherAnimationState.Kiai : CatcherAnimationState.Idle);
            else if (!(fruit is Banana))
                updateState(CatcherAnimationState.Fail);

            return validCatch;
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

        public void UpdatePosition(float position)
        {
            position = Math.Clamp(position, 0, CatchPlayfield.WIDTH);

            if (position == X)
                return;

            Scale = new Vector2(Math.Abs(Scale.X) * (position > X ? 1 : -1), Scale.Y);
            X = position;
        }

        public bool OnPressed(CatchAction action)
        {
            switch (action)
            {
                case CatchAction.MoveLeft:
                    currentDirection--;
                    return true;

                case CatchAction.MoveRight:
                    currentDirection++;
                    return true;

                case CatchAction.Dash:
                    Dashing = true;
                    return true;
            }

            return false;
        }

        public void OnReleased(CatchAction action)
        {
            switch (action)
            {
                case CatchAction.MoveLeft:
                    currentDirection++;
                    break;

                case CatchAction.MoveRight:
                    currentDirection--;
                    break;

                case CatchAction.Dash:
                    Dashing = false;
                    break;
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

            if (hyperDashing)
            {
                this.FadeColour(hyperDashColour, HYPER_DASH_TRANSITION_DURATION, Easing.OutQuint);
                this.FadeTo(0.2f, HYPER_DASH_TRANSITION_DURATION, Easing.OutQuint);
            }
            else
            {
                this.FadeColour(Color4.White, HYPER_DASH_TRANSITION_DURATION, Easing.OutQuint);
                this.FadeTo(1f, HYPER_DASH_TRANSITION_DURATION, Easing.OutQuint);
            }
        }

        private void updateTrailVisibility() => trails.DisplayTrail = Dashing || HyperDashing;

        protected override void SkinChanged(ISkinSource skin, bool allowFallback)
        {
            base.SkinChanged(skin, allowFallback);

            hyperDashColour =
                skin.GetConfig<CatchSkinColour, Color4>(CatchSkinColour.HyperDash)?.Value ??
                DEFAULT_HYPER_DASH_COLOUR;

            hyperDashEndGlowColour =
                skin.GetConfig<CatchSkinColour, Color4>(CatchSkinColour.HyperDashAfterImage)?.Value ??
                hyperDashColour;

            trails.HyperDashTrailsColour = hyperDashColour;
            trails.EndGlowSpritesColour = hyperDashEndGlowColour;

            runHyperDashStateTransition(HyperDashing);
        }

        protected override void Update()
        {
            base.Update();

            if (currentDirection == 0) return;

            var direction = Math.Sign(currentDirection);

            var dashModifier = Dashing ? 1 : 0.5;
            var speed = BASE_SPEED * dashModifier * hyperDashModifier;

            UpdatePosition((float)(X + direction * Clock.ElapsedFrameTime * speed));

            // Correct overshooting.
            if ((hyperDashDirection > 0 && hyperDashTargetPosition < X) ||
                (hyperDashDirection < 0 && hyperDashTargetPosition > X))
            {
                X = hyperDashTargetPosition;
                SetHyperDashState();
            }
        }

        private void updateCatcher()
        {
            currentCatcher?.Hide();

            switch (CurrentState)
            {
                default:
                    currentCatcher = catcherIdle;
                    break;

                case CatcherAnimationState.Fail:
                    currentCatcher = catcherFail;
                    break;

                case CatcherAnimationState.Kiai:
                    currentCatcher = catcherKiai;
                    break;
            }

            currentCatcher.Show();
            (currentCatcher.Drawable as IFramedAnimation)?.GotoFrame(0);
        }

        private void updateState(CatcherAnimationState state)
        {
            if (CurrentState == state)
                return;

            CurrentState = state;
            updateCatcher();
        }

        private void placeCaughtObject(PalpableCatchHitObject source)
        {
            var caughtObject = createCaughtObject(source);

            if (caughtObject == null) return;

            caughtObject.RelativePositionAxes = Axes.None;
            caughtObject.X = source.X - X;
            caughtObject.IsOnPlate = true;

            caughtObject.Anchor = Anchor.TopCentre;
            caughtObject.Origin = Anchor.Centre;
            caughtObject.Scale *= 0.5f;
            caughtObject.LifetimeStart = source.StartTime;
            caughtObject.LifetimeEnd = double.MaxValue;

            adjustPositionInStack(caughtObject);

            caughtFruitContainer.Add(caughtObject);

            addLighting(caughtObject);

            if (!caughtObject.StaysOnPlate)
                removeFromPlate(caughtObject, DroppedObjectAnimation.Explode);
        }

        private void adjustPositionInStack(DrawablePalpableCatchHitObject caughtObject)
        {
            const float radius_div_2 = CatchHitObject.OBJECT_RADIUS / 2;
            const float allowance = 10;

            float caughtObjectRadius = caughtObject.DisplayRadius;

            while (caughtFruitContainer.Any(f => Vector2Extensions.Distance(f.Position, caughtObject.Position) < (caughtObjectRadius + radius_div_2) / (allowance / 2)))
            {
                float diff = (caughtObjectRadius + radius_div_2) / allowance;

                caughtObject.X += (RNG.NextSingle() - 0.5f) * diff * 2;
                caughtObject.Y -= RNG.NextSingle() * diff;
            }

            caughtObject.X = Math.Clamp(caughtObject.X, -CatcherArea.CATCHER_SIZE / 2, CatcherArea.CATCHER_SIZE / 2);
        }

        private void addLighting(DrawablePalpableCatchHitObject caughtObject)
        {
            if (!hitLighting.Value) return;

            HitExplosion hitExplosion = hitExplosionPool.Get();
            hitExplosion.X = caughtObject.X;
            hitExplosion.Scale = new Vector2(caughtObject.HitObject.Scale);
            hitExplosion.ObjectColour = caughtObject.AccentColour.Value;
            hitExplosionContainer.Add(hitExplosion);
        }

        private DrawablePalpableCatchHitObject createCaughtObject(PalpableCatchHitObject source)
        {
            switch (source)
            {
                case Banana banana:
                    return new DrawableBanana(banana);

                case Fruit fruit:
                    return new DrawableFruit(fruit);

                case TinyDroplet tiny:
                    return new DrawableTinyDroplet(tiny);

                case Droplet droplet:
                    return new DrawableDroplet(droplet);

                default:
                    return null;
            }
        }

        private void clearPlate(DroppedObjectAnimation animation)
        {
            var caughtObjects = caughtFruitContainer.Children.ToArray();
            caughtFruitContainer.Clear(false);

            droppedObjectTarget.AddRange(caughtObjects);

            foreach (var caughtObject in caughtObjects)
                drop(caughtObject, animation);
        }

        private void removeFromPlate(DrawablePalpableCatchHitObject caughtObject, DroppedObjectAnimation animation)
        {
            if (!caughtFruitContainer.Remove(caughtObject))
                throw new InvalidOperationException("Can only drop a caught object on the plate");

            droppedObjectTarget.Add(caughtObject);

            drop(caughtObject, animation);
        }

        private void drop(DrawablePalpableCatchHitObject d, DroppedObjectAnimation animation)
        {
            var originalX = d.X * Scale.X;
            var startTime = Clock.CurrentTime;

            d.Anchor = Anchor.TopLeft;
            d.Position = caughtFruitContainer.ToSpaceOfOtherDrawable(d.DrawPosition, droppedObjectTarget);

            // we cannot just apply the transforms because DHO clears transforms when state is updated
            d.ApplyCustomUpdateState += (o, state) => animate(o, animation, originalX, startTime);
            if (d.IsLoaded)
                animate(d, animation, originalX, startTime);
        }

        private void animate(Drawable d, DroppedObjectAnimation animation, float originalX, double startTime)
        {
            using (d.BeginAbsoluteSequence(startTime))
            {
                switch (animation)
                {
                    case DroppedObjectAnimation.Drop:
                        d.MoveToY(d.Y + 75, 750, Easing.InSine);
                        d.FadeOut(750);
                        break;

                    case DroppedObjectAnimation.Explode:
                        d.MoveToY(d.Y - 50, 250, Easing.OutSine).Then().MoveToY(d.Y + 50, 500, Easing.InSine);
                        d.MoveToX(d.X + originalX * 6, 1000);
                        d.FadeOut(750);
                        break;
                }

                d.Expire();
            }
        }

        private enum DroppedObjectAnimation
        {
            Drop,
            Explode
        }
    }
}
