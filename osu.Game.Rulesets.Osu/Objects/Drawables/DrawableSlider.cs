// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using JetBrains.Annotations;
using osuTK;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Containers;
using osu.Game.Audio;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu.Skinning;
using osu.Game.Rulesets.Osu.Skinning.Default;
using osu.Game.Rulesets.Osu.UI;
using osuTK.Graphics;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Osu.Objects.Drawables
{
    public class DrawableSlider : DrawableOsuHitObject
    {
        public new Slider HitObject => (Slider)base.HitObject;

        public DrawableSliderHead HeadCircle => headContainer.Child;
        public DrawableSliderTail TailCircle => tailContainer.Child;

        public SliderBall Ball { get; private set; }
        public SkinnableDrawable Body { get; private set; }

        public override bool DisplayResult => false;

        private PlaySliderBody sliderBody => Body.Drawable as PlaySliderBody;

        public IBindable<int> PathVersion => pathVersion;
        private readonly Bindable<int> pathVersion = new Bindable<int>();

        private Container<DrawableSliderHead> headContainer;
        private Container<DrawableSliderTail> tailContainer;
        private Container<DrawableSliderTick> tickContainer;
        private Container<DrawableSliderRepeat> repeatContainer;
        private PausableSkinnableSound slidingSample;

        public DrawableSlider()
            : this(null)
        {
        }

        public DrawableSlider([CanBeNull] Slider s = null)
            : base(s)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChildren = new Drawable[]
            {
                Body = new SkinnableDrawable(new OsuSkinComponent(OsuSkinComponents.SliderBody), _ => new DefaultSliderBody(), confineMode: ConfineMode.NoScaling),
                tailContainer = new Container<DrawableSliderTail> { RelativeSizeAxes = Axes.Both },
                tickContainer = new Container<DrawableSliderTick> { RelativeSizeAxes = Axes.Both },
                repeatContainer = new Container<DrawableSliderRepeat> { RelativeSizeAxes = Axes.Both },
                Ball = new SliderBall(this)
                {
                    GetInitialHitAction = () => HeadCircle.HitAction,
                    BypassAutoSizeAxes = Axes.Both,
                    AlwaysPresent = true,
                    Alpha = 0
                },
                headContainer = new Container<DrawableSliderHead> { RelativeSizeAxes = Axes.Both },
                slidingSample = new PausableSkinnableSound { Looping = true }
            };

            PositionBindable.BindValueChanged(_ => Position = HitObject.StackedPosition);
            StackHeightBindable.BindValueChanged(_ => Position = HitObject.StackedPosition);
            ScaleBindable.BindValueChanged(scale => Ball.Scale = new Vector2(scale.NewValue));

            AccentColour.BindValueChanged(colour =>
            {
                foreach (var drawableHitObject in NestedHitObjects)
                    drawableHitObject.AccentColour.Value = colour.NewValue;
                updateBallTint();
            }, true);

            Tracking.BindValueChanged(updateSlidingSample);
        }

        protected override void OnApply()
        {
            base.OnApply();

            // Ensure that the version will change after the upcoming BindTo().
            pathVersion.Value = int.MaxValue;
            PathVersion.BindTo(HitObject.Path.Version);
        }

        protected override void OnFree()
        {
            base.OnFree();

            PathVersion.UnbindFrom(HitObject.Path.Version);

            slidingSample.Samples = null;
        }

        protected override void LoadSamples()
        {
            base.LoadSamples();

            var firstSample = HitObject.Samples.FirstOrDefault();

            if (firstSample != null)
            {
                var clone = HitObject.SampleControlPoint.ApplyTo(firstSample).With("sliderslide");

                slidingSample.Samples = new ISampleInfo[] { clone };
            }
        }

        public override void StopAllSamples()
        {
            base.StopAllSamples();
            slidingSample?.Stop();
        }

        private void updateSlidingSample(ValueChangedEvent<bool> tracking)
        {
            if (tracking.NewValue)
                slidingSample?.Play();
            else
                slidingSample?.Stop();
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

            headContainer.Clear(false);
            tailContainer.Clear(false);
            repeatContainer.Clear(false);
            tickContainer.Clear(false);
        }

        protected override DrawableHitObject CreateNestedHitObject(HitObject hitObject)
        {
            switch (hitObject)
            {
                case SliderTailCircle tail:
                    return new DrawableSliderTail(tail);

                case SliderHeadCircle head:
                    return new DrawableSliderHead(head);

                case SliderTick tick:
                    return new DrawableSliderTick(tick);

                case SliderRepeat repeat:
                    return new DrawableSliderRepeat(repeat);
            }

            return base.CreateNestedHitObject(hitObject);
        }

        public readonly Bindable<bool> Tracking = new Bindable<bool>();

        protected override void Update()
        {
            base.Update();

            Tracking.Value = Ball.Tracking;

            if (Tracking.Value && slidingSample != null)
                // keep the sliding sample playing at the current tracking position
                slidingSample.Balance.Value = CalculateSamplePlaybackBalance(Ball.X / OsuPlayfield.BASE_SIZE.X);

            double completionProgress = Math.Clamp((Time.Current - HitObject.StartTime) / HitObject.Duration, 0, 1);

            Ball.UpdateProgress(completionProgress);
            sliderBody?.UpdateProgress(completionProgress);

            foreach (DrawableHitObject hitObject in NestedHitObjects)
            {
                if (hitObject is ITrackSnaking s) s.UpdateSnakingPosition(HitObject.Path.PositionAt(sliderBody?.SnakedStart ?? 0), HitObject.Path.PositionAt(sliderBody?.SnakedEnd ?? 0));
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

            updateBallTint();
        }

        private void updateBallTint()
        {
            if (CurrentSkin == null)
                return;

            bool allowBallTint = CurrentSkin.GetConfig<OsuSkinConfiguration, bool>(OsuSkinConfiguration.AllowSliderBallTint)?.Value ?? false;
            Ball.AccentColour = allowBallTint ? AccentColour.Value : Color4.White;
        }

        protected override void CheckForResult(bool userTriggered, double timeOffset)
        {
            if (userTriggered || Time.Current < HitObject.EndTime)
                return;

            ApplyResult(r => r.Type = NestedHitObjects.Any(h => h.Result.IsHit) ? r.Judgement.MaxResult : r.Judgement.MinResult);
        }

        public override void PlaySamples()
        {
            // rather than doing it this way, we should probably attach the sample to the tail circle.
            // this can only be done after we stop using LegacyLastTick.
            if (TailCircle.IsHit)
                base.PlaySamples();
        }

        protected override void UpdateInitialTransforms()
        {
            base.UpdateInitialTransforms();

            Body.FadeInFromZero(HitObject.TimeFadeIn);
        }

        protected override void UpdateStartTimeStateTransforms()
        {
            base.UpdateStartTimeStateTransforms();

            Ball.FadeIn();
            Ball.ScaleTo(HitObject.Scale);
        }

        protected override void UpdateHitStateTransforms(ArmedState state)
        {
            base.UpdateHitStateTransforms(state);

            const float fade_out_time = 450;

            // intentionally pile on an extra FadeOut to make it happen much faster.
            Ball.FadeOut(fade_out_time / 4, Easing.Out);

            switch (state)
            {
                case ArmedState.Hit:
                    Ball.ScaleTo(HitObject.Scale * 1.4f, fade_out_time, Easing.Out);
                    if (sliderBody?.SnakingOut.Value == true)
                        Body.FadeOut(40); // short fade to allow for any body colour to smoothly disappear.
                    break;
            }

            this.FadeOut(fade_out_time, Easing.OutQuint).Expire();
        }

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => sliderBody?.ReceivePositionalInputAt(screenSpacePos) ?? base.ReceivePositionalInputAt(screenSpacePos);

        private class DefaultSliderBody : PlaySliderBody
        {
        }
    }
}
