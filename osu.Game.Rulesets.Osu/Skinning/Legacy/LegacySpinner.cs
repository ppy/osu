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
        protected const float SPRITE_SCALE = 0.625f;

        protected DrawableSpinner DrawableSpinner { get; private set; }

        private Sprite spin;
        private Sprite clear;

        [BackgroundDependencyLoader]
        private void load(DrawableHitObject drawableHitObject, ISkinSource source)
        {
            RelativeSizeAxes = Axes.Both;

            DrawableSpinner = (DrawableSpinner)drawableHitObject;

            AddRangeInternal(new[]
            {
                spin = new Sprite
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Depth = float.MinValue,
                    Texture = source.GetTexture("spinner-spin"),
                    Scale = new Vector2(SPRITE_SCALE),
                    Y = 120 - 45 // offset temporarily to avoid overlapping default spin counter
                },
                clear = new Sprite
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Depth = float.MinValue,
                    Alpha = 0,
                    Texture = source.GetTexture("spinner-clear"),
                    Scale = new Vector2(SPRITE_SCALE),
                    Y = -60
                },
            });
        }

        private readonly Bindable<bool> completed = new Bindable<bool>();

        protected override void LoadComplete()
        {
            base.LoadComplete();

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
