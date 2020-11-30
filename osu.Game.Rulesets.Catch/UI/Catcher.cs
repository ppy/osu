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
using osu.Framework.Input.Bindings;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.Objects.Drawables;
using osu.Game.Rulesets.Catch.Skinning;
using osu.Game.Rulesets.Objects.Drawables;
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

        public Container ExplodingFruitTarget;

        private Container<DrawableHitObject> caughtFruitContainer { get; } = new Container<DrawableHitObject>
        {
            Anchor = Anchor.TopCentre,
            Origin = Anchor.BottomCentre,
        };

        [NotNull]
        private readonly Container trailsTarget;

        private CatcherTrailDisplay trails;

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

        private CatcherSprite catcherIdle;
        private CatcherSprite catcherKiai;
        private CatcherSprite catcherFail;

        private CatcherSprite currentCatcher;

        private Color4 hyperDashColour = DEFAULT_HYPER_DASH_COLOUR;
        private Color4 hyperDashEndGlowColour = DEFAULT_HYPER_DASH_COLOUR;

        private int currentDirection;

        private double hyperDashModifier = 1;
        private int hyperDashDirection;
        private float hyperDashTargetPosition;
        private Bindable<bool> hitLighting;

        public Catcher([NotNull] Container trailsTarget, BeatmapDifficulty difficulty = null)
        {
            this.trailsTarget = trailsTarget;

            Origin = Anchor.TopCentre;

            Size = new Vector2(CatcherArea.CATCHER_SIZE);
            if (difficulty != null)
                Scale = calculateScale(difficulty);

            catchWidth = CalculateCatchWidth(Scale);
        }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            hitLighting = config.GetBindable<bool>(OsuSetting.HitLighting);

            InternalChildren = new Drawable[]
            {
                caughtFruitContainer,
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
                }
            };

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
        private static Vector2 calculateScale(BeatmapDifficulty difficulty)
            => new Vector2(1.0f - 0.7f * (difficulty.CircleSize - 5) / 5);

        /// <summary>
        /// Calculates the width of the area used for attempting catches in gameplay.
        /// </summary>
        /// <param name="scale">The scale of the catcher.</param>
        internal static float CalculateCatchWidth(Vector2 scale)
            => CatcherArea.CATCHER_SIZE * Math.Abs(scale.X) * ALLOWED_CATCH_RANGE;

        /// <summary>
        /// Calculates the width of the area used for attempting catches in gameplay.
        /// </summary>
        /// <param name="difficulty">The beatmap difficulty.</param>
        internal static float CalculateCatchWidth(BeatmapDifficulty difficulty)
            => CalculateCatchWidth(calculateScale(difficulty));

        /// <summary>
        /// Add a caught fruit to the catcher's stack.
        /// </summary>
        /// <param name="fruit">The fruit that was caught.</param>
        public void PlaceOnPlate(DrawableCatchHitObject fruit)
        {
            var ourRadius = fruit.DisplayRadius;
            float theirRadius = 0;

            const float allowance = 10;

            while (caughtFruitContainer.Any(f =>
                f.LifetimeEnd == double.MaxValue &&
                Vector2Extensions.Distance(f.Position, fruit.Position) < (ourRadius + (theirRadius = f.DrawSize.X / 2 * f.Scale.X)) / (allowance / 2)))
            {
                var diff = (ourRadius + theirRadius) / allowance;
                fruit.X += (RNG.NextSingle() - 0.5f) * diff * 2;
                fruit.Y -= RNG.NextSingle() * diff;
            }

            fruit.X = Math.Clamp(fruit.X, -CatcherArea.CATCHER_SIZE / 2, CatcherArea.CATCHER_SIZE / 2);

            caughtFruitContainer.Add(fruit);

            if (hitLighting.Value)
            {
                AddInternal(new HitExplosion(fruit)
                {
                    X = fruit.X,
                    Scale = new Vector2(fruit.HitObject.Scale)
                });
            }
        }

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

            // only update hyperdash state if we are not catching a tiny droplet.
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

        public void UpdatePosition(float position)
        {
            position = Math.Clamp(position, 0, CatchPlayfield.WIDTH);

            if (position == X)
                return;

            Scale = new Vector2(Math.Abs(Scale.X) * (position > X ? 1 : -1), Scale.Y);
            X = position;
        }

        /// <summary>
        /// Drop any fruit off the plate.
        /// </summary>
        public void Drop()
        {
            foreach (var f in caughtFruitContainer.ToArray())
                Drop(f);
        }

        /// <summary>
        /// Explode any fruit off the plate.
        /// </summary>
        public void Explode()
        {
            foreach (var f in caughtFruitContainer.ToArray())
                Explode(f);
        }

        public void Drop(DrawableHitObject fruit)
        {
            removeFromPlateWithTransform(fruit, f =>
            {
                f.MoveToY(f.Y + 75, 750, Easing.InSine);
                f.FadeOut(750);
            });
        }

        public void Explode(DrawableHitObject fruit)
        {
            var originalX = fruit.X * Scale.X;

            removeFromPlateWithTransform(fruit, f =>
            {
                f.MoveToY(f.Y - 50, 250, Easing.OutSine).Then().MoveToY(f.Y + 50, 500, Easing.InSine);
                f.MoveToX(f.X + originalX * 6, 1000);
                f.FadeOut(750);
            });
        }

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

        private void removeFromPlateWithTransform(DrawableHitObject fruit, Action<DrawableHitObject> action)
        {
            if (ExplodingFruitTarget != null)
            {
                fruit.Anchor = Anchor.TopLeft;
                fruit.Position = caughtFruitContainer.ToSpaceOfOtherDrawable(fruit.DrawPosition, ExplodingFruitTarget);

                if (!caughtFruitContainer.Remove(fruit))
                    // we may have already been removed by a previous operation (due to the weird OnLoadComplete scheduling).
                    // this avoids a crash on potentially attempting to Add a fruit to ExplodingFruitTarget twice.
                    return;

                ExplodingFruitTarget.Add(fruit);
            }

            var actionTime = Clock.CurrentTime;

            fruit.ApplyCustomUpdateState += onFruitOnApplyCustomUpdateState;
            onFruitOnApplyCustomUpdateState(fruit, fruit.State.Value);

            void onFruitOnApplyCustomUpdateState(DrawableHitObject o, ArmedState state)
            {
                using (fruit.BeginAbsoluteSequence(actionTime))
                    action(fruit);

                fruit.Expire();
            }
        }
    }
}
