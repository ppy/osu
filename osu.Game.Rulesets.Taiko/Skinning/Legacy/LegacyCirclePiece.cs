// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Animations;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Skinning;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Taiko.Skinning.Legacy
{
    public class LegacyCirclePiece : CompositeDrawable, IHasAccentColour
    {
        private Drawable backgroundLayer;

        public LegacyCirclePiece()
        {
            RelativeSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load(ISkinSource skin, DrawableHitObject drawableHitObject)
        {
            Drawable getDrawableFor(string lookup)
            {
                const string normal_hit = "taikohit";
                const string big_hit = "taikobig";

                string prefix = ((drawableHitObject.HitObject as TaikoStrongableHitObject)?.IsStrong ?? false) ? big_hit : normal_hit;

                return skin.GetAnimation($"{prefix}{lookup}", true, false) ??
                       // fallback to regular size if "big" version doesn't exist.
                       skin.GetAnimation($"{normal_hit}{lookup}", true, false);
            }

            // backgroundLayer is guaranteed to exist due to the pre-check in TaikoLegacySkinTransformer.
            AddInternal(backgroundLayer = getDrawableFor("circle"));

            var foregroundLayer = getDrawableFor("circleoverlay");
            if (foregroundLayer != null)
                AddInternal(foregroundLayer);

            // Animations in taiko skins are used in a custom way (>150 combo and animating in time with beat).
            // For now just stop at first frame for sanity.
            foreach (var c in InternalChildren)
            {
                (c as IFramedAnimation)?.Stop();

                c.Anchor = Anchor.Centre;
                c.Origin = Anchor.Centre;
            }
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            updateAccentColour();
        }

        protected override void Update()
        {
            base.Update();

            // Not all skins (including the default osu-stable) have similar sizes for "hitcircle" and "hitcircleoverlay".
            // This ensures they are scaled relative to each other but also match the expected DrawableHit size.
            foreach (var c in InternalChildren)
                c.Scale = new Vector2(DrawHeight / 128);
        }

        private Color4 accentColour;

        public Color4 AccentColour
        {
            get => accentColour;
            set
            {
                if (value == accentColour)
                    return;

                accentColour = value;
                if (IsLoaded)
                    updateAccentColour();
            }
        }

        private void updateAccentColour()
        {
            backgroundLayer.Colour = LegacyColourCompatibility.DisallowZeroAlpha(accentColour);
        }
    }
}
