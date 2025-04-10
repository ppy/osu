// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Utils;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osuTK;

namespace osu.Game.Rulesets.Osu.Skinning.Default
{
    public partial class DefaultReverseArrow : CompositeDrawable
    {
        private DrawableSliderRepeat drawableRepeat { get; set; } = null!;

        public DefaultReverseArrow()
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            Size = OsuHitObject.OBJECT_DIMENSIONS;

            InternalChild = new SpriteIcon
            {
                RelativeSizeAxes = Axes.Both,
                Blending = BlendingParameters.Additive,
                Icon = FontAwesome.Solid.ChevronRight,
                Size = new Vector2(0.35f),
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            };
        }

        [BackgroundDependencyLoader]
        private void load(DrawableHitObject drawableObject)
        {
            drawableRepeat = (DrawableSliderRepeat)drawableObject;
        }

        protected override void Update()
        {
            base.Update();

            if (Time.Current >= drawableRepeat.HitStateUpdateTime && drawableRepeat.State.Value == ArmedState.Hit)
            {
                double animDuration = Math.Min(300, drawableRepeat.HitObject.SpanDuration);
                Scale = new Vector2(Interpolation.ValueAt(Time.Current, 1, 1.5f, drawableRepeat.HitStateUpdateTime, drawableRepeat.HitStateUpdateTime + animDuration, Easing.Out));
            }
            else
            {
                const float scale_amount = 1.3f;

                const double move_out_duration = 35;
                const double move_in_duration = 250;
                const double total = 300;

                double loopCurrentTime = (Time.Current - drawableRepeat.AnimationStartTime.Value) % total;
                if (loopCurrentTime < move_out_duration)
                    Scale = new Vector2(Interpolation.ValueAt(loopCurrentTime, 1, scale_amount, 0, move_out_duration, Easing.Out));
                else
                    Scale = new Vector2(Interpolation.ValueAt(loopCurrentTime, scale_amount, 1f, move_out_duration, move_out_duration + move_in_duration, Easing.Out));
            }
        }
    }
}
