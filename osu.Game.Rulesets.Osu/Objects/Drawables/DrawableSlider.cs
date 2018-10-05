// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects.Drawables.Pieces;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Containers;
using osu.Game.Configuration;
using osu.Game.Rulesets.Scoring;
using OpenTK.Graphics;

namespace osu.Game.Rulesets.Osu.Objects.Drawables
{
    public class DrawableSlider : DrawableOsuHitObject, IDrawableHitObjectWithProxiedApproach
    {
        private readonly Slider slider;
        private readonly List<Drawable> components = new List<Drawable>();

        public readonly DrawableHitCircle HeadCircle;
        public readonly DrawableSliderTail TailCircle;

        public readonly SliderBody Body;
        public readonly SliderBall Ball;

        public DrawableSlider(Slider s)
            : base(s)
        {
            slider = s;

            Position = s.StackedPosition;

            Container<DrawableSliderTick> ticks;
            Container<DrawableRepeatPoint> repeatPoints;

            InternalChildren = new Drawable[]
            {
                Body = new SliderBody(s)
                {
                    PathWidth = s.Scale * 64,
                },
                ticks = new Container<DrawableSliderTick> { RelativeSizeAxes = Axes.Both },
                repeatPoints = new Container<DrawableRepeatPoint> { RelativeSizeAxes = Axes.Both },
                Ball = new SliderBall(s, this)
                {
                    BypassAutoSizeAxes = Axes.Both,
                    Scale = new Vector2(s.Scale),
                    AlwaysPresent = true,
                    Alpha = 0
                },
                HeadCircle = new DrawableSliderHead(s, s.HeadCircle)
                {
                    OnShake = Shake
                },
                TailCircle = new DrawableSliderTail(s, s.TailCircle)
            };

            components.Add(Body);
            components.Add(Ball);

            AddNested(HeadCircle);

            AddNested(TailCircle);
            components.Add(TailCircle);

            foreach (var tick in s.NestedHitObjects.OfType<SliderTick>())
            {
                var drawableTick = new DrawableSliderTick(tick) { Position = tick.Position - s.Position };

                ticks.Add(drawableTick);
                components.Add(drawableTick);
                AddNested(drawableTick);
            }

            foreach (var repeatPoint in s.NestedHitObjects.OfType<RepeatPoint>())
            {
                var drawableRepeatPoint = new DrawableRepeatPoint(repeatPoint, this) { Position = repeatPoint.Position - s.Position };

                repeatPoints.Add(drawableRepeatPoint);
                components.Add(drawableRepeatPoint);
                AddNested(drawableRepeatPoint);
            }

            HitObject.PositionChanged += _ => Position = HitObject.StackedPosition;
        }

        public override Color4 AccentColour
        {
            get { return base.AccentColour; }
            set
            {
                base.AccentColour = value;
                Body.AccentColour = AccentColour;
                Ball.AccentColour = AccentColour;

                foreach (var drawableHitObject in NestedHitObjects)
                    drawableHitObject.AccentColour = AccentColour;
            }
        }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            config.BindWith(OsuSetting.SnakingInSliders, Body.SnakingIn);
            config.BindWith(OsuSetting.SnakingOutSliders, Body.SnakingOut);
        }

        public bool Tracking;

        protected override void Update()
        {
            base.Update();

            Tracking = Ball.Tracking;

            double completionProgress = MathHelper.Clamp((Time.Current - slider.StartTime) / slider.Duration, 0, 1);

            foreach (var c in components.OfType<ISliderProgress>()) c.UpdateProgress(completionProgress);
            foreach (var c in components.OfType<ITrackSnaking>()) c.UpdateSnakingPosition(slider.Curve.PositionAt(Body.SnakedStart ?? 0), slider.Curve.PositionAt(Body.SnakedEnd ?? 0));
            foreach (var t in components.OfType<IRequireTracking>()) t.Tracking = Ball.Tracking;

            Size = Body.Size;
            OriginPosition = Body.PathOffset;

            if (DrawSize != Vector2.Zero)
            {
                var childAnchorPosition = Vector2.Divide(OriginPosition, DrawSize);
                foreach (var obj in NestedHitObjects)
                    obj.RelativeAnchorPosition = childAnchorPosition;
                Ball.RelativeAnchorPosition = childAnchorPosition;
            }
        }

        protected override void CheckForResult(bool userTriggered, double timeOffset)
        {
            if (userTriggered || Time.Current < slider.EndTime)
                return;

            ApplyResult(r =>
            {
                var judgementsCount = NestedHitObjects.Count();
                var judgementsHit = NestedHitObjects.Count(h => h.IsHit);

                var hitFraction = (double)judgementsHit / judgementsCount;

                if (hitFraction == 1 && HeadCircle.Result.Type == HitResult.Great)
                    r.Type = HitResult.Great;
                else if (hitFraction >= 0.5 && HeadCircle.Result.Type >= HitResult.Good)
                    r.Type = HitResult.Good;
                else if (hitFraction > 0)
                    r.Type = HitResult.Meh;
                else
                    r.Type = HitResult.Miss;
            });
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

            Expire(true);
        }

        public Drawable ProxiedLayer => HeadCircle.ApproachCircle;

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => Body.ReceivePositionalInputAt(screenSpacePos);
    }
}
