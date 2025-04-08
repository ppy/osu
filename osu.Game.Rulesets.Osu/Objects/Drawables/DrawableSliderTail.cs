// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Diagnostics;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Utils;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Skinning;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Osu.Objects.Drawables
{
    public partial class DrawableSliderTail : DrawableOsuHitObject
    {
        public new SliderTailCircle HitObject => (SliderTailCircle)base.HitObject;

        [CanBeNull]
        public Slider Slider => DrawableSlider?.HitObject;

        protected DrawableSlider DrawableSlider => (DrawableSlider)ParentHitObject;

        /// <summary>
        /// Whether the hit samples only play on successful hits.
        /// If <c>false</c>, the hit samples will also play on misses.
        /// </summary>
        public bool SamplePlaysOnlyOnHit { get; set; } = true;

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

            ApplyRepeatFadeIn(CirclePiece, HitObject.TimeFadeIn);
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

        protected override void CheckForResult(bool userTriggered, double timeOffset) => DrawableSlider.SliderInputManager.TryJudgeNestedObject(this, timeOffset);

        protected override void OnApply()
        {
            base.OnApply();

            if (Slider != null)
                Position = Slider.CurvePositionAt(HitObject.RepeatIndex % 2 == 0 ? 1 : 0);
        }

        #region FOR EDITOR USE ONLY, DO NOT USE FOR ANY OTHER PURPOSE

        internal void SuppressHitAnimations()
        {
            UpdateState(ArmedState.Idle);
            UpdateComboColour();

            // This method is called every frame in editor contexts, thus the lack of need for transforms.

            if (Time.Current >= HitStateUpdateTime)
            {
                // More or less matches stable (see https://github.com/peppy/osu-stable-reference/blob/bb57924c1552adbed11ee3d96cdcde47cf96f2b6/osu!/GameplayElements/HitObjects/Osu/HitCircleOsu.cs#L336-L338)
                AccentColour.Value = Color4.White;
                Alpha = Interpolation.ValueAt(Time.Current, 1f, 0f, HitStateUpdateTime, HitStateUpdateTime + 700);
            }

            LifetimeEnd = HitStateUpdateTime + 700;
        }

        internal void RestoreHitAnimations()
        {
            UpdateState(ArmedState.Hit);
            UpdateComboColour();
        }

        #endregion
    }
}
