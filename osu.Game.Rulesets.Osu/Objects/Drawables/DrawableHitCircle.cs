﻿// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects.Drawables.Pieces;
using OpenTK;
using osu.Game.Rulesets.Osu.Judgements;
using osu.Game.Rulesets.Scoring;
using OpenTK.Graphics;

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

        public DrawableHitCircle(HitCircle h)
            : base(h)
        {
            Origin = Anchor.Centre;

            Position = HitObject.StackedPosition;
            Scale = new Vector2(h.Scale);

            InternalChildren = new Drawable[]
            {
                glow = new GlowPiece(),
                circle = new CirclePiece
                {
                    Hit = () =>
                    {
                        if (AllJudged)
                            return false;

                        UpdateJudgement(true);
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
            };

            //may not be so correct
            Size = circle.DrawSize;

            HitObject.PositionChanged += _ => Position = HitObject.StackedPosition;
        }

        public override Color4 AccentColour
        {
            get { return base.AccentColour; }
            set
            {
                base.AccentColour = value;
                explode.Colour = AccentColour;
                glow.Colour = AccentColour;
                circle.Colour = AccentColour;
                ApproachCircle.Colour = AccentColour;
            }
        }

        protected override void CheckForJudgements(bool userTriggered, double timeOffset)
        {
            if (!userTriggered)
            {
                if (!HitObject.HitWindows.CanBeHit(timeOffset))
                    AddJudgement(new OsuJudgement { Result = HitResult.Miss });
                return;
            }

            var result = HitObject.HitWindows.ResultFor(timeOffset);
            if (result == HitResult.None)
                return;

            AddJudgement(new OsuJudgement
            {
                Result = result,
                PositionOffset = Vector2.Zero //todo: set to correct value
            });
        }

        protected override void UpdatePreemptState()
        {
            base.UpdatePreemptState();

            ApproachCircle.FadeIn(Math.Min(HitObject.TimeFadein * 2, HitObject.TimePreempt));
            ApproachCircle.ScaleTo(1.1f, HitObject.TimePreempt);
        }

        protected override void UpdateCurrentState(ArmedState state)
        {
            glow.FadeOut(400);

            switch (state)
            {
                case ArmedState.Idle:
                    this.Delay(HitObject.TimePreempt).FadeOut(500);

                    Expire(true);

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

                        this.FadeOut(800)
                            .ScaleTo(Scale * 1.5f, 400, Easing.OutQuad);
                    }

                    Expire();
                    break;
            }
        }

        public Drawable ProxiedLayer => ApproachCircle;
    }
}
