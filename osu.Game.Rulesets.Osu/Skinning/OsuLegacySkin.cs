// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Audio.Sample;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Textures;
using osu.Game.Audio;
using osu.Game.Skinning;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Osu.Skinning
{
    public class OsuLegacySkin : ISkin
    {
        private readonly ISkin source;

        private Lazy<SkinConfiguration> configuration;

        private Lazy<bool> hasHitCircle;

        /// <summary>
        /// On osu-stable, hitcircles have 5 pixels of transparent padding on each side to allow for shadows etc.
        /// Their hittable area is 128px, but the actual circle portion is 118px.
        /// We must account for some gameplay elements such as slider bodies, where this padding is not present.
        /// </summary>
        private const float legacy_circle_radius = 64 - 5;

        public OsuLegacySkin(ISkinSource source)
        {
            this.source = source;

            source.SourceChanged += sourceChanged;
            sourceChanged();
        }

        private void sourceChanged()
        {
            // these need to be lazy in order to ensure they aren't called before the dependencies have been loaded into our source.
            configuration = new Lazy<SkinConfiguration>(() =>
            {
                var config = new SkinConfiguration();
                if (hasHitCircle.Value)
                    config.SliderPathRadius = legacy_circle_radius;

                // defaults should only be applied for non-beatmap skins (which are parsed via this constructor).
                config.CustomColours["SliderBall"] =
                    source.GetValue<SkinConfiguration, Color4?>(s => s.CustomColours.TryGetValue("SliderBall", out var val) ? val : (Color4?)null)
                    ?? new Color4(2, 170, 255, 255);

                return config;
            });

            hasHitCircle = new Lazy<bool>(() => source.GetTexture("hitcircle") != null);
        }

        public Drawable GetDrawableComponent(ISkinComponent component)
        {
            if (!(component is OsuSkinComponent osuComponent))
                return null;

            switch (osuComponent.Component)
            {
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

                case OsuSkinComponents.HitCircleText:
                    string font = GetValue<SkinConfiguration, string>(config => config.HitCircleFont);
                    var overlap = GetValue<SkinConfiguration, float>(config => config.HitCircleOverlap);

                    return !hasFont(font)
                        ? null
                        : new LegacySpriteText(source, font)
                        {
                            // Spacing value was reverse-engineered from the ratio of the rendered sprite size in the visual inspector vs the actual texture size
                            Scale = new Vector2(0.96f),
                            Spacing = new Vector2(-overlap * 0.89f, 0)
                        };
            }

            return null;
        }

        public Texture GetTexture(string componentName) => source.GetTexture(componentName);

        public SampleChannel GetSample(ISampleInfo sample) => source.GetSample(sample);

        public TValue GetValue<TConfiguration, TValue>(Func<TConfiguration, TValue> query) where TConfiguration : SkinConfiguration
        {
            TValue val;
            if (configuration.Value is TConfiguration conf && (val = query.Invoke(conf)) != null)
                return val;

            return source.GetValue(query);
        }

        private bool hasFont(string fontName) => source.GetTexture($"{fontName}-0") != null;
    }
}
