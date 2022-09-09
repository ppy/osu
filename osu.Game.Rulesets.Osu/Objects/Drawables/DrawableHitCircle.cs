// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Diagnostics;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Judgements;
using osu.Game.Rulesets.Osu.Skinning;
using osu.Game.Rulesets.Osu.Skinning.Default;
using osu.Game.Rulesets.Scoring;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Rulesets.Osu.Objects.Drawables
{
    public class DrawableHitCircle : DrawableOsuHitObject, IHasMainCirclePiece, IHasApproachCircle
    {
        public OsuAction? HitAction => HitArea.HitAction;
        protected virtual OsuSkinComponents CirclePieceComponent => OsuSkinComponents.HitCircle;

        public SkinnableDrawable ApproachCircle { get; private set; }
        public HitReceptor HitArea { get; private set; }
        public SkinnableDrawable CirclePiece { get; private set; }

        Drawable IHasApproachCircle.ApproachCircle => ApproachCircle;

        private Container scaleContainer;
        private InputManager inputManager;

        public DrawableHitCircle()
            : this(null)
        {
        }

        public DrawableHitCircle([CanBeNull] HitCircle h = null)
            : base(h)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Origin = Anchor.Centre;

            InternalChildren = new Drawable[]
            {
                scaleContainer = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                    Children = new Drawable[]
                    {
                        HitArea = new HitReceptor
                        {
                            Hit = () =>
                            {
                                if (AllJudged)
                                    return false;

                                UpdateResult(true);
                                return true;
                            },
                        },
                        CirclePiece = new SkinnableDrawable(new OsuSkinComponent(CirclePieceComponent), _ => new MainCirclePiece())
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                        },
                        ApproachCircle = new ProxyableSkinnableDrawable(new OsuSkinComponent(OsuSkinComponents.ApproachCircle), _ => new DefaultApproachCircle())
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.Both,
                            Alpha = 0,
                            Scale = new Vector2(4),
                        }
                    }
                },
            };

            Size = HitArea.DrawSize;

            PositionBindable.BindValueChanged(_ => Position = HitObject.StackedPosition);
            StackHeightBindable.BindValueChanged(_ => Position = HitObject.StackedPosition);
            ScaleBindable.BindValueChanged(scale => scaleContainer.Scale = new Vector2(scale.NewValue));
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            inputManager = GetContainingInputManager();
        }

        public override double LifetimeStart
        {
            get => base.LifetimeStart;
            set
            {
                base.LifetimeStart = value;
                ApproachCircle.LifetimeStart = value;
            }
        }

        public override double LifetimeEnd
        {
            get => base.LifetimeEnd;
            set
            {
                base.LifetimeEnd = value;
                ApproachCircle.LifetimeEnd = value;
            }
        }

        protected override void CheckForResult(bool userTriggered, double timeOffset)
        {
            Debug.Assert(HitObject.HitWindows != null);

            if (!userTriggered)
            {
                if (!HitObject.HitWindows.CanBeHit(timeOffset))
                    ApplyResult(r => r.Type = r.Judgement.MinResult);

                return;
            }

            var result = ResultFor(timeOffset);

            if (result == HitResult.None || CheckHittable?.Invoke(this, Time.Current) == false)
            {
                Shake(Math.Abs(timeOffset) - HitObject.HitWindows.WindowFor(HitResult.Miss));
                return;
            }

            ApplyResult(r =>
            {
                var circleResult = (OsuHitCircleJudgementResult)r;

                // Todo: This should also consider misses, but they're a little more interesting to handle, since we don't necessarily know the position at the time of a miss.
                if (result.IsHit())
                {
                    var localMousePosition = ToLocalSpace(inputManager.CurrentState.Mouse.Position);
                    circleResult.CursorPositionAtHit = HitObject.StackedPosition + (localMousePosition - DrawSize / 2);
                }

                circleResult.Type = result;
            });
        }

        /// <summary>
        /// Retrieves the <see cref="HitResult"/> for a time offset.
        /// </summary>
        /// <param name="timeOffset">The time offset.</param>
        /// <returns>The hit result, or <see cref="HitResult.None"/> if <paramref name="timeOffset"/> doesn't result in a judgement.</returns>
        protected virtual HitResult ResultFor(double timeOffset) => HitObject.HitWindows.ResultFor(timeOffset);

        protected override void UpdateInitialTransforms()
        {
            base.UpdateInitialTransforms();

            CirclePiece.FadeInFromZero(HitObject.TimeFadeIn);

            ApproachCircle.FadeIn(Math.Min(HitObject.TimeFadeIn * 2, HitObject.TimePreempt));
            ApproachCircle.ScaleTo(1f, HitObject.TimePreempt);
            ApproachCircle.Expire(true);
        }

        protected override void UpdateStartTimeStateTransforms()
        {
            base.UpdateStartTimeStateTransforms();

            // always fade out at the circle's start time (to match user expectations).
            ApproachCircle.FadeOut(50);
        }

        protected override void UpdateHitStateTransforms(ArmedState state)
        {
            Debug.Assert(HitObject.HitWindows != null);

            // todo: temporary / arbitrary, used for lifetime optimisation.
            this.Delay(800).FadeOut();

            // in the case of an early state change, the fade should be expedited to the current point in time.
            if (HitStateUpdateTime < HitObject.StartTime)
                ApproachCircle.FadeOut(50);

            switch (state)
            {
                case ArmedState.Idle:
                    HitArea.HitAction = null;
                    break;

                case ArmedState.Miss:
                    this.FadeOut(100);
                    break;
            }

            Expire();
        }

        public Drawable ProxiedLayer => ApproachCircle;

        protected override JudgementResult CreateResult(Judgement judgement) => new OsuHitCircleJudgementResult(HitObject, judgement);

        public class HitReceptor : CompositeDrawable, IKeyBindingHandler<OsuAction>
        {
            // IsHovered is used
            public override bool HandlePositionalInput => true;

            public Func<bool> Hit;

            public OsuAction? HitAction;

            public HitReceptor()
            {
                Size = new Vector2(OsuHitObject.OBJECT_RADIUS * 2);

                Anchor = Anchor.Centre;
                Origin = Anchor.Centre;

                CornerRadius = OsuHitObject.OBJECT_RADIUS;
                CornerExponent = 2;
            }

            public bool OnPressed(KeyBindingPressEvent<OsuAction> e)
            {
                switch (e.Action)
                {
                    case OsuAction.LeftButton:
                    case OsuAction.RightButton:
                        if (IsHovered && (Hit?.Invoke() ?? false))
                        {
                            HitAction = e.Action;
                            return true;
                        }

                        break;
                }

                return false;
            }

            public void OnReleased(KeyBindingReleaseEvent<OsuAction> e)
            {
            }
        }

        private class ProxyableSkinnableDrawable : SkinnableDrawable
        {
            public override bool RemoveWhenNotAlive => false;

            public ProxyableSkinnableDrawable(ISkinComponent component, Func<ISkinComponent, Drawable> defaultImplementation = null, ConfineMode confineMode = ConfineMode.NoScaling)
                : base(component, defaultImplementation, confineMode)
            {
            }
        }
    }
}
