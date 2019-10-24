// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osuTK;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects.Drawables.Pieces;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu.Configuration;
using osu.Game.Rulesets.Osu.Skinning;
using osu.Game.Rulesets.Scoring;
using osuTK.Graphics;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Osu.Objects.Drawables
{
    public class DrawableSlider : DrawableOsuHitObject, IDrawableHitObjectWithProxiedApproach
    {
        public DrawableSliderHead HeadCircle => headContainer.Child;
        public DrawableSliderTail TailCircle => tailContainer.Child;

        public readonly SnakingSliderBody Body;
        public readonly SliderBall Ball;

        private readonly Container<DrawableSliderHead> headContainer;
        private readonly Container<DrawableSliderTail> tailContainer;
        private readonly Container<DrawableSliderTick> tickContainer;
        private readonly Container<DrawableRepeatPoint> repeatContainer;

        private readonly Slider slider;

        private readonly IBindable<Vector2> positionBindable = new Bindable<Vector2>();
        private readonly IBindable<int> stackHeightBindable = new Bindable<int>();
        private readonly IBindable<float> scaleBindable = new Bindable<float>();
        private readonly IBindable<SliderPath> pathBindable = new Bindable<SliderPath>();

        [Resolved(CanBeNull = true)]
        private OsuRulesetConfigManager config { get; set; }

        public DrawableSlider(Slider s)
            : base(s)
        {
            slider = s;

            Position = s.StackedPosition;

            InternalChildren = new Drawable[]
            {
                Body = new SnakingSliderBody(s),
                tickContainer = new Container<DrawableSliderTick> { RelativeSizeAxes = Axes.Both },
                repeatContainer = new Container<DrawableRepeatPoint> { RelativeSizeAxes = Axes.Both },
                Ball = new SliderBall(s, this)
                {
                    GetInitialHitAction = () => HeadCircle.HitAction,
                    BypassAutoSizeAxes = Axes.Both,
                    Scale = new Vector2(s.Scale),
                    AlwaysPresent = true,
                    Alpha = 0
                },
                headContainer = new Container<DrawableSliderHead> { RelativeSizeAxes = Axes.Both },
                tailContainer = new Container<DrawableSliderTail> { RelativeSizeAxes = Axes.Both },
            };
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            config?.BindWith(OsuRulesetSetting.SnakingInSliders, Body.SnakingIn);
            config?.BindWith(OsuRulesetSetting.SnakingOutSliders, Body.SnakingOut);

            positionBindable.BindValueChanged(_ => Position = HitObject.StackedPosition);
            stackHeightBindable.BindValueChanged(_ => Position = HitObject.StackedPosition);
            scaleBindable.BindValueChanged(scale =>
            {
                updatePathRadius();
                Ball.Scale = new Vector2(scale.NewValue);
            });

            positionBindable.BindTo(HitObject.PositionBindable);
            stackHeightBindable.BindTo(HitObject.StackHeightBindable);
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

        protected override void AddNestedHitObject(DrawableHitObject hitObject)
        {
            base.AddNestedHitObject(hitObject);

            switch (hitObject)
            {
                case DrawableSliderHead head:
                    headContainer.Child = head;
                    break;

                case DrawableSliderTail tail:
                    tailContainer.Child = tail;
                    break;

                case DrawableSliderTick tick:
                    tickContainer.Add(tick);
                    break;

                case DrawableRepeatPoint repeat:
                    repeatContainer.Add(repeat);
                    break;
            }
        }

        protected override void ClearNestedHitObjects()
        {
            base.ClearNestedHitObjects();

            headContainer.Clear();
            tailContainer.Clear();
            repeatContainer.Clear();
            tickContainer.Clear();
        }

        protected override DrawableHitObject CreateNestedHitObject(HitObject hitObject)
        {
            switch (hitObject)
            {
                case SliderTailCircle tail:
                    return new DrawableSliderTail(slider, tail);

                case HitCircle head:
                    return new DrawableSliderHead(slider, head) { OnShake = Shake };

                case SliderTick tick:
                    return new DrawableSliderTick(tick) { Position = tick.Position - slider.Position };

                case RepeatPoint repeat:
                    return new DrawableRepeatPoint(repeat, this) { Position = repeat.Position - slider.Position };
            }

            return base.CreateNestedHitObject(hitObject);
        }

        protected override void UpdateInitialTransforms()
        {
            base.UpdateInitialTransforms();

            Body.FadeInFromZero(HitObject.TimeFadeIn);
        }

        public readonly Bindable<bool> Tracking = new Bindable<bool>();

        protected override void Update()
        {
            base.Update();

            Tracking.Value = Ball.Tracking;

            double completionProgress = MathHelper.Clamp((Time.Current - slider.StartTime) / slider.Duration, 0, 1);

            Ball.UpdateProgress(completionProgress);
            Body.UpdateProgress(completionProgress);

            foreach (DrawableHitObject hitObject in NestedHitObjects)
            {
                if (hitObject is ITrackSnaking s) s.UpdateSnakingPosition(slider.Path.PositionAt(Body.SnakedStart ?? 0), slider.Path.PositionAt(Body.SnakedEnd ?? 0));
                if (hitObject is IRequireTracking t) t.Tracking = Ball.Tracking;
            }

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

        protected override void ApplySkin(ISkinSource skin, bool allowFallback)
        {
            base.ApplySkin(skin, allowFallback);

            Body.BorderSize = skin.GetConfig<OsuSkinConfiguration, float>(OsuSkinConfiguration.SliderBorderSize)?.Value ?? SliderBody.DEFAULT_BORDER_SIZE;
            sliderPathRadius = skin.GetConfig<OsuSkinConfiguration, float>(OsuSkinConfiguration.SliderPathRadius)?.Value ?? OsuHitObject.OBJECT_RADIUS;
            updatePathRadius();

            Body.AccentColour = skin.GetConfig<OsuSkinColour, Color4>(OsuSkinColour.SliderTrackOverride)?.Value ?? AccentColour.Value;
            Body.BorderColour = skin.GetConfig<OsuSkinColour, Color4>(OsuSkinColour.SliderBorder)?.Value ?? Color4.White;

            bool allowBallTint = skin.GetConfig<OsuSkinConfiguration, bool>(OsuSkinConfiguration.AllowSliderBallTint)?.Value ?? false;
            Ball.Colour = allowBallTint ? AccentColour.Value : Color4.White;
        }

        private void updatePathRadius() => Body.PathRadius = slider.Scale * sliderPathRadius;

        protected override void CheckForResult(bool userTriggered, double timeOffset)
        {
            if (userTriggered || Time.Current < slider.EndTime)
                return;

            ApplyResult(r =>
            {
                var judgementsCount = NestedHitObjects.Count;
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
            base.UpdateStateTransforms(state);

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

                this.FadeOut(fade_out_time, Easing.OutQuint);
            }
        }

        public Drawable ProxiedLayer => HeadCircle.ProxiedLayer;

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => Body.ReceivePositionalInputAt(screenSpacePos);
    }
}
