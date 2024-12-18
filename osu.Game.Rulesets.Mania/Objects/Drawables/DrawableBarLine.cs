// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Mania.Skinning.Default;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Mania.Objects.Drawables
{
    /// <summary>
    /// Visualises a <see cref="BarLine"/>. Although this derives DrawableManiaHitObject,
    /// this does not handle input/sound like a normal hit object.
    /// </summary>
    public partial class DrawableBarLine : DrawableManiaHitObject<BarLine>
    {
        public readonly Bindable<bool> Major = new Bindable<bool>();

        public DrawableBarLine()
            : this(null!)
        {
        }

        public DrawableBarLine(BarLine barLine)
            : base(barLine)
        {
            RelativeSizeAxes = Axes.X;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            AddInternal(new SkinnableDrawable(new ManiaSkinComponentLookup(ManiaSkinComponents.BarLine), _ => new DefaultBarLine())
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            });

            Major.BindValueChanged(major => Height = major.NewValue ? 1.7f : 1.2f, true);
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

        protected override void UpdateStartTimeStateTransforms() => this.FadeOut(150);
    }
}
