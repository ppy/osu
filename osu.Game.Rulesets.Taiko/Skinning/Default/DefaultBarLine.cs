// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Taiko.Objects.Drawables;
using osuTK;

namespace osu.Game.Rulesets.Taiko.Skinning.Default
{
    public partial class DefaultBarLine : CompositeDrawable
    {
        /// <summary>
        /// The vertical offset of the triangles from the line tracker.
        /// </summary>
        private const float triangle_offset = 10f;

        /// <summary>
        /// The size of the triangles.
        /// </summary>
        private const float triangle_size = 20f;

        /// <summary>
        /// Container with triangles. Only visible for major lines.
        /// </summary>
        private Container triangleContainer = null!;

        private Bindable<bool> major = null!;

        [BackgroundDependencyLoader]
        private void load(DrawableHitObject drawableHitObject)
        {
            RelativeSizeAxes = Axes.Both;

            InternalChildren = new Drawable[]
            {
                line = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    EdgeSmoothness = new Vector2(0.5f, 0),
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                },
                triangleContainer = new Container
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Children = new[]
                    {
                        new EquilateralTriangle
                        {
                            Name = "Top",
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Position = new Vector2(0, -triangle_offset),
                            Size = new Vector2(-triangle_size),
                            EdgeSmoothness = new Vector2(1),
                        },
                        new EquilateralTriangle
                        {
                            Name = "Bottom",
                            Anchor = Anchor.BottomCentre,
                            Origin = Anchor.TopCentre,
                            Position = new Vector2(0, triangle_offset),
                            Size = new Vector2(triangle_size),
                            EdgeSmoothness = new Vector2(1),
                        }
                    }
                }
            };

            major = ((DrawableBarLine)drawableHitObject).Major.GetBoundCopy();
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            major.BindValueChanged(updateMajor, true);
        }

        private Box line = null!;

        private void updateMajor(ValueChangedEvent<bool> major)
        {
            line.Alpha = major.NewValue ? 1f : 0.75f;
            triangleContainer.Alpha = major.NewValue ? 1 : 0;
        }
    }
}
