// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Configuration;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Skinning;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Osu.Skinning.Default
{
    public abstract partial class PlaySliderBody : SnakingSliderBody
    {
        protected IBindable<float> ScaleBindable { get; private set; } = null!;

        protected IBindable<Color4> AccentColourBindable { get; private set; } = null!;

        private IBindable<int> pathVersion = null!;

        [Resolved(canBeNull: true)]
        private IReadOnlyList<Mod>? mods { get; set; }

        protected OsuModHidden? Hidden { get; private set; }

        [Resolved(CanBeNull = true)]
        private OsuRulesetConfigManager? config { get; set; }

        private readonly BindableBool legacySliderFade = new BindableBool();

        private readonly Bindable<bool> configSnakingOut = new Bindable<bool>();

        [BackgroundDependencyLoader]
        private void load(ISkinSource skin, DrawableHitObject drawableObject)
        {
            var drawableSlider = (DrawableSlider)drawableObject;

            ScaleBindable = drawableSlider.ScaleBindable.GetBoundCopy();
            ScaleBindable.BindValueChanged(scale => PathRadius = OsuHitObject.OBJECT_RADIUS * scale.NewValue, true);

            pathVersion = drawableSlider.PathVersion.GetBoundCopy();
            pathVersion.BindValueChanged(_ => Scheduler.AddOnce(Refresh));

            AccentColourBindable = drawableObject.AccentColour.GetBoundCopy();
            AccentColourBindable.BindValueChanged(accent => AccentColour = GetBodyAccentColour(skin, accent.NewValue), true);

            config?.BindWith(OsuRulesetSetting.SnakingInSliders, SnakingIn);
            config?.BindWith(OsuRulesetSetting.SnakingOutSliders, configSnakingOut);

            Hidden = mods?.OfType<OsuModHidden>().FirstOrDefault();

            if (Hidden != null)
            {
                legacySliderFade.BindTo(Hidden.LegacySliderFade);
                if (legacySliderFade.Value == true)
                {
                    SnakingIn.Value = false;
                    SnakingOut.Value = false;
                }
                else SnakingOut.BindTo(configSnakingOut);
            }
            else SnakingOut.BindTo(configSnakingOut);

            BorderColour = GetBorderColour(skin);
        }

        protected virtual Color4 GetBorderColour(ISkinSource skin) => Color4.White;

        protected virtual Color4 GetBodyAccentColour(ISkinSource skin, Color4 hitObjectAccentColour) => hitObjectAccentColour;
    }
}
