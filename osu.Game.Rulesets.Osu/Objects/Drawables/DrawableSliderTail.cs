// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Rulesets.Osu.Objects.Drawables
{
    public partial class DrawableSliderTail : DrawableOsuHitObject, IRequireTracking
    {
        public new SliderTailCircle HitObject => (SliderTailCircle)base.HitObject;

        [CanBeNull]
        public Slider Slider => DrawableSlider?.HitObject;

        protected DrawableSlider DrawableSlider => (DrawableSlider)ParentHitObject;

        /// <summary>
        /// The judgement text is provided by the <see cref="DrawableSlider"/>.
        /// </summary>
        public override bool DisplayResult => false;

        /// <summary>
        /// Whether the hit samples only play on successful hits.
        /// If <c>false</c>, the hit samples will also play on misses.
        /// </summary>
        public bool SamplePlaysOnlyOnHit { get; set; } = true;

        public bool Tracking { get; set; }

        public SkinnableDrawable CirclePiece { get; private set; }

        private Container scaleContainer;

        public DrawableSliderTail()
            : base(null)
        {
        }

        public DrawableSliderTail(SliderTailCircle tailCircle)
            : base(tailCircle)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Origin = Anchor.Centre;
            Size = OsuHitObject.OBJECT_DIMENSIONS;

            AddRangeInternal(new Drawable[]
            {
                scaleContainer = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                    Children = new Drawable[]
                    {
                        // no default for this; only visible in legacy skins.
                        CirclePiece = new SkinnableDrawable(new OsuSkinComponentLookup(OsuSkinComponents.SliderTailHitCircle), _ => Empty())
                    }
                },
            });

            ScaleBindable.BindValueChanged(scale => scaleContainer.Scale = new Vector2(scale.NewValue));
        }

        protected override void LoadSamples()
        {
            // Tail models don't actually get samples, as the playback is handled by DrawableSlider.
            // This override is only here for visibility in explaining this weird flow.
        }

        public override void PlaySamples()
        {
            // Tail models don't actually get samples, as the playback is handled by DrawableSlider.
            // This override is only here for visibility in explaining this weird flow.
        }

        protected override void UpdateInitialTransforms()
        {
            base.UpdateInitialTransforms();

            // When snaking in is enabled, the first end circle needs to be delayed until the snaking completes.
            bool delayFadeIn = DrawableSlider.SliderBody?.SnakingIn.Value == true && HitObject.RepeatIndex == 0;

            CirclePiece
                .FadeOut()
                .Delay(delayFadeIn ? (Slider?.TimePreempt ?? 0) / 3 : 0)
                .FadeIn(HitObject.TimeFadeIn);
        }

        protected override void UpdateHitStateTransforms(ArmedState state)
        {
            base.UpdateHitStateTransforms(state);

            Debug.Assert(HitObject.HitWindows != null);

            switch (state)
            {
                case ArmedState.Idle:
                    this.Delay(HitObject.TimePreempt).FadeOut(500);
                    break;

                case ArmedState.Miss:
                    this.FadeOut(100);
                    break;

                case ArmedState.Hit:
                    // todo: temporary / arbitrary
                    this.Delay(800).FadeOut();
                    break;
            }
        }

        public void HitForcefully()
        {
            if (Judged)
                return;

            ApplyResult(r => r.Type = r.Judgement.MaxResult);
        }

        protected override void CheckForResult(bool userTriggered, double timeOffset)
        {
            if (userTriggered)
                return;

            // Ensure the tail can only activate after all previous ticks/repeats already have.
            //
            // This covers the edge case where the lenience may allow the tail to activate before
            // the last tick, changing ordering of score/combo awarding.
            var lastTick = DrawableSlider.NestedHitObjects.LastOrDefault(o => o.HitObject is SliderTick || o.HitObject is SliderRepeat);
            if (lastTick?.Judged == false)
                return;

            if (timeOffset < SliderEventGenerator.TAIL_LENIENCY)
                return;

            // This check is applied to all nested slider objects apart from the head (ticks, repeats, tail).
            if (!DrawableSlider.HeadCircle.Judged)
            {
                if (Tracking)
                {
                    // Attempt to preserve correct ordering of judgements as best we can by forcing an un-judged head to be missed when the user has clearly skipped it.
                    DrawableSlider.HeadCircle.MissForcefully();
                }
                else
                {
                    // Don't judge this object as a miss before the head has been judged, to allow the head to be hit late.
                    return;
                }
            }

            // The player needs to have engaged in tracking at any point after the tail leniency cutoff.
            // An actual tick miss should only occur if reaching the tick itself.
            if (Tracking)
                ApplyResult(r => r.Type = r.Judgement.MaxResult);
            else if (timeOffset > 0)
                ApplyResult(r => r.Type = r.Judgement.MinResult);
        }

        protected override void OnApply()
        {
            base.OnApply();

            if (Slider != null)
                Position = Slider.CurvePositionAt(HitObject.RepeatIndex % 2 == 0 ? 1 : 0);
        }
    }
}
