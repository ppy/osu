// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
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

            AddInternal(mainLine = new Box
            {
                Name = "Bar line",
                Anchor = Anchor.BottomCentre,
                Origin = Anchor.BottomCentre,
                RelativeSizeAxes = Axes.Both,
            });

            Vector2 size = new Vector2(22, 6);
            const float line_offset = 4;

            AddInternal(leftAnchor = new Circle
            {
                Name = "Left anchor",
                Anchor = Anchor.CentreLeft,
                Origin = Anchor.CentreRight,
                Size = size,
                X = -line_offset,
            });

            AddInternal(rightAnchor = new Circle
            {
                Name = "Right anchor",
                Anchor = Anchor.CentreRight,
                Origin = Anchor.CentreLeft,
                Size = size,
                X = line_offset,
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
            leftAnchor.Alpha = rightAnchor.Alpha = major.NewValue ? 1 : 0;
        }
    }
}
