// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Configuration;
using osu.Game.Rulesets.Osu.Skinning;
using osu.Game.Skinning;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Osu.Objects.Drawables.Pieces
{
    public abstract class PlaySliderBody : SnakingSliderBody
    {
        private IBindable<float> scaleBindable;
        private IBindable<int> pathVersion;
        private IBindable<Color4> accentColour;

        [Resolved]
        private DrawableHitObject drawableObject { get; set; }

        [Resolved(CanBeNull = true)]
        private OsuRulesetConfigManager config { get; set; }

        private Slider slider;
        private float defaultPathRadius;

        [BackgroundDependencyLoader]
        private void load(ISkinSource skin)
        {
            slider = (Slider)drawableObject.HitObject;
            defaultPathRadius = skin.GetConfig<OsuSkinConfiguration, float>(OsuSkinConfiguration.SliderPathRadius)?.Value ?? OsuHitObject.OBJECT_RADIUS;

            scaleBindable = slider.ScaleBindable.GetBoundCopy();
            scaleBindable.BindValueChanged(_ => updatePathRadius(), true);

            pathVersion = slider.Path.Version.GetBoundCopy();
            pathVersion.BindValueChanged(_ => Refresh());

            accentColour = drawableObject.AccentColour.GetBoundCopy();
            accentColour.BindValueChanged(accent => updateAccentColour(skin, accent.NewValue), true);

            config?.BindWith(OsuRulesetSetting.SnakingInSliders, SnakingIn);
            config?.BindWith(OsuRulesetSetting.SnakingOutSliders, SnakingOut);

            BorderSize = skin.GetConfig<OsuSkinConfiguration, float>(OsuSkinConfiguration.SliderBorderSize)?.Value ?? 1;
            BorderColour = skin.GetConfig<OsuSkinColour, Color4>(OsuSkinColour.SliderBorder)?.Value ?? Color4.White;
        }

        private void updatePathRadius()
            => PathRadius = defaultPathRadius * scaleBindable.Value;

        private void updateAccentColour(ISkinSource skin, Color4 defaultAccentColour)
            => AccentColour = skin.GetConfig<OsuSkinColour, Color4>(OsuSkinColour.SliderTrackOverride)?.Value ?? defaultAccentColour;
    }
}
