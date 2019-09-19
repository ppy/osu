// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Bindings;
using osu.Framework.MathUtils;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Catch.Judgements;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.Objects.Drawable;
using osu.Game.Rulesets.Catch.Replays;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.UI;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Catch.UI
{
    public class CatcherArea : Container
    {
        public const float CATCHER_SIZE = 106.75f;

        protected internal readonly Catcher MovableCatcher;

        public Func<CatchHitObject, DrawableHitObject<CatchHitObject>> CreateDrawableRepresentation;

        public Container ExplodingFruitTarget
        {
            set => MovableCatcher.ExplodingFruitTarget = value;
        }

        public CatcherArea(BeatmapDifficulty difficulty = null)
        {
            RelativeSizeAxes = Axes.X;
            Height = CATCHER_SIZE;
            Child = MovableCatcher = new Catcher(difficulty)
            {
                AdditiveTarget = this,
            };
        }

        private DrawableCatchHitObject lastPlateableFruit;

        public void OnResult(DrawableCatchHitObject fruit, JudgementResult result)
        {
            void runAfterLoaded(Action action)
            {
                if (lastPlateableFruit == null)
                    return;

                // this is required to make this run after the last caught fruit runs updateState() at least once.
                // TODO: find a better alternative
                if (lastPlateableFruit.IsLoaded)
                    action();
                else
                    lastPlateableFruit.OnLoadComplete += _ => action();
            }

            if (result.IsHit && fruit.CanBePlated)
            {
                var caughtFruit = (DrawableCatchHitObject)CreateDrawableRepresentation?.Invoke(fruit.HitObject);

                if (caughtFruit == null) return;

                caughtFruit.RelativePositionAxes = Axes.None;
                caughtFruit.Position = new Vector2(MovableCatcher.ToLocalSpace(fruit.ScreenSpaceDrawQuad.Centre).X - MovableCatcher.DrawSize.X / 2, 0);
                caughtFruit.IsOnPlate = true;

                caughtFruit.Anchor = Anchor.TopCentre;
                caughtFruit.Origin = Anchor.Centre;
                caughtFruit.Scale *= 0.7f;
                caughtFruit.LifetimeStart = caughtFruit.HitObject.StartTime;
                caughtFruit.LifetimeEnd = double.MaxValue;

                MovableCatcher.Add(caughtFruit);
                lastPlateableFruit = caughtFruit;

                if (!fruit.StaysOnPlate)
                    runAfterLoaded(() => MovableCatcher.Explode(caughtFruit));
            }

            if (fruit.HitObject.LastInCombo)
            {
                if (((CatchJudgement)result.Judgement).ShouldExplodeFor(result))
                    runAfterLoaded(() => MovableCatcher.Explode());
                else
                    MovableCatcher.Drop();
            }
        }

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();

            var state = (GetContainingInputManager().CurrentState as RulesetInputManagerInputState<CatchAction>)?.LastReplayState as CatchFramedReplayInputHandler.CatchReplayState;

            if (state?.CatcherX != null)
                MovableCatcher.X = state.CatcherX.Value;
        }

        public bool OnReleased(CatchAction action) => false;

        public bool AttemptCatch(CatchHitObject obj) => MovableCatcher.AttemptCatch(obj);

        public static float GetCatcherSize(BeatmapDifficulty difficulty)
        {
            return CATCHER_SIZE / CatchPlayfield.BASE_WIDTH * (1.0f - 0.7f * (difficulty.CircleSize - 5) / 5);
        }

        public class Catcher : Container, IKeyBindingHandler<CatchAction>
        {
            /// <summary>
            /// Width of the area that can be used to attempt catches during gameplay.
            /// </summary>
            internal float CatchWidth => CATCHER_SIZE * Math.Abs(Scale.X);

            private Container<DrawableHitObject> caughtFruit;

            public Container ExplodingFruitTarget;

            public Container AdditiveTarget;

            public Catcher(BeatmapDifficulty difficulty = null)
            {
                RelativePositionAxes = Axes.X;
                X = 0.5f;

                Origin = Anchor.TopCentre;
                Anchor = Anchor.TopLeft;

                Size = new Vector2(CATCHER_SIZE);
                if (difficulty != null)
                    Scale = new Vector2(1.0f - 0.7f * (difficulty.CircleSize - 5) / 5);
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                Children = new[]
                {
                    caughtFruit = new Container<DrawableHitObject>
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.BottomCentre,
                    },
                    createCatcherSprite(),
                };
            }

            private int currentDirection;

            private bool dashing;

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

            private bool trail;

            /// <summary>
            /// Activate or deactive the trail. Will be automatically deactivated when conditions to keep the trail displayed are no longer met.
            /// </summary>
            protected bool Trail
            {
                get => trail;
                set
                {
                    if (value == trail) return;

                    trail = value;

                    if (Trail)
                        beginTrail();
                }
            }

            private void beginTrail()
            {
                Trail &= dashing || HyperDashing;
                Trail &= AdditiveTarget != null;

                if (!Trail) return;

                var additive = createCatcherSprite();

                additive.Anchor = Anchor;
                additive.OriginPosition = additive.OriginPosition + new Vector2(DrawWidth / 2, 0); // also temporary to align sprite correctly.
                additive.Position = Position;
                additive.Scale = Scale;
                additive.Colour = HyperDashing ? Color4.Red : Color4.White;
                additive.RelativePositionAxes = RelativePositionAxes;
                additive.Blending = BlendingParameters.Additive;

                AdditiveTarget.Add(additive);

                additive.FadeTo(0.4f).FadeOut(800, Easing.OutQuint);
                additive.Expire(true);

                Scheduler.AddDelayed(beginTrail, HyperDashing ? 25 : 50);
            }

            private Drawable createCatcherSprite() => new CatcherSprite();

            /// <summary>
            /// Add a caught fruit to the catcher's stack.
            /// </summary>
            /// <param name="fruit">The fruit that was caught.</param>
            public void Add(DrawableHitObject fruit)
            {
                float ourRadius = fruit.DrawSize.X / 2 * fruit.Scale.X;
                float theirRadius = 0;

                const float allowance = 6;

                while (caughtFruit.Any(f =>
                    f.LifetimeEnd == double.MaxValue &&
                    Vector2Extensions.Distance(f.Position, fruit.Position) < (ourRadius + (theirRadius = f.DrawSize.X / 2 * f.Scale.X)) / (allowance / 2)))
                {
                    float diff = (ourRadius + theirRadius) / allowance;
                    fruit.X += (RNG.NextSingle() - 0.5f) * 2 * diff;
                    fruit.Y -= RNG.NextSingle() * diff;
                }

                fruit.X = MathHelper.Clamp(fruit.X, -CATCHER_SIZE / 2, CATCHER_SIZE / 2);

                caughtFruit.Add(fruit);
            }

            /// <summary>
            /// Let the catcher attempt to catch a fruit.
            /// </summary>
            /// <param name="fruit">The fruit to catch.</param>
            /// <returns>Whether the catch is possible.</returns>
            public bool AttemptCatch(CatchHitObject fruit)
            {
                float halfCatchWidth = CatchWidth * 0.5f;

                // this stuff wil disappear once we move fruit to non-relative coordinate space in the future.
                var catchObjectPosition = fruit.X * CatchPlayfield.BASE_WIDTH;
                var catcherPosition = Position.X * CatchPlayfield.BASE_WIDTH;

                var validCatch =
                    catchObjectPosition >= catcherPosition - halfCatchWidth &&
                    catchObjectPosition <= catcherPosition + halfCatchWidth;

                if (validCatch && fruit.HyperDash)
                {
                    var target = fruit.HyperDashTarget;
                    double timeDifference = target.StartTime - fruit.StartTime;
                    double positionDifference = target.X * CatchPlayfield.BASE_WIDTH - catcherPosition;
                    double velocity = positionDifference / Math.Max(1.0, timeDifference - 1000.0 / 60.0);

                    SetHyperDashState(Math.Abs(velocity), target.X);
                }
                else
                {
                    SetHyperDashState();
                }

                return validCatch;
            }

            private double hyperDashModifier = 1;
            private int hyperDashDirection;
            private float hyperDashTargetPosition;

            /// <summary>
            /// Whether we are hyper-dashing or not.
            /// </summary>
            public bool HyperDashing => hyperDashModifier != 1;

            /// <summary>
            /// Set hyper-dash state.
            /// </summary>
            /// <param name="modifier">The speed multiplier. If this is less or equals to 1, this catcher will be non-hyper-dashing state.</param>
            /// <param name="targetPosition">When this catcher crosses this position, this catcher ends hyper-dashing.</param>
            public void SetHyperDashState(double modifier = 1, float targetPosition = -1)
            {
                const float hyper_dash_transition_length = 180;

                bool previouslyHyperDashing = HyperDashing;

                if (modifier <= 1 || X == targetPosition)
                {
                    hyperDashModifier = 1;
                    hyperDashDirection = 0;

                    if (previouslyHyperDashing)
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

                    if (!previouslyHyperDashing)
                    {
                        this.FadeColour(Color4.OrangeRed, hyper_dash_transition_length, Easing.OutQuint);
                        this.FadeTo(0.2f, hyper_dash_transition_length, Easing.OutQuint);
                        Trail = true;
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

            public bool OnReleased(CatchAction action)
            {
                switch (action)
                {
                    case CatchAction.MoveLeft:
                        currentDirection++;
                        return true;

                    case CatchAction.MoveRight:
                        currentDirection--;
                        return true;

                    case CatchAction.Dash:
                        Dashing = false;
                        return true;
                }

                return false;
            }

            /// <summary>
            /// The relative space to cover in 1 millisecond. based on 1 game pixel per millisecond as in osu-stable.
            /// </summary>
            public const double BASE_SPEED = 1.0 / 512;

            protected override void Update()
            {
                base.Update();

                if (currentDirection == 0) return;

                var direction = Math.Sign(currentDirection);

                double dashModifier = Dashing ? 1 : 0.5;
                double speed = BASE_SPEED * dashModifier * hyperDashModifier;

                Scale = new Vector2(Math.Abs(Scale.X) * direction, Scale.Y);
                X = (float)MathHelper.Clamp(X + direction * Clock.ElapsedFrameTime * speed, 0, 1);

                // Correct overshooting.
                if ((hyperDashDirection > 0 && hyperDashTargetPosition < X) ||
                    (hyperDashDirection < 0 && hyperDashTargetPosition > X))
                {
                    X = hyperDashTargetPosition;
                    SetHyperDashState();
                }
            }

            /// <summary>
            /// Drop any fruit off the plate.
            /// </summary>
            public void Drop()
            {
                var fruit = caughtFruit.ToArray();

                foreach (var f in fruit)
                {
                    if (ExplodingFruitTarget != null)
                    {
                        f.Anchor = Anchor.TopLeft;
                        f.Position = caughtFruit.ToSpaceOfOtherDrawable(f.DrawPosition, ExplodingFruitTarget);

                        caughtFruit.Remove(f);

                        ExplodingFruitTarget.Add(f);
                    }

                    f.MoveToY(f.Y + 75, 750, Easing.InSine);
                    f.FadeOut(750);

                    // todo: this shouldn't exist once DrawableHitObject's ClearTransformsAfter overrides are repaired.
                    f.LifetimeStart = Time.Current;
                    f.Expire();
                }
            }

            /// <summary>
            /// Explode any fruit off the plate.
            /// </summary>
            public void Explode()
            {
                foreach (var f in caughtFruit.ToArray())
                    Explode(f);
            }

            public void Explode(DrawableHitObject fruit)
            {
                var originalX = fruit.X * Scale.X;

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

                fruit.ClearTransforms();
                fruit.MoveToY(fruit.Y - 50, 250, Easing.OutSine).Then().MoveToY(fruit.Y + 50, 500, Easing.InSine);
                fruit.MoveToX(fruit.X + originalX * 6, 1000);
                fruit.FadeOut(750);

                // todo: this shouldn't exist once DrawableHitObject's ClearTransformsAfter overrides are repaired.
                fruit.LifetimeStart = Time.Current;
                fruit.Expire();
            }
        }
    }
}
