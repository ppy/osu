﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects.Drawables.Pieces;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Osu.Judgements;
using osu.Framework.Graphics.Primitives;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Osu.Objects.Drawables
{
    public class DrawableSlider : DrawableOsuHitObject, IDrawableHitObjectWithProxiedApproach
    {
        private readonly Slider slider;

        public readonly DrawableHitCircle InitialCircle;

        private readonly List<ISliderProgress> components = new List<ISliderProgress>();

        private readonly Container<DrawableSliderTick> ticks;
        private readonly Container<DrawableRepeatPoint> repeatPoints;

        public readonly SliderBody Body;
        public readonly SliderBall Ball;

        public DrawableSlider(Slider s)
            : base(s)
        {
            slider = s;

            Children = new Drawable[]
            {
                Body = new SliderBody(s)
                {
                    AccentColour = AccentColour,
                    Position = s.StackedPosition,
                    PathWidth = s.Scale * 64,
                },
                ticks = new Container<DrawableSliderTick>(),
                repeatPoints = new Container<DrawableRepeatPoint>(),
                Ball = new SliderBall(s)
                {
                    Scale = new Vector2(s.Scale),
                    AccentColour = AccentColour,
                    AlwaysPresent = true,
                    Alpha = 0
                },
                InitialCircle = new DrawableHitCircle(new HitCircle
                {
                    StartTime = s.StartTime,
                    Position = s.StackedPosition,
                    ComboIndex = s.ComboIndex,
                    Scale = s.Scale,
                    ComboColour = s.ComboColour,
                    Samples = s.Samples,
                    SampleControlPoint = s.SampleControlPoint
                })
            };

            components.Add(Body);
            components.Add(Ball);

            AddNested(InitialCircle);

            var repeatDuration = s.Curve.Distance / s.Velocity;
            foreach (var tick in s.NestedHitObjects.OfType<SliderTick>())
            {
                var repeatStartTime = s.StartTime + tick.RepeatIndex * repeatDuration;
                var fadeInTime = repeatStartTime + (tick.StartTime - repeatStartTime) / 2 - (tick.RepeatIndex == 0 ? FadeInDuration : FadeInDuration / 2);
                var fadeOutTime = repeatStartTime + repeatDuration;

                var drawableTick = new DrawableSliderTick(tick)
                {
                    FadeInTime = fadeInTime,
                    FadeOutTime = fadeOutTime,
                    Position = tick.Position,
                };

                ticks.Add(drawableTick);
                AddNested(drawableTick);
            }

            foreach (var repeatPoint in s.NestedHitObjects.OfType<RepeatPoint>())
            {
                var repeatStartTime = s.StartTime + repeatPoint.RepeatIndex * repeatDuration;
                var fadeInTime = repeatStartTime + (repeatPoint.StartTime - repeatStartTime) / 2 - (repeatPoint.RepeatIndex == 0 ? FadeInDuration : FadeInDuration / 2);
                var fadeOutTime = repeatStartTime + repeatDuration;

                var drawableRepeatPoint = new DrawableRepeatPoint(repeatPoint, this)
                {
                    FadeInTime = fadeInTime,
                    FadeOutTime = fadeOutTime,
                    Position = repeatPoint.Position,
                };

                repeatPoints.Add(drawableRepeatPoint);
                AddNested(drawableRepeatPoint);
            }
        }

        private int currentRepeat;
        public bool Tracking;

        public override double FadeInDuration
        {
            get { return base.FadeInDuration; }
            set { InitialCircle.FadeInDuration = base.FadeInDuration = value; }
        }

        protected override void Update()
        {
            base.Update();

            Tracking = Ball.Tracking;

            double progress = MathHelper.Clamp((Time.Current - slider.StartTime) / slider.Duration, 0, 1);

            int repeat = slider.RepeatAt(progress);
            progress = slider.ProgressAt(progress);

            if (repeat > currentRepeat)
                currentRepeat = repeat;

            //todo: we probably want to reconsider this before adding scoring, but it looks and feels nice.
            if (!InitialCircle.Judgements.Any(j => j.IsHit))
                InitialCircle.Position = slider.Curve.PositionAt(progress);

            foreach (var c in components) c.UpdateProgress(progress, repeat);
            foreach (var t in ticks.Children) t.Tracking = Ball.Tracking;
        }

        protected override void CheckForJudgements(bool userTriggered, double timeOffset)
        {
            if (!userTriggered && Time.Current >= slider.EndTime)
            {
                var judgementsCount = ticks.Children.Count + repeatPoints.Children.Count + 1;
                var judgementsHit = ticks.Children.Count(t => t.Judgements.Any(j => j.IsHit)) + repeatPoints.Children.Count(t => t.Judgements.Any(j => j.IsHit));
                if (InitialCircle.Judgements.Any(j => j.IsHit))
                    judgementsHit++;

                var hitFraction = (double)judgementsHit / judgementsCount;
                if (hitFraction == 1 && InitialCircle.Judgements.Any(j => j.Result == HitResult.Great))
                    AddJudgement(new OsuJudgement { Result = HitResult.Great });
                else if (hitFraction >= 0.5 && InitialCircle.Judgements.Any(j => j.Result >= HitResult.Good))
                    AddJudgement(new OsuJudgement { Result = HitResult.Good });
                else if (hitFraction > 0)
                    AddJudgement(new OsuJudgement { Result = HitResult.Meh });
                else
                    AddJudgement(new OsuJudgement { Result = HitResult.Miss });
            }
        }

        protected override void UpdateCurrentState(ArmedState state)
        {
            Ball.FadeIn();
            Ball.ScaleTo(HitObject.Scale);

            using (BeginDelayedSequence(slider.Duration, true))
            {
                const float fade_out_time = 450;

                // intentionally pile on an extra FadeOut to make it happen much faster.
                Ball.FadeOut(fade_out_time / 4, Easing.Out);

                switch (state)
                {
                    case ArmedState.Hit:
                        Ball.ScaleTo(HitObject.Scale * 1.4f, fade_out_time, Easing.Out);
                        break;
                }

                this.FadeOut(fade_out_time, Easing.OutQuint).Expire();
            }
        }

        public Drawable ProxiedLayer => InitialCircle.ApproachCircle;

        public override Vector2 SelectionPoint => ToScreenSpace(Body.Position);
        public override Quad SelectionQuad => Body.PathDrawQuad;
    }
}
