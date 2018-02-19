// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
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
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Osu.Objects.Drawables
{
    public class DrawableSlider : DrawableOsuHitObject, IDrawableHitObjectWithProxiedApproach
    {
        private readonly Slider slider;
        private readonly List<Drawable> components = new List<Drawable>();

        public readonly DrawableHitCircle HeadCircle;
        public readonly SliderBody Body;
        public readonly SliderBall Ball;

        public DrawableSlider(Slider s)
            : base(s)
        {
            slider = s;

            DrawableSliderTail tail;
            Container<DrawableSliderTick> ticks;
            Container<DrawableRepeatPoint> repeatPoints;

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
                HeadCircle = new DrawableHitCircle(s.HeadCircle),
                tail = new DrawableSliderTail(s.TailCircle)
            };

            components.Add(Body);
            components.Add(Ball);

            AddNested(HeadCircle);

            AddNested(tail);
            components.Add(tail);

            foreach (var tick in s.NestedHitObjects.OfType<SliderTick>())
            {
                var drawableTick = new DrawableSliderTick(tick)
                {
                    Position = tick.Position
                };

                ticks.Add(drawableTick);
                components.Add(drawableTick);
                AddNested(drawableTick);
            }

            foreach (var repeatPoint in s.NestedHitObjects.OfType<RepeatPoint>())
            {
                var drawableRepeatPoint = new DrawableRepeatPoint(repeatPoint, this)
                {
                    Position = repeatPoint.Position
                };

                repeatPoints.Add(drawableRepeatPoint);
                components.Add(drawableRepeatPoint);
                AddNested(drawableRepeatPoint);
            }
        }

        private int currentSpan;
        public bool Tracking;

        protected override void Update()
        {
            base.Update();

            Tracking = Ball.Tracking;

            double progress = MathHelper.Clamp((Time.Current - slider.StartTime) / slider.Duration, 0, 1);

            int span = slider.SpanAt(progress);
            progress = slider.ProgressAt(progress);

            if (span > currentSpan)
                currentSpan = span;

            //todo: we probably want to reconsider this before adding scoring, but it looks and feels nice.
            if (!HeadCircle.IsHit)
                HeadCircle.Position = slider.Curve.PositionAt(progress);

            foreach (var c in components.OfType<ISliderProgress>()) c.UpdateProgress(progress, span);
            foreach (var c in components.OfType<ITrackSnaking>()) c.UpdateSnakingPosition(slider.Curve.PositionAt(Body.SnakedStart ?? 0), slider.Curve.PositionAt(Body.SnakedEnd ?? 0));
            foreach (var t in components.OfType<IRequireTracking>()) t.Tracking = Ball.Tracking;
        }

        protected override void CheckForJudgements(bool userTriggered, double timeOffset)
        {
            if (!userTriggered && Time.Current >= slider.EndTime)
            {
                var judgementsCount = NestedHitObjects.Count;
                var judgementsHit = NestedHitObjects.Count(h => h.IsHit);

                var hitFraction = (double)judgementsHit / judgementsCount;
                if (hitFraction == 1 && HeadCircle.Judgements.Any(j => j.Result == HitResult.Great))
                    AddJudgement(new OsuJudgement { Result = HitResult.Great });
                else if (hitFraction >= 0.5 && HeadCircle.Judgements.Any(j => j.Result >= HitResult.Good))
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

        public Drawable ProxiedLayer => HeadCircle.ApproachCircle;

        public override bool ReceiveMouseInputAt(Vector2 screenSpacePos) => Body.ReceiveMouseInputAt(screenSpacePos);

        public override Vector2 SelectionPoint => ToScreenSpace(Body.Position);
        public override Quad SelectionQuad => Body.PathDrawQuad;
    }
}
