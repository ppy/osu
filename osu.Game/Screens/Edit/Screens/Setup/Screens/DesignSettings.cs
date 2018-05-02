// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Skinning;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Screens.Edit.Components;
using OpenTK;

namespace osu.Game.Screens.Edit.Screens.Setup.Screens
{
    public class DesignSettings : SettingsGroup
    {
        private EditorCheckbox enableCountdownCheckbox;
        private EditorSliderBar<int> countdownOffsetSlider;
        private EditorSliderBar<int> countdownSpeedSlider;
        private OsuSpriteText countdownOffsetLabel;
        private OsuSpriteText countdownOffsetText;
        private OsuSpriteText countdownSpeedLabel;
        private OsuSpriteText countdownSpeedText;
        private EditorDropdown<int> skinsDropdown;
        private FillFlowContainer ffc;
        public SkinManager Skins;

        protected override string Title => @"design";

        [BackgroundDependencyLoader]
        private void load(SkinManager skins)
        {
            AllowCollapsing = false;

            Skins = skins;
            Children = new Drawable[]
            {
                createSettingCheckBox("Display Epilepsy Warning"),
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(0, 5),
                    Children = new Drawable[]
                    {
                        createSettingLabelText("Beatmap Skin"),
                        new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Children = new Drawable[]
                            {
                                skinsDropdown = new EditorDropdown<int>
                                {
                                    RelativeSizeAxes = Axes.X,
                                    Anchor = Anchor.TopLeft,
                                    Origin = Anchor.TopLeft,
                                    Items = new List<KeyValuePair<string, int>>
                                    {
                                        new KeyValuePair<string, int>("No Custom Skin", -1),
                                    },
                                    Alpha = 1,
                                },
                            },
                        },
                    }
                },
                ffc = new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(0, 15),
                    Children = new Drawable[]
                    {
                        enableCountdownCheckbox = createSettingCheckBox("Enable Countdown", true),
                        new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Children = new Drawable[]
                            {
                                countdownSpeedLabel = createSettingLabelText("Countdown Speed"),
                                countdownSpeedText = createSettingLabelTextBold(),
                            },
                        },
                        countdownSpeedSlider = createSliderBar(1, 1, 0, 2, 1),
                        new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Children = new Drawable[]
                            {
                                countdownOffsetLabel = createSettingLabelText("Countdown Beat Offset"),
                                countdownOffsetText = createSettingLabelTextBold(),
                            },
                        },
                        countdownOffsetSlider = createSliderBar(0, 0, 0, 3, 1),
                    }
                }
            };

            var s = Skins.GetAllUsableSkins();
            for (int i = 0; i < s.Count; i++)
            {
                skinsDropdown.AddDropdownItem(s[i].Name, i);
            }
            skinsDropdown.Current.ValueChanged += a => { }; // Change the skin used in the beatmap
            enableCountdownCheckbox.Current.ValueChanged += updateStatus;
            enableCountdownCheckbox.Current.TriggerChange();
            countdownOffsetSlider.Bindable.ValueChanged += showValue => countdownOffsetText.Text = $"{countdownOffsetSlider.Bar.TooltipText}";
            countdownOffsetSlider.Bindable.TriggerChange();
            countdownSpeedSlider.Bindable.ValueChanged += showValue => countdownSpeedText.Text = $"{getCountdownSpeedString(countdownSpeedSlider.Bindable.Value)}";
            countdownSpeedSlider.Bindable.TriggerChange();
        }

        private void updateStatus(bool a)
        {
            countdownSpeedLabel.Alpha = countdownSpeedText.Alpha = countdownSpeedSlider.Alpha = countdownOffsetLabel.Alpha = countdownOffsetText.Alpha = countdownOffsetSlider.Alpha = enableCountdownCheckbox.Current.Value ? 1 : 0;
            ffc.Spacing = new Vector2(0, enableCountdownCheckbox.Current.Value ? 15 : 0);
        }

        private string getCountdownSpeedString(int speed)
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
        private EditorCheckbox createSettingCheckBox(string text) => new EditorCheckbox
        {
            LabelText = text,
        };
        private EditorCheckbox createSettingCheckBox(string text, bool defaultValue) => new EditorCheckbox
        {
            LabelText = text,
            Bindable = new BindableBool(defaultValue)
        };
        private OsuSpriteText createSettingLabelText(string text) => new OsuSpriteText
        {
            Anchor = Anchor.TopLeft,
            Origin = Anchor.TopLeft,
            Text = text,
        };
        private OsuSpriteText createSettingLabelTextBold() => new OsuSpriteText
        {
            Anchor = Anchor.CentreRight,
            Origin = Anchor.CentreRight,
            Font = @"Exo2.0-Bold",
        };
        private EditorSliderBar<int> createSliderBar(int value, int defaultValue, int min, int max, int precision) => new EditorSliderBar<int>
        {
            NormalPrecision = precision,
            AlternatePrecision = precision,
            Bindable = createBindable(value, defaultValue, min, max, precision),
        };
        private Bindable<int> createBindable(int value, int defaultValue, int min, int max, int precision) => new BindableInt(value)
        {
            Default = defaultValue,
            MinValue = min,
            MaxValue = max,
            Precision = precision,
        };
    }
}
