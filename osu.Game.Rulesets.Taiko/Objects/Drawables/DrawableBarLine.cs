// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Rulesets.Objects;
using osuTK;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Taiko.Objects.Drawables
{
    /// <summary>
    /// A line that scrolls alongside hit objects in the playfield and visualises control points.
    /// </summary>
    public class DrawableBarLine : DrawableHitObject<HitObject>
    {
        public new BarLine HitObject => (BarLine)base.HitObject;

        /// <summary>
        /// The width of the line tracker.
        /// </summary>
        private const float tracker_width = 2f;

        /// <summary>
        /// The vertical offset of the triangles from the line tracker.
        /// </summary>
        private const float triangle_offset = 10f;

        /// <summary>
        /// The size of the triangles.
        /// </summary>
        private const float triangle_size = 20f;

        /// <summary>
        /// The visual line tracker.
        /// </summary>
        private SkinnableDrawable line;

        /// <summary>
        /// Container with triangles. Only visible for major lines.
        /// </summary>
        private Container triangleContainer;

        private readonly Bindable<bool> major = new Bindable<bool>();

        public DrawableBarLine()
            : this(null)
        {
        }

        public DrawableBarLine([CanBeNull] BarLine barLine)
            : base(barLine)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Anchor = Anchor.CentreLeft;
            Origin = Anchor.Centre;

            RelativeSizeAxes = Axes.Y;
            Width = tracker_width;

            AddRangeInternal(new Drawable[]
            {
                line = new SkinnableDrawable(new TaikoSkinComponent(TaikoSkinComponents.BarLine), _ => new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    EdgeSmoothness = new Vector2(0.5f, 0),
                })
                {
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
            });
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            major.BindValueChanged(updateMajor, true);
        }

        private void updateMajor(ValueChangedEvent<bool> major)
        {
            line.Alpha = major.NewValue ? 1f : 0.75f;
            triangleContainer.Alpha = major.NewValue ? 1 : 0;
        }

        protected override void OnApply()
        {
            base.OnApply();
            major.BindTo(HitObject.MajorBindable);
        }

        protected override void OnFree()
        {
            base.OnFree();
            major.UnbindFrom(HitObject.MajorBindable);
        }

        protected override void UpdateHitStateTransforms(ArmedState state)
        {
            using (BeginAbsoluteSequence(HitObject.StartTime))
                this.FadeOutFromOne(150).Expire();
        }
    }
}
