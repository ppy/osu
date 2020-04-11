// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Animations;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Skinning;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Taiko.Skinning
{
    public class LegacyHit : CompositeDrawable, IHasAccentColour
    {
        private readonly TaikoSkinComponents component;

        private Drawable backgroundLayer;

        public LegacyHit(TaikoSkinComponents component)
        {
            this.component = component;

            RelativeSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load(ISkinSource skin)
        {
            InternalChildren = new[]
            {
                backgroundLayer = skin.GetAnimation("taikohitcircle", true, false),
                skin.GetAnimation("taikohitcircleoverlay", true, false),
            };

            // animations in taiko skins are used in a custom way (>150 combo and animating in time with beat).
            // for now just stop at first frame for sanity.
            foreach (var c in InternalChildren)
            {
                (c as IFramedAnimation)?.Stop();

                c.Anchor = Anchor.Centre;
                c.Origin = Anchor.Centre;
            }

            AccentColour = component == TaikoSkinComponents.CentreHit
                ? new Color4(235, 69, 44, 255)
                : new Color4(67, 142, 172, 255);
        }

        protected override void Update()
        {
            base.Update();

            // not all skins (including the default osu-stable) have similar sizes for hitcircle and hitcircleoverlay.
            // this ensures they are scaled relative to each other but also match the expected DrawableHit size.
            foreach (var c in InternalChildren)
                c.Scale = new Vector2(DrawWidth / 128);
        }

        private Color4 accentColour;

        public Color4 AccentColour
        {
            get => accentColour;
            set
            {
                if (value == accentColour)
                    return;

                backgroundLayer.Colour = accentColour = value;
            }
        }
    }
}
