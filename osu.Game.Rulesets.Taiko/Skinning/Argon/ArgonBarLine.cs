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

namespace osu.Game.Rulesets.Taiko.Skinning.Argon
{
    public partial class ArgonBarLine : CompositeDrawable
    {
        private Container majorEdgeContainer = null!;

        private Bindable<bool> major = null!;

        [BackgroundDependencyLoader]
        private void load(DrawableHitObject drawableHitObject)
        {
            RelativeSizeAxes = Axes.Both;

            const float line_offset = 8;
            var majorPieceSize = new Vector2(6, 20);

            InternalChildren = new Drawable[]
            {
                line = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    EdgeSmoothness = new Vector2(0.5f, 0),
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                },
                majorEdgeContainer = new Container
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Children = new[]
                    {
                        new Circle
                        {
                            Name = "Top line",
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.BottomCentre,
                            Size = majorPieceSize,
                            Y = -line_offset,
                        },
                        new Circle
                        {
                            Name = "Bottom line",
                            Anchor = Anchor.BottomCentre,
                            Origin = Anchor.TopCentre,
                            Size = majorPieceSize,
                            Y = line_offset,
                        },
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
            line.Alpha = major.NewValue ? 1f : 0.5f;
            line.Width = major.NewValue ? 1 : 0.5f;
            majorEdgeContainer.Alpha = major.NewValue ? 1 : 0;
        }
    }
}
