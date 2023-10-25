// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Globalization;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Rulesets.Osu.Skinning.Legacy
{
    public abstract partial class LegacySpinner : CompositeDrawable, IHasApproachCircle
    {
        public const float SPRITE_SCALE = 0.625f;

        /// <remarks>
        /// All constants are in osu!stable's gamefield space, which is shifted 16px downwards.
        /// This offset is negated to bring all constants into window-space.
        /// Note: SPINNER_Y_CENTRE + SPINNER_TOP_OFFSET - Position.Y = 240 (=480/2, or half the window-space in osu!stable)
        /// </remarks>
        protected const float SPINNER_TOP_OFFSET = 45f - 16f;

        protected const float SPINNER_Y_CENTRE = SPINNER_TOP_OFFSET + 219f;

        private const float spm_hide_offset = 50f;

        protected DrawableSpinner DrawableSpinner { get; private set; } = null!;

        public Drawable? ApproachCircle { get; protected set; }

        private Sprite spin = null!;
        private Sprite clear = null!;

        private LegacySpriteText bonusCounter = null!;

        private Sprite spmBackground = null!;
        private LegacySpriteText spmCounter = null!;

        [BackgroundDependencyLoader]
        private void load(DrawableHitObject drawableHitObject, ISkinSource source)
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            // osu!stable positions spinner components in window-space (as opposed to gamefield-space). This is a 640x480 area taking up the entire screen.
            // In lazer, the gamefield-space positional transformation is applied in OsuPlayfieldAdjustmentContainer, which is inverted here to make this area take up the entire window space.
            Size = new Vector2(640, 480);
            Position = new Vector2(0, -8f);

            DrawableSpinner = (DrawableSpinner)drawableHitObject;

            AddInternal(new Container
            {
                Depth = float.MinValue,
                RelativeSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    spin = new Sprite
                    {
                        Alpha = 0,
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.Centre,
                        Texture = source.GetTexture("spinner-spin"),
                        Scale = new Vector2(SPRITE_SCALE),
                        Y = SPINNER_TOP_OFFSET + 335,
                    },
                    clear = new Sprite
                    {
                        Alpha = 0,
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.Centre,
                        Texture = source.GetTexture("spinner-clear"),
                        Scale = new Vector2(SPRITE_SCALE),
                        Y = SPINNER_TOP_OFFSET + 115,
                    },
                    bonusCounter = new LegacySpriteText(LegacyFont.Score)
                    {
                        Alpha = 0,
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.Centre,
                        Scale = new Vector2(SPRITE_SCALE),
                        Y = SPINNER_TOP_OFFSET + 299,
                    }.With(s => s.Font = s.Font.With(fixedWidth: false)),
                    spmBackground = new Sprite
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopLeft,
                        Texture = source.GetTexture("spinner-rpm"),
                        Scale = new Vector2(SPRITE_SCALE),
                        Position = new Vector2(-87, 445 + spm_hide_offset),
                    },
                    spmCounter = new LegacySpriteText(LegacyFont.Score)
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopRight,
                        Scale = new Vector2(SPRITE_SCALE * 0.9f),
                        Position = new Vector2(80, 448 + spm_hide_offset),
                    }.With(s => s.Font = s.Font.With(fixedWidth: false)),
                }
            });
        }

        private IBindable<int> completedSpins = null!;
        private IBindable<double> spinsPerMinute = null!;

        private readonly Bindable<bool> completed = new Bindable<bool>();

        protected override void LoadComplete()
        {
            base.LoadComplete();

            completedSpins = DrawableSpinner.CompletedFullSpins.GetBoundCopy();
            completedSpins.BindValueChanged(bonus =>
            {
                if (DrawableSpinner.CurrentBonusScore <= 0)
                    return;

                bonusCounter.Text = DrawableSpinner.CurrentBonusScore.ToString(NumberFormatInfo.InvariantInfo);

                if (DrawableSpinner.CurrentBonusScore == DrawableSpinner.MaximumBonusScore)
                {
                    bonusCounter.ScaleTo(1.4f).Then().ScaleTo(1.8f, 1000, Easing.Out);
                    bonusCounter.FadeOutFromOne(500, Easing.Out);
                }
                else
                {
                    bonusCounter.FadeOutFromOne(800, Easing.Out);
                    bonusCounter.ScaleTo(SPRITE_SCALE * 2f).Then().ScaleTo(SPRITE_SCALE * 1.28f, 800, Easing.Out);
                }
            });

            spinsPerMinute = DrawableSpinner.SpinsPerMinute.GetBoundCopy();
            spinsPerMinute.BindValueChanged(spm =>
            {
                spmCounter.Text = Math.Truncate(spm.NewValue).ToString(@"#0");
            }, true);

            completed.BindValueChanged(onCompletedChanged, true);

            DrawableSpinner.ApplyCustomUpdateState += UpdateStateTransforms;
            UpdateStateTransforms(DrawableSpinner, DrawableSpinner.State.Value);
        }

        private void onCompletedChanged(ValueChangedEvent<bool> completed)
        {
            if (completed.NewValue)
            {
                double startTime = Math.Min(Time.Current, DrawableSpinner.HitStateUpdateTime - 400);

                using (BeginAbsoluteSequence(startTime))
                {
                    clear.FadeInFromZero(400, Easing.Out);

                    clear.ScaleTo(SPRITE_SCALE * 2)
                         .Then().ScaleTo(SPRITE_SCALE * 0.8f, 240, Easing.Out)
                         .Then().ScaleTo(SPRITE_SCALE, 160);
                }

                const double fade_out_duration = 50;
                using (BeginAbsoluteSequence(DrawableSpinner.HitStateUpdateTime - fade_out_duration))
                    clear.FadeOut(fade_out_duration);
            }
            else
            {
                clear.ClearTransforms();
                clear.Alpha = 0;
            }
        }

        protected override void Update()
        {
            base.Update();
            completed.Value = Time.Current >= DrawableSpinner.Result.TimeCompleted;
        }

        protected virtual void UpdateStateTransforms(DrawableHitObject drawableHitObject, ArmedState state)
        {
            switch (drawableHitObject)
            {
                case DrawableSpinner d:
                    using (BeginAbsoluteSequence(d.HitObject.StartTime - d.HitObject.TimeFadeIn))
                    {
                        spmBackground.MoveToOffset(new Vector2(0, -spm_hide_offset), d.HitObject.TimeFadeIn, Easing.Out);
                        spmCounter.MoveToOffset(new Vector2(0, -spm_hide_offset), d.HitObject.TimeFadeIn, Easing.Out);
                    }

                    using (BeginAbsoluteSequence(d.HitObject.StartTime - d.HitObject.TimeFadeIn / 2))
                        spin.FadeInFromZero(d.HitObject.TimeFadeIn / 2);

                    using (BeginAbsoluteSequence(d.HitObject.StartTime))
                        ApproachCircle?.ScaleTo(SPRITE_SCALE * 0.1f, d.HitObject.Duration);

                    double spinFadeOutLength = Math.Min(400, d.HitObject.Duration);

                    using (BeginAbsoluteSequence(drawableHitObject.HitStateUpdateTime - spinFadeOutLength))
                        spin.FadeOutFromOne(spinFadeOutLength);
                    break;

                case DrawableSpinnerTick d:
                    if (state == ArmedState.Hit)
                    {
                        using (BeginAbsoluteSequence(d.HitStateUpdateTime))
                            spin.FadeOut(300);
                    }

                    break;
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (DrawableSpinner.IsNotNull())
                DrawableSpinner.ApplyCustomUpdateState -= UpdateStateTransforms;
        }
    }
}
