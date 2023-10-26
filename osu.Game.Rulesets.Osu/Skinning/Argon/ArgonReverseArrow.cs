// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
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

            drawableRepeat.ApplyCustomUpdateState += updateStateTransforms;
        }

        private void updateStateTransforms(DrawableHitObject hitObject, ArmedState state)
        {
            const float move_distance = -12;
            const double move_out_duration = 35;
            const double move_in_duration = 250;
            const double total = 300;

            switch (state)
            {
                case ArmedState.Idle:
                    main.ScaleTo(1.3f, move_out_duration, Easing.Out)
                        .Then()
                        .ScaleTo(1f, move_in_duration, Easing.Out)
                        .Loop(total - (move_in_duration + move_out_duration));
                    side
                        .MoveToX(move_distance, move_out_duration, Easing.Out)
                        .Then()
                        .MoveToX(0, move_in_duration, Easing.Out)
                        .Loop(total - (move_in_duration + move_out_duration));
                    break;

                case ArmedState.Hit:
                    double animDuration = Math.Min(300, drawableRepeat.HitObject.SpanDuration);
                    this.ScaleTo(1.5f, animDuration, Easing.Out);
                    break;
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (drawableRepeat.IsNotNull())
                drawableRepeat.ApplyCustomUpdateState -= updateStateTransforms;
        }
    }
}
