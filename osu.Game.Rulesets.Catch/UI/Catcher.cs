// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Animations;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Bindings;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.Objects.Drawables;
using osu.Game.Rulesets.Objects.Drawables;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Catch.UI
{
    public class Catcher : Container, IKeyBindingHandler<CatchAction>
    {
        /// <summary>
        /// Whether we are hyper-dashing or not.
        /// </summary>
        public bool HyperDashing => hyperDashModifier != 1;

        /// <summary>
        /// The relative space to cover in 1 millisecond. based on 1 game pixel per millisecond as in osu-stable.
        /// </summary>
        public const double BASE_SPEED = 1.0 / 512;

        public Container ExplodingFruitTarget;

        public Container AdditiveTarget;

        public CatcherAnimationState CurrentState { get; private set; }

        /// <summary>
        /// Width of the area that can be used to attempt catches during gameplay.
        /// </summary>
        internal float CatchWidth => CatcherArea.CATCHER_SIZE * Math.Abs(Scale.X);

        protected bool Dashing
        {
            get => dashing;
            set
            {
                if (value == dashing) return;

                dashing = value;

                Trail |= dashing;
            }
        }

        /// <summary>
        /// Activate or deactivate the trail. Will be automatically deactivated when conditions to keep the trail displayed are no longer met.
        /// </summary>
        protected bool Trail
        {
            get => trail;
            set
            {
                if (value == trail || AdditiveTarget == null) return;

                trail = value;

                if (Trail)
                    beginTrail();
            }
        }

        private Container<DrawableHitObject> caughtFruit;

        private CatcherSprite catcherIdle;
        private CatcherSprite catcherKiai;
        private CatcherSprite catcherFail;

        private CatcherSprite currentCatcher;

        private int currentDirection;

        private bool dashing;

        private bool trail;

        private double hyperDashModifier = 1;
        private int hyperDashDirection;
        private float hyperDashTargetPosition;

        public Catcher(BeatmapDifficulty difficulty = null)
        {
            RelativePositionAxes = Axes.X;
            X = 0.5f;

            Origin = Anchor.TopCentre;

            Size = new Vector2(CatcherArea.CATCHER_SIZE);
            if (difficulty != null)
                Scale = new Vector2(1.0f - 0.7f * (difficulty.CircleSize - 5) / 5);
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Children = new Drawable[]
            {
                caughtFruit = new Container<DrawableHitObject>
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
                }
            };

            updateCatcher();
        }

        /// <summary>
        /// Add a caught fruit to the catcher's stack.
        /// </summary>
        /// <param name="fruit">The fruit that was caught.</param>
        public void PlaceOnPlate(DrawableCatchHitObject fruit)
        {
            var ourRadius = fruit.DisplayRadius;
            float theirRadius = 0;

            const float allowance = 6;

            while (caughtFruit.Any(f =>
                f.LifetimeEnd == double.MaxValue &&
                Vector2Extensions.Distance(f.Position, fruit.Position) < (ourRadius + (theirRadius = f.DrawSize.X / 2 * f.Scale.X)) / (allowance / 2)))
            {
                var diff = (ourRadius + theirRadius) / allowance;
                fruit.X += (RNG.NextSingle() - 0.5f) * 2 * diff;
                fruit.Y -= RNG.NextSingle() * diff;
            }

            fruit.X = Math.Clamp(fruit.X, -CatcherArea.CATCHER_SIZE / 2, CatcherArea.CATCHER_SIZE / 2);

            caughtFruit.Add(fruit);

            Add(new HitExplosion(fruit)
            {
                X = fruit.X,
                Scale = new Vector2(fruit.HitObject.Scale)
            });
        }

        /// <summary>
        /// Let the catcher attempt to catch a fruit.
        /// </summary>
        /// <param name="fruit">The fruit to catch.</param>
        /// <returns>Whether the catch is possible.</returns>
        public bool AttemptCatch(CatchHitObject fruit)
        {
            var halfCatchWidth = CatchWidth * 0.5f;

            // this stuff wil disappear once we move fruit to non-relative coordinate space in the future.
            var catchObjectPosition = fruit.X * CatchPlayfield.BASE_WIDTH;
            var catcherPosition = Position.X * CatchPlayfield.BASE_WIDTH;

            var validCatch =
                catchObjectPosition >= catcherPosition - halfCatchWidth &&
                catchObjectPosition <= catcherPosition + halfCatchWidth;

            // only update hyperdash state if we are catching a fruit.
            // exceptions are Droplets and JuiceStreams.
            if (!(fruit is Fruit)) return validCatch;

            if (validCatch && fruit.HyperDash)
            {
                var target = fruit.HyperDashTarget;
                var timeDifference = target.StartTime - fruit.StartTime;
                double positionDifference = target.X * CatchPlayfield.BASE_WIDTH - catcherPosition;
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
            const float hyper_dash_transition_length = 180;

            var wasHyperDashing = HyperDashing;

            if (modifier <= 1 || X == targetPosition)
            {
                hyperDashModifier = 1;
                hyperDashDirection = 0;

                if (wasHyperDashing)
                {
                    this.FadeColour(Color4.White, hyper_dash_transition_length, Easing.OutQuint);
                    this.FadeTo(1, hyper_dash_transition_length, Easing.OutQuint);
                    Trail &= Dashing;
                }
            }
            else
            {
                hyperDashModifier = modifier;
                hyperDashDirection = Math.Sign(targetPosition - X);
                hyperDashTargetPosition = targetPosition;

                if (!wasHyperDashing)
                {
                    this.FadeColour(Color4.OrangeRed, hyper_dash_transition_length, Easing.OutQuint);
                    this.FadeTo(0.2f, hyper_dash_transition_length, Easing.OutQuint);
                    Trail = true;

                    var hyperDashEndGlow = createAdditiveSprite();

                    hyperDashEndGlow.MoveToOffset(new Vector2(0, -10), 1200, Easing.In);
                    hyperDashEndGlow.ScaleTo(hyperDashEndGlow.Scale * 0.95f).ScaleTo(hyperDashEndGlow.Scale * 1.2f, 1200, Easing.In);
                    hyperDashEndGlow.FadeOut(1200);
                    hyperDashEndGlow.Expire(true);
                }
            }
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

        public void UpdatePosition(float position)
        {
            position = Math.Clamp(position, 0, 1);

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
            foreach (var f in caughtFruit.ToArray())
                Drop(f);
        }

        /// <summary>
        /// Explode any fruit off the plate.
        /// </summary>
        public void Explode()
        {
            foreach (var f in caughtFruit.ToArray())
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
            (currentCatcher.Drawable as IAnimation)?.GotoFrame(0);
        }

        private void beginTrail()
        {
            if (!dashing && !HyperDashing)
            {
                Trail = false;
                return;
            }

            var additive = createAdditiveSprite();

            additive.FadeTo(0.4f).FadeOut(800, Easing.OutQuint);
            additive.Expire(true);

            Scheduler.AddDelayed(beginTrail, HyperDashing ? 25 : 50);
        }

        private void updateState(CatcherAnimationState state)
        {
            if (CurrentState == state)
                return;

            CurrentState = state;
            updateCatcher();
        }

        private CatcherTrailSprite createAdditiveSprite()
        {
            var tex = (currentCatcher.Drawable as TextureAnimation)?.CurrentFrame ?? ((Sprite)currentCatcher.Drawable).Texture;

            var sprite = new CatcherTrailSprite(tex)
            {
                Anchor = Anchor,
                Scale = Scale,
                Colour = HyperDashing ? Color4.Red : Color4.White,
                Blending = BlendingParameters.Additive,
                RelativePositionAxes = RelativePositionAxes,
                Position = Position
            };

            AdditiveTarget?.Add(sprite);

            return sprite;
        }

        private void removeFromPlateWithTransform(DrawableHitObject fruit, Action<DrawableHitObject> action)
        {
            if (ExplodingFruitTarget != null)
            {
                fruit.Anchor = Anchor.TopLeft;
                fruit.Position = caughtFruit.ToSpaceOfOtherDrawable(fruit.DrawPosition, ExplodingFruitTarget);

                if (!caughtFruit.Remove(fruit))
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
