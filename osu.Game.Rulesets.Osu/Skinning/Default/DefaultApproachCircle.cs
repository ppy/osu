// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Skinning;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Osu.Skinning.Default
{
    public partial class DefaultApproachCircle : SkinnableSprite
    {
        private readonly IBindable<Color4> accentColour = new Bindable<Color4>();

        [Resolved]
        private DrawableHitObject drawableObject { get; set; } = null!;

        public DefaultApproachCircle()
            : base("Gameplay/osu/approachcircle")
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            accentColour.BindTo(drawableObject.AccentColour);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            accentColour.BindValueChanged(colour => Colour = colour.NewValue, true);
        }

        protected override Drawable CreateDefault(ISkinComponentLookup lookup)
        {
            var drawable = base.CreateDefault(lookup);

            // Although this is a non-legacy component, osu-resources currently stores approach circle as a legacy-like texture.
            // See LegacyApproachCircle for documentation as to why this is required.
            drawable.Scale = new Vector2(128 / 118f);

            return drawable;
        }
    }
}
