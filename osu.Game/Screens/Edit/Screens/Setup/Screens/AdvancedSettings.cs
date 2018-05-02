// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Screens.Edit.Components;

namespace osu.Game.Screens.Edit.Screens.Setup.Screens
{
    public class AdvancedSettings : SettingsGroup
    {
        protected override string Title => @"advanced";

        public AdvancedSettings()
        {
            AllowCollapsing = false;

            EditorSliderBar<float> stackLeniencySliderBar;
            OsuSpriteText stackLeniencyText;

            Children = new Drawable[]
            {
                new Container
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Children = new Drawable[]
                    {
                        createSettingLabelText("Stack Leniency"),
                        stackLeniencyText = createSettingLabelTextBold(),
                    },
                },
                stackLeniencySliderBar = new EditorSliderBar<float>
                {
                    NormalPrecision = 1,
                    AlternatePrecision = 1,
                    Bindable = createBindable(7, 7, 2, 10, 1),
                },
            };

            stackLeniencySliderBar.Bindable.ValueChanged += showValue => stackLeniencyText.Text = $"{stackLeniencySliderBar.Bar.TooltipText}";
            stackLeniencySliderBar.Bindable.TriggerChange();
        }

        private OsuSpriteText createSettingLabelText(string text) => new OsuSpriteText
        {
            Anchor = Anchor.CentreLeft,
            Origin = Anchor.CentreLeft,
            Text = text,
        };
        private OsuSpriteText createSettingLabelTextBold() => new OsuSpriteText
        {
            Anchor = Anchor.CentreRight,
            Origin = Anchor.CentreRight,
            Font = @"Exo2.0-Bold",
        };
        private Bindable<float> createBindable(float value, float defaultValue, float min, float max, float precision) => new BindableFloat(value)
        {
            Default = defaultValue,
            MinValue = min,
            MaxValue = max,
            Precision = precision,
        };
    }
}
