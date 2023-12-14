// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Audio;
using osu.Game.Graphics.Containers;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Skinning.Default;
using osu.Game.Rulesets.Scoring;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Rulesets.Osu.Objects.Drawables
{
    public partial class DrawableSlider : DrawableOsuHitObject
    {
        public new Slider HitObject => (Slider)base.HitObject;

        public DrawableSliderHead HeadCircle => headContainer.Child;
        public DrawableSliderTail TailCircle => tailContainer.Child;
        public IEnumerable<DrawableSliderTick> Ticks => tickContainer.Children;
        public IEnumerable<DrawableSliderRepeat> Repeats => repeatContainer.Children;

        [Cached]
        public DrawableSliderBall Ball { get; private set; }

        public SkinnableDrawable Body { get; private set; }

        private ShakeContainer shakeContainer;

        protected override IEnumerable<Drawable> DimmablePieces => new Drawable[]
        {
            HeadCircle,
            TailCircle,
            repeatContainer,
            Body,
        };

        /// <summary>
        /// A target container which can be used to add top level elements to the slider's display.
        /// Intended to be used for proxy purposes only.
        /// </summary>
        public Container OverlayElementContainer { get; private set; }

        public override bool DisplayResult => HitObject.ClassicSliderBehaviour;

        [CanBeNull]
        public PlaySliderBody SliderBody => Body.Drawable as PlaySliderBody;

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
            Ball = new DrawableSliderBall
            {
                GetInitialHitAction = () => HeadCircle.HitAction,
                BypassAutoSizeAxes = Axes.Both,
                AlwaysPresent = true,
                Alpha = 0
            };
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            tailContainer = new Container<DrawableSliderTail> { RelativeSizeAxes = Axes.Both };

            AddRangeInternal(new Drawable[]
            {
                shakeContainer = new ShakeContainer
                {
                    ShakeDuration = 30,
                    RelativeSizeAxes = Axes.Both,
                    Children = new[]
                    {
                        Body = new SkinnableDrawable(new OsuSkinComponentLookup(OsuSkinComponents.SliderBody), _ => new DefaultSliderBody(), confineMode: ConfineMode.NoScaling),
                        // proxied here so that the tail is drawn under repeats/ticks - legacy skins rely on this
                        tailContainer.CreateProxy(),
                        tickContainer = new Container<DrawableSliderTick> { RelativeSizeAxes = Axes.Both },
                        repeatContainer = new Container<DrawableSliderRepeat> { RelativeSizeAxes = Axes.Both },
                        // actual tail container is placed here to ensure that tail hitobjects are processed after ticks/repeats.
                        // this is required for the correct operation of Score V2.
                        tailContainer,
                    }
                },
                // slider head is not included in shake as it handles hit detection, and handles its own shaking.
                headContainer = new Container<DrawableSliderHead> { RelativeSizeAxes = Axes.Both },
                OverlayElementContainer = new Container { RelativeSizeAxes = Axes.Both, },
                Ball,
                slidingSample = new PausableSkinnableSound
                {
                    Looping = true,
                    MinimumSampleVolume = MINIMUM_SAMPLE_VOLUME,
                }
            });

            PositionBindable.BindValueChanged(_ => Position = HitObject.StackedPosition);
            StackHeightBindable.BindValueChanged(_ => Position = HitObject.StackedPosition);
            ScaleBindable.BindValueChanged(scale => Ball.Scale = new Vector2(scale.NewValue));

            AccentColour.BindValueChanged(colour =>
            {
                foreach (var drawableHitObject in NestedHitObjects)
                    drawableHitObject.AccentColour.Value = colour.NewValue;
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

        public override void Shake() => shakeContainer.Shake();

        protected override void OnFree()
        {
            base.OnFree();

            PathVersion.UnbindFrom(HitObject.Path.Version);

            slidingSample?.ClearSamples();
        }

        protected override void LoadSamples()
        {
            // Note: base.LoadSamples() isn't called since the slider plays the tail's hitsounds for the time being.

            Samples.Samples = HitObject.TailSamples.Cast<ISampleInfo>().ToArray();
            slidingSample.Samples = HitObject.CreateSlidingSamples().Cast<ISampleInfo>().ToArray();
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

            OverlayElementContainer.Clear(false);
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
                slidingSample.Balance.Value = CalculateSamplePlaybackBalance(CalculateDrawableRelativePosition(Ball));

            double completionProgress = Math.Clamp((Time.Current - HitObject.StartTime) / HitObject.Duration, 0, 1);

            Ball.UpdateProgress(completionProgress);
            SliderBody?.UpdateProgress(HeadCircle.IsHit ? completionProgress : 0);

            foreach (DrawableHitObject hitObject in NestedHitObjects)
            {
                if (hitObject is ITrackSnaking s) s.UpdateSnakingPosition(HitObject.Path.PositionAt(SliderBody?.SnakedStart ?? 0), HitObject.Path.PositionAt(SliderBody?.SnakedEnd ?? 0));
                if (hitObject is IRequireTracking t) t.Tracking = Ball.Tracking;
            }

            Size = SliderBody?.Size ?? Vector2.Zero;
            OriginPosition = SliderBody?.PathOffset ?? Vector2.Zero;

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
            SliderBody?.RecyclePath();
        }

        protected override void CheckForResult(bool userTriggered, double timeOffset)
        {
            if (userTriggered || !TailCircle.Judged || Time.Current < HitObject.EndTime)
                return;

            if (HitObject.ClassicSliderBehaviour)
            {
                // Classic behaviour means a slider is judged proportionally to the number of nested hitobjects hit. This is the classic osu!stable scoring.
                ApplyResult(r =>
                {
                    int totalTicks = NestedHitObjects.Count;
                    int hitTicks = NestedHitObjects.Count(h => h.IsHit);

                    if (hitTicks == totalTicks)
                        r.Type = HitResult.Great;
                    else if (hitTicks == 0)
                        r.Type = HitResult.Miss;
                    else
                    {
                        double hitFraction = (double)hitTicks / totalTicks;
                        r.Type = hitFraction >= 0.5 ? HitResult.Ok : HitResult.Meh;
                    }
                });
            }
            else
            {
                // If only the nested hitobjects are judged, then the slider's own judgement is ignored for scoring purposes.
                // But the slider needs to still be judged with a reasonable hit/miss result for visual purposes (hit/miss transforms, etc).
                ApplyResult(r => r.Type = NestedHitObjects.Any(h => h.Result.IsHit) ? r.Judgement.MaxResult : r.Judgement.MinResult);
            }
        }

        public override void PlaySamples()
        {
            // rather than doing it this way, we should probably attach the sample to the tail circle.
            // this can only be done if we stop using LastTick.
            if (!TailCircle.SamplePlaysOnlyOnHit || TailCircle.IsHit)
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

            const float fade_out_time = 240;

            switch (state)
            {
                case ArmedState.Hit:
                    if (HeadCircle.IsHit && SliderBody?.SnakingOut.Value == true)
                        Body.FadeOut(40); // short fade to allow for any body colour to smoothly disappear.
                    break;
            }

            this.FadeOut(fade_out_time).Expire();
        }

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => SliderBody?.ReceivePositionalInputAt(screenSpacePos) ?? base.ReceivePositionalInputAt(screenSpacePos);

        private partial class DefaultSliderBody : PlaySliderBody
        {
        }
    }
}
