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
        private readonly CirclePiece circle;
        private readonly RingPiece ring;
        private readonly FlashPiece flash;
        private readonly ExplodePiece explode;
        private readonly NumberPiece number;
        private readonly GlowPiece glow;

        private readonly IBindable<Vector2> positionBindable = new Bindable<Vector2>();
        private readonly IBindable<int> stackHeightBindable = new Bindable<int>();
        private readonly IBindable<float> scaleBindable = new Bindable<float>();

        public OsuAction? HitAction => circle.HitAction;

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
                            glow = new GlowPiece(),
                            circle = new CirclePiece
                            {
                                Hit = () =>
                                {
                                    if (AllJudged)
                                        return false;

                                    UpdateResult(true);
                                    return true;
                                },
                            },
                            number = new NumberPiece
                            {
                                Text = (HitObject.IndexInCurrentCombo + 1).ToString(),
                            },
                            ring = new RingPiece(),
                            flash = new FlashPiece(),
                            explode = new ExplodePiece(),
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
            Size = circle.DrawSize;
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
                explode.Colour = AccentColour;
                glow.Colour = AccentColour;
                circle.Colour = AccentColour;
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
            glow.FadeOut(400);

            switch (state)
            {
                case ArmedState.Idle:
                    this.Delay(HitObject.TimePreempt).FadeOut(500);

                    Expire(true);

                    circle.HitAction = null;

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
                    flash.FadeTo(0.8f, flash_in)
                         .Then()
                         .FadeOut(100);

                    explode.FadeIn(flash_in);

                    using (BeginDelayedSequence(flash_in, true))
                    {
                        //after the flash, we can hide some elements that were behind it
                        ring.FadeOut();
                        circle.FadeOut();
                        number.FadeOut();

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
