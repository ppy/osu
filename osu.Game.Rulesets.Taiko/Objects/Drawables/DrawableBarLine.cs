// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Taiko.Skinning.Default;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Taiko.Objects.Drawables
{
    /// <summary>
    /// A line that scrolls alongside hit objects in the playfield and visualises control points.
    /// </summary>
    public partial class DrawableBarLine : DrawableHitObject<HitObject>
    {
        public new BarLine HitObject => (BarLine)base.HitObject;

        /// <summary>
        /// The width of the line tracker.
        /// </summary>
        private const float tracker_width = 2f;

        public readonly Bindable<bool> Major = new Bindable<bool>();

        public DrawableBarLine()
            : this(null)
        {
        }

        public DrawableBarLine(BarLine? barLine)
            : base(barLine!)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Anchor = Anchor.CentreLeft;
            Origin = Anchor.Centre;

            RelativeSizeAxes = Axes.Y;
            Width = tracker_width;

            AddInternal(new SkinnableDrawable(new TaikoSkinComponentLookup(TaikoSkinComponents.BarLine), _ => new DefaultBarLine())
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            });
        }

        protected override void OnApply()
        {
            base.OnApply();
            Major.BindTo(HitObject.MajorBindable);
        }

        protected override void OnFree()
        {
            base.OnFree();
            Major.UnbindFrom(HitObject.MajorBindable);
        }

        protected override void UpdateHitStateTransforms(ArmedState state)
        {
            using (BeginAbsoluteSequence(HitObject.StartTime))
                this.FadeOutFromOne(150).Expire();
        }
    }
}
