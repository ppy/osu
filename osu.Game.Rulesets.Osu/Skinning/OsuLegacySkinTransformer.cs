// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Textures;
using osu.Game.Audio;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Rulesets.Osu.Skinning
{
    public class OsuLegacySkinTransformer : ISkin
    {
        private readonly ISkin source;

        private Lazy<bool> hasHitCircle;

        /// <summary>
        /// On osu-stable, hitcircles have 5 pixels of transparent padding on each side to allow for shadows etc.
        /// Their hittable area is 128px, but the actual circle portion is 118px.
        /// We must account for some gameplay elements such as slider bodies, where this padding is not present.
        /// </summary>
        private const float legacy_circle_radius = 64 - 5;

        public OsuLegacySkinTransformer(ISkinSource source)
        {
            this.source = source;

            source.SourceChanged += sourceChanged;
            sourceChanged();
        }

        private void sourceChanged()
        {
            hasHitCircle = new Lazy<bool>(() => source.GetTexture("hitcircle") != null);
        }

        public Drawable GetDrawableComponent(ISkinComponent component)
        {
            if (!(component is OsuSkinComponent osuComponent))
                return null;

            switch (osuComponent.Component)
            {
                case OsuSkinComponents.FollowPoint:
                    return this.GetAnimation(component.LookupName, true, false);

                case OsuSkinComponents.SliderFollowCircle:
                    return this.GetAnimation("sliderfollowcircle", true, true);

                case OsuSkinComponents.SliderBall:
                    var sliderBallContent = this.GetAnimation("sliderb", true, true, "");

                    if (sliderBallContent != null)
                    {
                        var size = sliderBallContent.Size;

                        sliderBallContent.RelativeSizeAxes = Axes.Both;
                        sliderBallContent.Size = Vector2.One;

                        return new LegacySliderBall(sliderBallContent)
                        {
                            Size = size
                        };
                    }

                    return null;

                case OsuSkinComponents.HitCircle:
                    if (hasHitCircle.Value)
                        return new LegacyMainCirclePiece();

                    return null;

                case OsuSkinComponents.Cursor:
                    if (source.GetTexture("cursor") != null)
                        return new LegacyCursor();

                    return null;

                case OsuSkinComponents.CursorTrail:
                    if (source.GetTexture("cursortrail") != null)
                        return new LegacyCursorTrail();

                    return null;

                case OsuSkinComponents.HitCircleText:
                    var font = GetConfig<OsuSkinConfiguration, string>(OsuSkinConfiguration.HitCirclePrefix)?.Value ?? "default";
                    var overlap = GetConfig<OsuSkinConfiguration, float>(OsuSkinConfiguration.HitCircleOverlap)?.Value ?? 0;

                    return !hasFont(font)
                        ? null
                        : new LegacySpriteText(source, font)
                        {
                            // stable applies a blanket 0.8x scale to hitcircle fonts
                            Scale = new Vector2(0.8f),
                            Spacing = new Vector2(-overlap, 0)
                        };
            }

            return null;
        }

        public Texture GetTexture(string componentName) => source.GetTexture(componentName);

        public SampleChannel GetSample(ISampleInfo sample) => source.GetSample(sample);

        public IBindable<TValue> GetConfig<TLookup, TValue>(TLookup lookup)
        {
            switch (lookup)
            {
                case OsuSkinColour colour:
                    return source.GetConfig<SkinCustomColourLookup, TValue>(new SkinCustomColourLookup(colour));

                case OsuSkinConfiguration osuLookup:
                    switch (osuLookup)
                    {
                        case OsuSkinConfiguration.SliderPathRadius:
                            if (hasHitCircle.Value)
                                return SkinUtils.As<TValue>(new BindableFloat(legacy_circle_radius));

                            break;
                    }

                    break;
            }

            return source.GetConfig<TLookup, TValue>(lookup);
        }

        private bool hasFont(string fontName) => source.GetTexture($"{fontName}-0") != null;
    }
}
