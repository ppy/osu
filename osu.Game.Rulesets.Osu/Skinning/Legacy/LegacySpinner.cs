// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Rulesets.Osu.Skinning.Legacy
{
    public abstract class LegacySpinner : CompositeDrawable
    {
        /// <remarks>
        /// All constants are in osu!stable's gamefield space, which is shifted 16px downwards.
        /// This offset is negated in both osu!stable and osu!lazer to bring all constants into window-space.
        /// Note: SPINNER_Y_CENTRE + SPINNER_TOP_OFFSET - Position.Y = 240 (=480/2, or half the window-space in osu!stable)
        /// </remarks>
        protected const float SPINNER_TOP_OFFSET = 45f - 16f;

        protected const float SPINNER_Y_CENTRE = SPINNER_TOP_OFFSET + 219f;

        protected const float SPRITE_SCALE = 0.625f;

        protected DrawableSpinner DrawableSpinner { get; private set; }

        private Sprite spin;
        private Sprite clear;

        private LegacySpriteText bonusCounter;

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

            Container overlayContainer;

            AddInternal(overlayContainer = new Container
            {
                Depth = float.MinValue,
                RelativeSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    spin = new Sprite
                    {
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
                }
            });

            bonusCounter = (LegacySpriteText)source.GetDrawableComponent(new HUDSkinComponent(HUDSkinComponents.ScoreText));

            if (bonusCounter != null)
            {
                bonusCounter.Alpha = 0f;
                bonusCounter.Anchor = Anchor.TopCentre;
                bonusCounter.Origin = Anchor.Centre;
                bonusCounter.Font = bonusCounter.Font.With(fixedWidth: false);
                bonusCounter.Scale = new Vector2(SPRITE_SCALE);
                bonusCounter.Y = SPINNER_TOP_OFFSET + 299;
                overlayContainer.Add(bonusCounter);
            }
        }

        private IBindable<double> gainedBonus;

        private readonly Bindable<bool> completed = new Bindable<bool>();

        protected override void LoadComplete()
        {
            base.LoadComplete();

            if (bonusCounter != null)
            {
                gainedBonus = DrawableSpinner.GainedBonus.GetBoundCopy();
                gainedBonus.BindValueChanged(bonus =>
                {
                    bonusCounter.Text = $"{bonus.NewValue}";
                    bonusCounter.FadeOutFromOne(800, Easing.Out);
                    bonusCounter.ScaleTo(SPRITE_SCALE * 2f).Then().ScaleTo(SPRITE_SCALE * 1.28f, 800, Easing.Out);
                });
            }

            completed.BindValueChanged(onCompletedChanged, true);

            DrawableSpinner.ApplyCustomUpdateState += UpdateStateTransforms;
            UpdateStateTransforms(DrawableSpinner, DrawableSpinner.State.Value);
        }

        private void onCompletedChanged(ValueChangedEvent<bool> completed)
        {
            if (completed.NewValue)
            {
                double startTime = Math.Min(Time.Current, DrawableSpinner.HitStateUpdateTime - 400);

                using (BeginAbsoluteSequence(startTime, true))
                {
                    clear.FadeInFromZero(400, Easing.Out);

                    clear.ScaleTo(SPRITE_SCALE * 2)
                         .Then().ScaleTo(SPRITE_SCALE * 0.8f, 240, Easing.Out)
                         .Then().ScaleTo(SPRITE_SCALE, 160);
                }

                const double fade_out_duration = 50;
                using (BeginAbsoluteSequence(DrawableSpinner.HitStateUpdateTime - fade_out_duration, true))
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
                    double fadeOutLength = Math.Min(400, d.HitObject.Duration);

                    using (BeginAbsoluteSequence(drawableHitObject.HitStateUpdateTime - fadeOutLength, true))
                        spin.FadeOutFromOne(fadeOutLength);
                    break;

                case DrawableSpinnerTick d:
                    if (state == ArmedState.Hit)
                    {
                        using (BeginAbsoluteSequence(d.HitStateUpdateTime, true))
                            spin.FadeOut(300);
                    }

                    break;
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (DrawableSpinner != null)
                DrawableSpinner.ApplyCustomUpdateState -= UpdateStateTransforms;
        }
    }
}
