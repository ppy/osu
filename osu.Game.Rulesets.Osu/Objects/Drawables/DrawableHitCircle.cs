// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects.Drawables.Pieces;
using osuTK;
using osu.Game.Rulesets.Scoring;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Osu.Objects.Drawables
{
    public class DrawableHitCircle : DrawableOsuHitObject, IDrawableHitObjectWithProxiedApproach
    {
        public ApproachCircle ApproachCircle;
        public readonly CirclePiece Circle;
        public readonly RingPiece Ring;
        public readonly FlashPiece Flash;
        public readonly ExplodePiece Explode;
        public readonly NumberPiece Number;
        public readonly GlowPiece Glow;

        private readonly IBindable<Vector2> positionBindable = new Bindable<Vector2>();
        private readonly IBindable<int> stackHeightBindable = new Bindable<int>();
        private readonly IBindable<float> scaleBindable = new Bindable<float>();

        public OsuAction? HitAction => Circle.HitAction;

        private readonly Container explodeContainer;

        private readonly Container scaleContainer;

        public DrawableHitCircle(HitCircle h)
            : base(h)
        {
            Origin = Anchor.Centre;

            Position = HitObject.StackedPosition;

            InternalChildren = new Drawable[]
            {
                scaleContainer = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                    Child = explodeContainer = new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Origin = Anchor.Centre,
                        Anchor = Anchor.Centre,
                        Children = new Drawable[]
                        {
                            Glow = new GlowPiece(),
                            Circle = new CirclePiece
                            {
                                Hit = () =>
                                {
                                    if (AllJudged)
                                        return false;

                                    UpdateResult(true);
                                    return true;
                                },
                            },
                            Number = new NumberPiece
                            {
                                Text = (HitObject.IndexInCurrentCombo + 1).ToString(),
                            },
                            Ring = new RingPiece(),
                            Flash = new FlashPiece(),
                            Explode = new ExplodePiece(),
                            ApproachCircle = new ApproachCircle
                            {
                                Alpha = 0,
                                Scale = new Vector2(4),
                            }
                        }
                    }
                },
            };

            //may not be so correct
            Size = Circle.DrawSize;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            positionBindable.BindValueChanged(_ => Position = HitObject.StackedPosition);
            stackHeightBindable.BindValueChanged(_ => Position = HitObject.StackedPosition);
            scaleBindable.BindValueChanged(scale => scaleContainer.Scale = new Vector2(scale.NewValue), true);

            positionBindable.BindTo(HitObject.PositionBindable);
            stackHeightBindable.BindTo(HitObject.StackHeightBindable);
            scaleBindable.BindTo(HitObject.ScaleBindable);
        }

        public override Color4 AccentColour
        {
            get => base.AccentColour;
            set
            {
                base.AccentColour = value;
                Explode.Colour = AccentColour;
                Glow.Colour = AccentColour;
                Circle.Colour = AccentColour;
                ApproachCircle.Colour = AccentColour;
            }
        }

        protected override void CheckForResult(bool userTriggered, double timeOffset)
        {
            if (!userTriggered)
            {
                if (!HitObject.HitWindows.CanBeHit(timeOffset))
                    ApplyResult(r => r.Type = HitResult.Miss);

                return;
            }

            var result = HitObject.HitWindows.ResultFor(timeOffset);

            if (result == HitResult.None)
            {
                Shake(Math.Abs(timeOffset) - HitObject.HitWindows.HalfWindowFor(HitResult.Miss));
                return;
            }

            ApplyResult(r => r.Type = result);
        }

        protected override void UpdatePreemptState()
        {
            base.UpdatePreemptState();

            ApproachCircle.FadeIn(Math.Min(HitObject.TimeFadeIn * 2, HitObject.TimePreempt));
            ApproachCircle.ScaleTo(1.1f, HitObject.TimePreempt);
            ApproachCircle.Expire(true);
        }

        protected override void UpdateCurrentState(ArmedState state)
        {
            Glow.FadeOut(400);

            switch (state)
            {
                case ArmedState.Idle:
                    this.Delay(HitObject.TimePreempt).FadeOut(500);

                    Expire(true);

                    Circle.HitAction = null;

                    // override lifetime end as FadeIn may have been changed externally, causing out expiration to be too early.
                    LifetimeEnd = HitObject.StartTime + HitObject.HitWindows.HalfWindowFor(HitResult.Miss);
                    break;

                case ArmedState.Miss:
                    ApproachCircle.FadeOut(50);
                    this.FadeOut(100);
                    Expire();
                    break;

                case ArmedState.Hit:
                    ApproachCircle.FadeOut(50);

                    const double flash_in = 40;
                    Flash.FadeTo(0.8f, flash_in)
                         .Then()
                         .FadeOut(100);

                    Explode.FadeIn(flash_in);

                    using (BeginDelayedSequence(flash_in, true))
                    {
                        //after the flash, we can hide some elements that were behind it
                        Ring.FadeOut();
                        Circle.FadeOut();
                        Number.FadeOut();

                        this.FadeOut(800);
                        explodeContainer.ScaleTo(1.5f, 400, Easing.OutQuad);
                    }

                    Expire();
                    break;
            }
        }

        public Drawable ProxiedLayer => ApproachCircle;
    }
}
