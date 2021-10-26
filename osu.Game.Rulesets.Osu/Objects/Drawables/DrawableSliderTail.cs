// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Diagnostics;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Bindables;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu.Configuration;
using osu.Game.Rulesets.Osu.Skinning.Default;
using osu.Game.Skinning;
using osu.Game.Screens.Edit;
using osuTK.Graphics;
using osuTK;


namespace osu.Game.Rulesets.Osu.Objects.Drawables
{
    public class DrawableSliderTail : DrawableOsuHitObject, IRequireTracking, IHasMainCirclePiece
    {
          [Resolved]
        private OsuColour colours { get; set; }

        [Resolved(canBeNull: true)]
        private IBeatmap beatmap { get; set; }
        private readonly Bindable<bool> configTimingBasedNoteColouring = new Bindable<bool>();

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
        private void load(OsuRulesetConfigManager rulesetConfig)
        {
            Origin = Anchor.Centre;
            Size = new Vector2(OsuHitObject.OBJECT_RADIUS * 2);

            InternalChildren = new Drawable[]
            {
                scaleContainer = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                    Children = new Drawable[]
                    {
                        // no default for this; only visible in legacy skins.
                        CirclePiece = new SkinnableDrawable(new OsuSkinComponent(OsuSkinComponents.SliderTailHitCircle), _ => Empty())
                    }
                },
            };

            rulesetConfig?.BindWith(OsuRulesetSetting.TimingBasedNoteColouring, configTimingBasedNoteColouring);
            ScaleBindable.BindValueChanged(scale => scaleContainer.Scale = new Vector2(scale.NewValue));
        }
        protected override void LoadComplete()
        {
            base.LoadComplete();
            configTimingBasedNoteColouring.BindValueChanged(_ => updateSnapColour());
            StartTimeBindable.BindValueChanged(_ => updateSnapColour(), true);
        }
           protected override void OnApply()
        {
            base.OnApply();

            updateSnapColour();
            if (Slider != null)
                Position = Slider.CurvePositionAt(HitObject.RepeatIndex % 2 == 0 ? 1 : 0);
        }
        protected override void UpdateInitialTransforms()
        {
            base.UpdateInitialTransforms();

            CirclePiece.FadeInFromZero(HitObject.TimeFadeIn);
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

        protected override void CheckForResult(bool userTriggered, double timeOffset)
        {
            if (!userTriggered && timeOffset >= 0)
                ApplyResult(r => r.Type = Tracking ? r.Judgement.MaxResult : r.Judgement.MinResult);
        }
           private void updateSnapColour()
        {
            if (beatmap == null || HitObject == null) return;

            int snapDivisor = beatmap.ControlPointInfo.GetClosestBeatDivisor(HitObject.StartTime);

            AccentColour.Value = Color4.White;
        }
    }
}
