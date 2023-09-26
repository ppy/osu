// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Rulesets.Mania.Objects.Drawables;
using osu.Game.Rulesets.Objects.Drawables;
using osuTK;

namespace osu.Game.Rulesets.Mania.Skinning.Default
{
    public partial class DefaultBarLine : CompositeDrawable
    {
        private Bindable<bool> major = null!;

        private Drawable mainLine = null!;
        private Drawable leftAnchor = null!;
        private Drawable rightAnchor = null!;

        [BackgroundDependencyLoader]
        private void load(DrawableHitObject drawableHitObject)
        {
            RelativeSizeAxes = Axes.Both;

            // Avoid flickering due to no anti-aliasing of boxes by default.
            var edgeSmoothness = new Vector2(0.3f);

            AddInternal(mainLine = new Box
            {
                Name = "Bar line",
                EdgeSmoothness = edgeSmoothness,
                Anchor = Anchor.BottomCentre,
                Origin = Anchor.BottomCentre,
                RelativeSizeAxes = Axes.Both,
            });

            const float major_extension = 10;

            AddInternal(leftAnchor = new Box
            {
                Name = "Left anchor",
                EdgeSmoothness = edgeSmoothness,
                Blending = BlendingParameters.Additive,
                Anchor = Anchor.CentreLeft,
                Origin = Anchor.CentreRight,
                Width = major_extension,
                RelativeSizeAxes = Axes.Y,
                Colour = ColourInfo.GradientHorizontal(Colour4.Transparent, Colour4.White),
            });

            AddInternal(rightAnchor = new Box
            {
                Name = "Right anchor",
                EdgeSmoothness = edgeSmoothness,
                Blending = BlendingParameters.Additive,
                Anchor = Anchor.CentreRight,
                Origin = Anchor.CentreLeft,
                Width = major_extension,
                RelativeSizeAxes = Axes.Y,
                Colour = ColourInfo.GradientHorizontal(Colour4.White, Colour4.Transparent),
            });

            major = ((DrawableBarLine)drawableHitObject).Major.GetBoundCopy();
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            major.BindValueChanged(updateMajor, true);
        }

        private void updateMajor(ValueChangedEvent<bool> major)
        {
            mainLine.Alpha = major.NewValue ? 0.5f : 0.2f;
            leftAnchor.Alpha = rightAnchor.Alpha = major.NewValue ? mainLine.Alpha * 0.3f : 0;
        }
    }
}
