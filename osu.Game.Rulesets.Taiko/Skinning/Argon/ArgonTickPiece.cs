// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Rulesets.Taiko.Objects.Drawables;
using osuTK;

namespace osu.Game.Rulesets.Taiko.Skinning.Argon
{
    public partial class ArgonTickPiece : CompositeDrawable
    {
        private readonly Bindable<bool> isFirstTick = new Bindable<bool>();

        public ArgonTickPiece()
        {
            const float tick_size = 1 / TaikoHitObject.DEFAULT_SIZE * ArgonCirclePiece.ICON_SIZE;

            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            RelativeSizeAxes = Axes.Both;
            FillMode = FillMode.Fit;
            Size = new Vector2(tick_size);
        }

        [Resolved]
        private DrawableHitObject drawableHitObject { get; set; } = null!;

        protected override void LoadComplete()
        {
            base.LoadComplete();

            if (drawableHitObject is DrawableDrumRollTick drumRollTick)
                isFirstTick.BindTo(drumRollTick.IsFirstTick);

            isFirstTick.BindValueChanged(first =>
            {
                if (first.NewValue)
                {
                    InternalChild = new Circle
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.Both
                    };
                }
                else
                {
                    InternalChild = new SpriteIcon
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.Both,
                        Icon = FontAwesome.Solid.AngleLeft,
                        Scale = new Vector2(0.8f, 1)
                    };
                }
            }, true);
        }
    }
}
