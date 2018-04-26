// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input;
using osu.Framework.Screens;
using osu.Game.Skinning;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays.Settings;
using osu.Game.Screens.Backgrounds;
using osu.Game.Screens.Edit.Screens;
using osu.Game.Screens.Edit.Components;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Screens.Edit.Screens.Setup.Screens
{
    public class DesignSettings : EditorSettingsGroup
    {
        private EditorCheckbox enableCountdownCheckbox;
        private EditorSliderBar<int> countdownOffsetSlider;
        private EditorSliderBar<int> countdownSpeedSlider;
        private OsuSpriteText countdownOffsetLabel;
        private OsuSpriteText countdownOffsetText;
        private OsuSpriteText countdownSpeedLabel;
        private OsuSpriteText countdownSpeedText;
        private OsuDropdown<int> skinsDropdown;
        public SkinManager Skins;

        protected override string Title => @"design";

        public DesignSettings(SkinManager skins)
        {
            Skins = skins;
            Children = new Drawable[]
            {
                CreateSettingCheckBox("Display Epilepsy Warning"),
                CreateSettingLabelText("Beatmap Skin"),
                new Container
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Children = new Drawable[]
                    {
                        skinsDropdown = new OsuDropdown<int>
                        {
                            RelativeSizeAxes = Axes.X,
                            Anchor = Anchor.TopLeft,
                            Origin = Anchor.TopLeft,
                            Items = new KeyValuePair<string, int>[]
                            {
                                new KeyValuePair<string, int>("No Custom Skin", -1),
                            },
                            Alpha = 1,
                        },
                    },
                },
                enableCountdownCheckbox = CreateSettingCheckBox("Enable Countdown", true),
                new Container
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Children = new Drawable[]
                    {
                        countdownSpeedLabel = CreateSettingLabelText("Countdown Speed"),
                        countdownSpeedText = CreateSettingLabelTextBold(),
                    },
                },
                countdownSpeedSlider = CreateSliderBar(1, 1, 0, 2, 1),
                new Container
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Children = new Drawable[]
                    {
                        countdownOffsetLabel = CreateSettingLabelText("Countdown Beat Offset"),
                        countdownOffsetText = CreateSettingLabelTextBold(),
                    },
                },
                countdownOffsetSlider = CreateSliderBar(0, 0, 0, 3, 1),
            };

            //var s = Skins.GetAllUsableSkins();
            //for (int i = 0; i < s.Length; i++)
            //{
            //    skinsDropdown.AddDropdownItem(s[i].Name, i);
            //}
            enableCountdownCheckbox.Current.ValueChanged += updateValue => { countdownSpeedLabel.Alpha = countdownSpeedText.Alpha = countdownSpeedSlider.Alpha = countdownOffsetLabel.Alpha = countdownOffsetText.Alpha = countdownOffsetSlider.Alpha = (enableCountdownCheckbox.Current.Value ? 1 : 0); };
            enableCountdownCheckbox.Current.TriggerChange();
            countdownOffsetSlider.Bindable.ValueChanged += showValue => countdownOffsetText.Text = $"{countdownOffsetSlider.Bar.TooltipText}";
            countdownOffsetSlider.Bindable.TriggerChange();
            countdownSpeedSlider.Bindable.ValueChanged += showValue => countdownSpeedText.Text = $"{GetCountdownSpeedString(countdownSpeedSlider.Bindable.Value)}";
            countdownSpeedSlider.Bindable.TriggerChange();
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
        }

        string GetCountdownSpeedString(int speed)
        {
            switch (speed)
            {
                case 0:
                    return "Half";
                case 1:
                    return "Normal";
                case 2:
                    return "Double";
                default:
                    throw new ArgumentException("The speed value that was provided is not a valid countdown speed value.");
            }
        }
        EditorCheckbox CreateSettingCheckBox(string text) => new EditorCheckbox
        {
            //Anchor = Anchor.CentreLeft,
            //Origin = Anchor.CentreLeft,
            LabelText = text,
        };
        EditorCheckbox CreateSettingCheckBox(string text, bool defaultValue) => new EditorCheckbox
        {
            //Anchor = Anchor.CentreLeft,
            //Origin = Anchor.CentreLeft,
            LabelText = text,
            Bindable = new BindableBool(defaultValue)
        };
        OsuSpriteText CreateSettingLabelText(string text) => new OsuSpriteText
        {
            Anchor = Anchor.TopLeft,
            Origin = Anchor.TopLeft,
            Text = text,
        };
        OsuSpriteText CreateSettingLabelTextBold() => new OsuSpriteText
        {
            Anchor = Anchor.CentreRight,
            Origin = Anchor.CentreRight,
            Font = @"Exo2.0-Bold",
        };
        EditorSliderBar<int> CreateSliderBar(int value, int defaultValue, int min, int max, int precision) => new EditorSliderBar<int>
        {
            Bindable = CreateBindable(value, defaultValue, min, max, precision),
        };
        Bindable<int> CreateBindable(int value, int defaultValue, int min, int max, int precision) => new BindableInt(value)
        {
            Default = defaultValue,
            MinValue = min,
            MaxValue = max,
            Precision = precision,
        };
    }
}
