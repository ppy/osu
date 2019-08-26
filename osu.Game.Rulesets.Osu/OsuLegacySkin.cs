// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Animations;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Skinning;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Osu
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

        private const double default_frame_time = 1000 / 60d;

        public Drawable GetDrawableComponent(string componentName)
        {
            switch (componentName)
            {
                case "Play/osu/sliderball":
                    var sliderBallContent = getAnimation("sliderb", true, true, "");

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

                case "Play/osu/hitcircle":
                    if (hasHitCircle.Value)
                        return new LegacyMainCirclePiece();

                    return null;
            }

            return null;
        }

        private Drawable getAnimation(string componentName, bool animatable, bool looping, string animationSeparator = "-")
        {
            Texture texture;

            Texture getFrameTexture(int frame) => source.GetTexture($"{componentName}{animationSeparator}{frame}");

            TextureAnimation animation = null;

            if (animatable)
            {
                for (int i = 0;; i++)
                {
                    if ((texture = getFrameTexture(i)) == null)
                        break;

                    if (animation == null)
                        animation = new TextureAnimation
                        {
                            DefaultFrameLength = default_frame_time,
                            Repeat = looping
                        };

                    animation.AddFrame(texture);
                }
            }

            if (animation != null)
                return animation;

            if ((texture = source.GetTexture(componentName)) != null)
                return new Sprite { Texture = texture };

            return null;
        }

        public Texture GetTexture(string componentName) => null;

        public SampleChannel GetSample(string sampleName) => null;

        public TValue GetValue<TConfiguration, TValue>(Func<TConfiguration, TValue> query) where TConfiguration : SkinConfiguration
            => configuration.Value is TConfiguration conf ? query.Invoke(conf) : default;

        public class LegacySliderBall : CompositeDrawable
        {
            private readonly Drawable animationContent;

            public LegacySliderBall(Drawable animationContent)
            {
                this.animationContent = animationContent;
            }

            [BackgroundDependencyLoader]
            private void load(ISkinSource skin, DrawableHitObject drawableObject)
            {
                animationContent.Colour = skin.GetValue<SkinConfiguration, Color4?>(s => s.CustomColours.ContainsKey("SliderBall") ? s.CustomColours["SliderBall"] : (Color4?)null) ?? Color4.White;

                InternalChildren = new[]
                {
                    new Sprite
                    {
                        Texture = skin.GetTexture("sliderb-nd"),
                        Colour = new Color4(5, 5, 5, 255),
                    },
                    animationContent,
                    new Sprite
                    {
                        Texture = skin.GetTexture("sliderb-spec"),
                        Blending = BlendingParameters.Additive,
                    },
                };
            }
        }

        public class LegacyMainCirclePiece : CompositeDrawable
        {
            public LegacyMainCirclePiece()
            {
                Size = new Vector2(128);
            }

            private readonly IBindable<ArmedState> state = new Bindable<ArmedState>();

            private readonly Bindable<Color4> accentColour = new Bindable<Color4>();

            [BackgroundDependencyLoader]
            private void load(DrawableHitObject drawableObject, ISkinSource skin)
            {
                Sprite hitCircleSprite;

                InternalChildren = new Drawable[]
                {
                    hitCircleSprite = new Sprite
                    {
                        Texture = skin.GetTexture("hitcircle"),
                        Colour = drawableObject.AccentColour.Value,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                    },
                    new SkinnableSpriteText("Play/osu/number-text", _ => new OsuSpriteText
                    {
                        Font = OsuFont.Numeric.With(size: 40),
                        UseFullGlyphHeight = false,
                    }, confineMode: ConfineMode.NoScaling)
                    {
                        Text = (((IHasComboInformation)drawableObject.HitObject).IndexInCurrentCombo + 1).ToString()
                    },
                    new Sprite
                    {
                        Texture = skin.GetTexture("hitcircleoverlay"),
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                    }
                };

                state.BindTo(drawableObject.State);
                state.BindValueChanged(updateState, true);

                accentColour.BindTo(drawableObject.AccentColour);
                accentColour.BindValueChanged(colour => hitCircleSprite.Colour = colour.NewValue, true);
            }

            private void updateState(ValueChangedEvent<ArmedState> state)
            {
                const double legacy_fade_duration = 240;

                switch (state.NewValue)
                {
                    case ArmedState.Hit:
                        this.FadeOut(legacy_fade_duration, Easing.Out);
                        this.ScaleTo(1.4f, legacy_fade_duration, Easing.Out);
                        break;
                }
            }
        }
    }
}
