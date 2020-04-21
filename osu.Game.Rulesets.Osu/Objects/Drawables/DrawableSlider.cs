// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osuTK;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects.Drawables.Pieces;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Objects;
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

        public readonly SliderBall Ball;
        public readonly SkinnableDrawable Body;

        public override bool DisplayResult => false;

        private PlaySliderBody sliderBody => Body.Drawable as PlaySliderBody;

        private readonly Container<DrawableSliderHead> headContainer;
        private readonly Container<DrawableSliderTail> tailContainer;
        private readonly Container<DrawableSliderTick> tickContainer;
        private readonly Container<DrawableSliderRepeat> repeatContainer;

        private readonly Slider slider;

        private readonly IBindable<Vector2> positionBindable = new Bindable<Vector2>();
        private readonly IBindable<int> stackHeightBindable = new Bindable<int>();
        private readonly IBindable<float> scaleBindable = new BindableFloat();

        public DrawableSlider(Slider s)
            : base(s)
        {
            slider = s;

            Position = s.StackedPosition;

            InternalChildren = new Drawable[]
            {
                Body = new SkinnableDrawable(new OsuSkinComponent(OsuSkinComponents.SliderBody), _ => new DefaultSliderBody(), confineMode: ConfineMode.NoScaling),
                tickContainer = new Container<DrawableSliderTick> { RelativeSizeAxes = Axes.Both },
                repeatContainer = new Container<DrawableSliderRepeat> { RelativeSizeAxes = Axes.Both },
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
            positionBindable.BindValueChanged(_ => Position = HitObject.StackedPosition);
            stackHeightBindable.BindValueChanged(_ => Position = HitObject.StackedPosition);
            scaleBindable.BindValueChanged(scale => Ball.Scale = new Vector2(scale.NewValue));

            positionBindable.BindTo(HitObject.PositionBindable);
            stackHeightBindable.BindTo(HitObject.StackHeightBindable);
            scaleBindable.BindTo(HitObject.ScaleBindable);

            AccentColour.BindValueChanged(colour =>
            {
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

                case DrawableSliderRepeat repeat:
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

                case SliderHeadCircle head:
                    return new DrawableSliderHead(slider, head)
                    {
                        OnShake = Shake,
                        CheckHittable = (d, t) => CheckHittable?.Invoke(d, t) ?? true
                    };

                case SliderTick tick:
                    return new DrawableSliderTick(tick) { Position = tick.Position - slider.Position };

                case SliderRepeat repeat:
                    return new DrawableSliderRepeat(repeat, this) { Position = repeat.Position - slider.Position };
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

            double completionProgress = Math.Clamp((Time.Current - slider.StartTime) / slider.Duration, 0, 1);

            Ball.UpdateProgress(completionProgress);
            sliderBody?.UpdateProgress(completionProgress);

            foreach (DrawableHitObject hitObject in NestedHitObjects)
            {
                if (hitObject is ITrackSnaking s) s.UpdateSnakingPosition(slider.Path.PositionAt(sliderBody?.SnakedStart ?? 0), slider.Path.PositionAt(sliderBody?.SnakedEnd ?? 0));
                if (hitObject is IRequireTracking t) t.Tracking = Ball.Tracking;
            }

            Size = sliderBody?.Size ?? Vector2.Zero;
            OriginPosition = sliderBody?.PathOffset ?? Vector2.Zero;

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
            sliderBody?.RecyclePath();
        }

        protected override void ApplySkin(ISkinSource skin, bool allowFallback)
        {
            base.ApplySkin(skin, allowFallback);

            bool allowBallTint = skin.GetConfig<OsuSkinConfiguration, bool>(OsuSkinConfiguration.AllowSliderBallTint)?.Value ?? false;
            Ball.AccentColour = allowBallTint ? AccentColour.Value : Color4.White;
        }

        protected override void CheckForResult(bool userTriggered, double timeOffset)
        {
            if (userTriggered || Time.Current < slider.EndTime)
                return;

            ApplyResult(r => r.Type = r.Judgement.MaxResult);
        }

        public override void PlaySamples()
        {
            // rather than doing it this way, we should probably attach the sample to the tail circle.
            // this can only be done after we stop using LegacyLastTick.
            if (TailCircle.Result.Type != HitResult.Miss)
                base.PlaySamples();
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

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => sliderBody?.ReceivePositionalInputAt(screenSpacePos) ?? base.ReceivePositionalInputAt(screenSpacePos);

        private class DefaultSliderBody : PlaySliderBody
        {
        }
    }
}
