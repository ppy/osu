// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects;
using osuTK;

namespace osu.Game.Rulesets.Osu.Skinning.Default
{
    public partial class DefaultReverseArrow : CompositeDrawable
    {
        [Resolved]
        private DrawableHitObject drawableObject { get; set; } = null!;

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
        private void load()
        {
            drawableObject.ApplyCustomUpdateState += updateStateTransforms;
        }

        private void updateStateTransforms(DrawableHitObject hitObject, ArmedState state)
        {
            const double move_out_duration = 35;
            const double move_in_duration = 250;
            const double total = 300;

            switch (state)
            {
                case ArmedState.Idle:
                    InternalChild.ScaleTo(1.3f, move_out_duration, Easing.Out)
                                 .Then()
                                 .ScaleTo(1f, move_in_duration, Easing.Out)
                                 .Loop(total - (move_in_duration + move_out_duration));
                    break;
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (drawableObject.IsNotNull())
                drawableObject.ApplyCustomUpdateState -= updateStateTransforms;
        }
    }
}
