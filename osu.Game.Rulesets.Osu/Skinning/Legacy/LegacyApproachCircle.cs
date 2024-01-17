// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Skinning;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Osu.Skinning.Legacy
{
    // todo: this should probably not be a SkinnableSprite, as this is always created for legacy skins and is recreated on skin change.
    public partial class LegacyApproachCircle : SkinnableSprite
    {
        private readonly IBindable<Color4> accentColour = new Bindable<Color4>();

        [Resolved]
        private DrawableHitObject drawableObject { get; set; } = null!;

        public LegacyApproachCircle()
            : base(@"approachcircle", OsuHitObject.OBJECT_DIMENSIONS * 2)
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
            accentColour.BindValueChanged(colour => Colour = LegacyColourCompatibility.DisallowZeroAlpha(colour.NewValue), true);
        }

        protected override Drawable CreateDefault(ISkinComponentLookup lookup)
        {
            var drawable = base.CreateDefault(lookup);

            // account for the sprite being used for the default approach circle being taken from stable,
            // when hitcircles have 5px padding on each size. this should be removed if we update the sprite.
            drawable.Scale = new Vector2(128 / 118f);

            return drawable;
        }
    }
}
