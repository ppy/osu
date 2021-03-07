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
        /// <summary>
        /// An offset that simulates stable's spinner top offset, can be used with <see cref="LegacyCoordinatesContainer"/>
        /// for positioning some legacy spinner components perfectly as in stable.
        /// (e.g. 'spin' sprite, 'clear' sprite, metre in old-style spinners)
        /// </summary>
        public static readonly float SPINNER_TOP_OFFSET = (float)Math.Ceiling(45f * SPRITE_SCALE);

        protected const float SPRITE_SCALE = 0.625f;

        protected DrawableSpinner DrawableSpinner { get; private set; }

        private Sprite spin;
        private Sprite clear;

        [BackgroundDependencyLoader]
        private void load(DrawableHitObject drawableHitObject, ISkinSource source)
        {
            RelativeSizeAxes = Axes.Both;

            DrawableSpinner = (DrawableSpinner)drawableHitObject;

            AddInternal(new LegacyCoordinatesContainer
            {
                Depth = float.MinValue,
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

        /// <summary>
        /// A <see cref="Container"/> simulating osu!stable's absolute screen-space,
        /// for perfect placements of legacy spinner components with legacy coordinates.
        /// </summary>
        protected class LegacyCoordinatesContainer : Container
        {
            public LegacyCoordinatesContainer()
            {
                // legacy spinners relied heavily on absolute screen-space coordinate values.
                // wrap everything in a container simulating absolute coords to preserve alignment
                // as there are skins that depend on it.
                Anchor = Anchor.Centre;
                Origin = Anchor.Centre;
                Size = new Vector2(640, 480);

                // counteracts the playfield shift from OsuPlayfieldAdjustmentContainer.
                Position = new Vector2(0, -8f);
            }
        }
    }
}
