// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osuTK;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects.Drawables.Pieces;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu.Configuration;
using osu.Game.Rulesets.Scoring;
using osuTK.Graphics;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Osu.Objects.Drawables
{
    public class DrawableSlider : DrawableOsuHitObject, IDrawableHitObjectWithProxiedApproach
    {
        private readonly Slider slider;
        private readonly List<Drawable> components = new List<Drawable>();

        public readonly DrawableHitCircle HeadCircle;
        public readonly DrawableSliderTail TailCircle;

        public readonly SnakingSliderBody Body;
        public readonly SliderBall Ball;

        private readonly IBindable<Vector2> positionBindable = new Bindable<Vector2>();
        private readonly IBindable<float> scaleBindable = new Bindable<float>();
        private readonly IBindable<SliderPath> pathBindable = new Bindable<SliderPath>();

        [Resolved(CanBeNull = true)]
        private OsuRulesetConfigManager config { get; set; }

        public DrawableSlider(Slider s)
            : base(s)
        {
            slider = s;

            Position = s.StackedPosition;

            Container<DrawableSliderTick> ticks;
            Container<DrawableRepeatPoint> repeatPoints;

            InternalChildren = new Drawable[]
            {
                Body = new SnakingSliderBody(s),
                ticks = new Container<DrawableSliderTick> { RelativeSizeAxes = Axes.Both },
                repeatPoints = new Container<DrawableRepeatPoint> { RelativeSizeAxes = Axes.Both },
                Ball = new SliderBall(s, this)
                {
                    GetInitialHitAction = () => HeadCircle.HitAction,
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
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            config?.BindWith(OsuRulesetSetting.SnakingInSliders, Body.SnakingIn);
            config?.BindWith(OsuRulesetSetting.SnakingOutSliders, Body.SnakingOut);

            positionBindable.BindValueChanged(_ => Position = HitObject.StackedPosition);
            scaleBindable.BindValueChanged(scale =>
            {
                updatePathRadius();
                Ball.Scale = new Vector2(scale.NewValue);
            });

            positionBindable.BindTo(HitObject.PositionBindable);
            scaleBindable.BindTo(HitObject.ScaleBindable);
            pathBindable.BindTo(slider.PathBindable);

            pathBindable.BindValueChanged(_ => Body.Refresh());

            AccentColour.BindValueChanged(colour =>
            {
                Body.AccentColour = colour.NewValue;

                foreach (var drawableHitObject in NestedHitObjects)
                    drawableHitObject.AccentColour.Value = colour.NewValue;
            }, true);
        }

        public readonly Bindable<bool> Tracking = new Bindable<bool>();

        protected override void Update()
        {
            base.Update();

            Tracking.Value = Ball.Tracking;

            double completionProgress = MathHelper.Clamp((Time.Current - slider.StartTime) / slider.Duration, 0, 1);

            foreach (var c in components.OfType<ISliderProgress>()) c.UpdateProgress(completionProgress);
            foreach (var c in components.OfType<ITrackSnaking>()) c.UpdateSnakingPosition(slider.Path.PositionAt(Body.SnakedStart ?? 0), slider.Path.PositionAt(Body.SnakedEnd ?? 0));
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

        public override void OnKilled()
        {
            base.OnKilled();
            Body.RecyclePath();
        }

        private float sliderPathRadius;

        protected override void SkinChanged(ISkinSource skin, bool allowFallback)
        {
            base.SkinChanged(skin, allowFallback);

            Body.BorderSize = skin.GetValue<SkinConfiguration, float?>(s => s.SliderBorderSize) ?? SliderBody.DEFAULT_BORDER_SIZE;
            sliderPathRadius = skin.GetValue<SkinConfiguration, float?>(s => s.SliderPathRadius) ?? OsuHitObject.OBJECT_RADIUS;
            updatePathRadius();

            Body.AccentColour = skin.GetValue<SkinConfiguration, Color4?>(s => s.CustomColours.ContainsKey("SliderTrackOverride") ? s.CustomColours["SliderTrackOverride"] : (Color4?)null) ?? AccentColour.Value;
            Body.BorderColour = skin.GetValue<SkinConfiguration, Color4?>(s => s.CustomColours.ContainsKey("SliderBorder") ? s.CustomColours["SliderBorder"] : (Color4?)null) ?? Color4.White;
        }

        private void updatePathRadius() => Body.PathRadius = slider.Scale * sliderPathRadius;

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

        protected override void UpdateStateTransforms(ArmedState state)
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
