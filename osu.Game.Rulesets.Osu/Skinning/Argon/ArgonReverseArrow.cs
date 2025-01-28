// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Utils;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Osu.Skinning.Argon
{
    public partial class ArgonReverseArrow : CompositeDrawable
    {
        private DrawableSliderRepeat drawableRepeat { get; set; } = null!;

        private Bindable<Color4> accentColour = null!;

        private SpriteIcon icon = null!;
        private Container main = null!;
        private Sprite side = null!;

        [BackgroundDependencyLoader]
        private void load(DrawableHitObject drawableObject, TextureStore textures)
        {
            drawableRepeat = (DrawableSliderRepeat)drawableObject;

            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            Size = OsuHitObject.OBJECT_DIMENSIONS;

            InternalChildren = new Drawable[]
            {
                main = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Children = new Drawable[]
                    {
                        new Circle
                        {
                            Size = new Vector2(40, 20),
                            Colour = Color4.White,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                        },
                        icon = new SpriteIcon
                        {
                            Icon = FontAwesome.Solid.AngleDoubleRight,
                            Size = new Vector2(16),
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                        },
                    }
                },
                side = new Sprite
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Texture = textures.Get("Gameplay/osu/repeat-edge-piece"),
                    Size = new Vector2(ArgonMainCirclePiece.OUTER_GRADIENT_SIZE),
                }
            };

            accentColour = drawableRepeat.AccentColour.GetBoundCopy();
            accentColour.BindValueChanged(accent => icon.Colour = accent.NewValue.Darken(4), true);
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
                Scale = Vector2.One;

            const float move_distance = -12;
            const float scale_amount = 1.3f;

            const double move_out_duration = 35;
            const double move_in_duration = 250;
            const double total = 300;

            double loopCurrentTime = (Time.Current - drawableRepeat.AnimationStartTime.Value) % total;

            if (loopCurrentTime < move_out_duration)
                main.Scale = new Vector2(Interpolation.ValueAt(loopCurrentTime, 1, scale_amount, 0, move_out_duration, Easing.Out));
            else
                main.Scale = new Vector2(Interpolation.ValueAt(loopCurrentTime, scale_amount, 1f, move_out_duration, move_out_duration + move_in_duration, Easing.Out));

            if (loopCurrentTime < move_out_duration)
                side.X = Interpolation.ValueAt(loopCurrentTime, 0, move_distance, 0, move_out_duration, Easing.Out);
            else
                side.X = Interpolation.ValueAt(loopCurrentTime, move_distance, 0, move_out_duration, move_out_duration + move_in_duration, Easing.Out);
        }
    }
}
