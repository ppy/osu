// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Animations;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Input.Bindings;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Catch.Judgements;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.Objects.Drawables;
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
            if (result.Judgement is IgnoreJudgement)
                return;

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
                // create a new (cloned) fruit to stay on the plate. the original is faded out immediately.
                var caughtFruit = (DrawableCatchHitObject)CreateDrawableRepresentation?.Invoke(fruit.HitObject);

                if (caughtFruit == null) return;

                caughtFruit.RelativePositionAxes = Axes.None;
                caughtFruit.Position = new Vector2(MovableCatcher.ToLocalSpace(fruit.ScreenSpaceDrawQuad.Centre).X - MovableCatcher.DrawSize.X / 2, 0);
                caughtFruit.IsOnPlate = true;

                caughtFruit.Anchor = Anchor.TopCentre;
                caughtFruit.Origin = Anchor.Centre;
                caughtFruit.Scale *= 0.5f;
                caughtFruit.LifetimeStart = caughtFruit.HitObject.StartTime;
                caughtFruit.LifetimeEnd = double.MaxValue;

                MovableCatcher.PlaceOnPlate(caughtFruit);
                lastPlateableFruit = caughtFruit;

                if (!fruit.StaysOnPlate)
                    runAfterLoaded(() => MovableCatcher.Explode(caughtFruit));
            }

            if (fruit.HitObject.LastInCombo)
            {
                if (result.Judgement is CatchJudgement catchJudgement && catchJudgement.ShouldExplodeFor(result))
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

        public void OnReleased(CatchAction action)
        {
        }

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

                Size = new Vector2(CATCHER_SIZE);
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

            private CatcherSprite catcherIdle;
            private CatcherSprite catcherKiai;
            private CatcherSprite catcherFail;

            private void updateCatcher()
            {
                catcherIdle.Hide();
                catcherKiai.Hide();
                catcherFail.Hide();

                CatcherSprite current;

                switch (currentState)
                {
                    default:
                        current = catcherIdle;
                        break;

                    case CatcherAnimationState.Fail:
                        current = catcherFail;
                        break;

                    case CatcherAnimationState.Kiai:
                        current = catcherKiai;
                        break;
                }

                current.Show();
                (current.Drawable as IAnimation)?.GotoFrame(0);
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

                var additive = createAdditiveSprite(HyperDashing);

                additive.FadeTo(0.4f).FadeOut(800, Easing.OutQuint);
                additive.Expire(true);

                Scheduler.AddDelayed(beginTrail, HyperDashing ? 25 : 50);
            }

            private Drawable createAdditiveSprite(bool hyperDash)
            {
                var additive = createCatcherSprite();

                additive.Anchor = Anchor;
                additive.Scale = Scale;
                additive.Colour = hyperDash ? Color4.Red : Color4.White;
                additive.Blending = BlendingParameters.Additive;
                additive.RelativePositionAxes = RelativePositionAxes;
                additive.Position = Position;

                AdditiveTarget.Add(additive);

                return additive;
            }

            private Drawable createCatcherSprite() => new CatcherSprite(currentState);

            /// <summary>
            /// Add a caught fruit to the catcher's stack.
            /// </summary>
            /// <param name="fruit">The fruit that was caught.</param>
            public void PlaceOnPlate(DrawableCatchHitObject fruit)
            {
                float ourRadius = fruit.DisplayRadius;
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

                fruit.X = Math.Clamp(fruit.X, -CATCHER_SIZE / 2, CATCHER_SIZE / 2);

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
                float halfCatchWidth = CatchWidth * 0.5f;

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
                    double timeDifference = target.StartTime - fruit.StartTime;
                    double positionDifference = target.X * CatchPlayfield.BASE_WIDTH - catcherPosition;
                    double velocity = positionDifference / Math.Max(1.0, timeDifference - 1000.0 / 60.0);

                    SetHyperDashState(Math.Abs(velocity), target.X);
                }
                else
                {
                    SetHyperDashState();
                }

                if (validCatch)
                    updateState(fruit.Kiai ? CatcherAnimationState.Kiai : CatcherAnimationState.Idle);
                else
                    updateState(CatcherAnimationState.Fail);

                return validCatch;
            }

            private void updateState(CatcherAnimationState state)
            {
                if (currentState == state)
                    return;

                currentState = state;
                updateCatcher();
            }

            private CatcherAnimationState currentState;

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

                bool wasHyperDashing = HyperDashing;

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

                        var hyperDashEndGlow = createAdditiveSprite(true);

                        hyperDashEndGlow.MoveToOffset(new Vector2(0, -20), 1200, Easing.In);
                        hyperDashEndGlow.ScaleTo(hyperDashEndGlow.Scale * 0.9f).ScaleTo(hyperDashEndGlow.Scale * 1.2f, 1200, Easing.In);
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

                UpdatePosition((float)(X + direction * Clock.ElapsedFrameTime * speed));

                // Correct overshooting.
                if ((hyperDashDirection > 0 && hyperDashTargetPosition < X) ||
                    (hyperDashDirection < 0 && hyperDashTargetPosition > X))
                {
                    X = hyperDashTargetPosition;
                    SetHyperDashState();
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

            public void Drop(DrawableHitObject fruit) => removeFromPlateWithTransform(fruit, f =>
            {
                f.MoveToY(f.Y + 75, 750, Easing.InSine);
                f.FadeOut(750);
            });

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

                double actionTime = Clock.CurrentTime;

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

    public class HitExplosion : CompositeDrawable
    {
        private readonly CircularContainer largeFaint;

        public HitExplosion(DrawableCatchHitObject fruit)
        {
            Size = new Vector2(20);
            Anchor = Anchor.TopCentre;
            Origin = Anchor.BottomCentre;

            Color4 objectColour = fruit.AccentColour.Value;

            // scale roughly in-line with visual appearance of notes

            const float angle_variangle = 15; // should be less than 45

            const float roundness = 100;

            const float initial_height = 10;

            var colour = Interpolation.ValueAt(0.4f, objectColour, Color4.White, 0, 1);

            InternalChildren = new Drawable[]
            {
                largeFaint = new CircularContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Masking = true,
                    // we want our size to be very small so the glow dominates it.
                    Size = new Vector2(0.8f),
                    Blending = BlendingParameters.Additive,
                    EdgeEffect = new EdgeEffectParameters
                    {
                        Type = EdgeEffectType.Glow,
                        Colour = Interpolation.ValueAt(0.1f, objectColour, Color4.White, 0, 1).Opacity(0.3f),
                        Roundness = 160,
                        Radius = 200,
                    },
                },
                new CircularContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Masking = true,
                    Blending = BlendingParameters.Additive,
                    EdgeEffect = new EdgeEffectParameters
                    {
                        Type = EdgeEffectType.Glow,
                        Colour = Interpolation.ValueAt(0.6f, objectColour, Color4.White, 0, 1),
                        Roundness = 20,
                        Radius = 50,
                    },
                },
                new CircularContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Masking = true,
                    Size = new Vector2(0.01f, initial_height),
                    Blending = BlendingParameters.Additive,
                    Rotation = RNG.NextSingle(-angle_variangle, angle_variangle),
                    EdgeEffect = new EdgeEffectParameters
                    {
                        Type = EdgeEffectType.Glow,
                        Colour = colour,
                        Roundness = roundness,
                        Radius = 40,
                    },
                },
                new CircularContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Masking = true,
                    Size = new Vector2(0.01f, initial_height),
                    Blending = BlendingParameters.Additive,
                    Rotation = RNG.NextSingle(-angle_variangle, angle_variangle),
                    EdgeEffect = new EdgeEffectParameters
                    {
                        Type = EdgeEffectType.Glow,
                        Colour = colour,
                        Roundness = roundness,
                        Radius = 40,
                    },
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            const double duration = 400;

            largeFaint
                .ResizeTo(largeFaint.Size * new Vector2(5, 1), duration, Easing.OutQuint)
                .FadeOut(duration * 2);

            this.FadeInFromZero(50).Then().FadeOut(duration, Easing.Out);
            Expire(true);
        }
    }
}
