// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Taiko.Objects.Drawables;
using osuTK;

namespace osu.Game.Rulesets.Taiko.Skinning.Argon
{
    public partial class ArgonBarLine : CompositeDrawable
    {
        private Bindable<bool> major = null!;

        private Box mainLine = null!;
        private Drawable topAnchor = null!;
        private Drawable bottomAnchor = null!;

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
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
            });

            const float major_extension = 10;

            AddInternal(topAnchor = new Box
            {
                Name = "Top anchor",
                EdgeSmoothness = edgeSmoothness,
                Blending = BlendingParameters.Additive,
                Anchor = Anchor.TopCentre,
                Origin = Anchor.BottomCentre,
                Height = major_extension,
                RelativeSizeAxes = Axes.X,
                Colour = ColourInfo.GradientVertical(Colour4.Transparent, Colour4.White),
            });

            AddInternal(bottomAnchor = new Box
            {
                Name = "Bottom anchor",
                EdgeSmoothness = edgeSmoothness,
                Blending = BlendingParameters.Additive,
                Anchor = Anchor.BottomCentre,
                Origin = Anchor.TopCentre,
                Height = major_extension,
                RelativeSizeAxes = Axes.X,
                Colour = ColourInfo.GradientVertical(Colour4.White, Colour4.Transparent),
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
            mainLine.Alpha = major.NewValue ? 1f : 0.5f;
            topAnchor.Alpha = bottomAnchor.Alpha = major.NewValue ? mainLine.Alpha * 0.3f : 0;
        }
    }
}
