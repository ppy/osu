// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Game.Configuration;
using osu.Game.Graphics;

namespace osu.Game.Screens.Play.HUD
{
    public class LowHealthLayer : HealthDisplay
    {
        private const float max_alpha = 0.4f;

        private const double fade_time = 300;

        private readonly Box box;

        private Bindable<bool> configFadeRedWhenLowHealth;

        public LowHealthLayer()
        {
            Child = box = new Box
            {
                RelativeSizeAxes = Axes.Both,
                Alpha = 0
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config, OsuColour color)
        {
            configFadeRedWhenLowHealth = config.GetBindable<bool>(OsuSetting.FadePlayfieldWhenLowHealth);
            box.Colour = color.Red;

            configFadeRedWhenLowHealth.BindValueChanged(value =>
            {
                if (value.NewValue)
                    this.FadeIn(fade_time, Easing.OutQuint);
                else
                    this.FadeOut(fade_time, Easing.OutQuint);
            }, true);
        }
    }
}
