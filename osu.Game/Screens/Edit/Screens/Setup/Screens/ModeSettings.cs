// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Screens.Edit.Components;
using OpenTK;

namespace osu.Game.Screens.Edit.Screens.Setup.Screens
{
    public class ModeSettings : EditorSettingsGroup
    {
        private readonly EditorSliderBar<float> keyCountSliderBar;
        private readonly OsuSpriteText keyCountLabel;
        private readonly OsuSpriteText keyCountText;
        private readonly EditorCheckbox coOpModeCheckbox;
        private readonly EditorCheckbox specialKeyStyleCheckbox;
        private readonly FillFlowContainer ffc;

        /// <summary>Triggers when the available modes changes</summary>
        public event Action<AvailableModes> AvailableModesChanged;

        protected override string Title => @"mode";

        public ModeSettings()
        {
            EditorEnumDropdown<AvailableModes> modeDropdown;

            Children = new Drawable[]
            {
                ffc = new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(0, 15),
                    Children = new Drawable[]
                    {
                        new FillFlowContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Direction = FillDirection.Vertical,
                            Spacing = new Vector2(0, 5),
                            Children = new Drawable[]
                            {
                                createSettingLabelText("Available Modes"),
                                new Container
                                {
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Children = new Drawable[]
                                    {
                                        modeDropdown = new EditorEnumDropdown<AvailableModes>
                                        {
                                            RelativeSizeAxes = Axes.X,
                                            Anchor = Anchor.TopLeft,
                                            Origin = Anchor.TopLeft,
                                            Items = new List<KeyValuePair<string, AvailableModes>>
                                            {
                                                new KeyValuePair<string, AvailableModes>("All", AvailableModes.All),
                                                new KeyValuePair<string, AvailableModes>("osu!taiko", AvailableModes.Taiko),
                                                new KeyValuePair<string, AvailableModes>("osu!catch", AvailableModes.Catch),
                                                new KeyValuePair<string, AvailableModes>("osu!mania", AvailableModes.Mania),
                                            },
                                            Alpha = 1,
                                        },
                                    },
                                },
                            }
                        },
                        coOpModeCheckbox = createSettingCheckBox("Co-Op Mode", 0),
                        specialKeyStyleCheckbox = createSettingCheckBox("Use Special Key Style", 0),
                        new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Children = new Drawable[]
                            {
                                keyCountLabel = createSettingLabelText("Key Count", 0),
                                keyCountText = createSettingLabelTextBold(0),
                            },
                        },
                        keyCountSliderBar = new EditorSliderBar<float>
                        {
                            NormalPrecision = 1,
                            AlternatePrecision = 1,
                            Bindable = createBindable(4, 4, 1, 9, 1),
                            Alpha = 0,
                        },
                    }
                }
            };
            modeDropdown.Current.ValueChanged += TriggerAvailableModesChanged;
            modeDropdown.Current.TriggerChange();
            keyCountSliderBar.Bindable.ValueChanged += showValue => keyCountText.Text = $"{keyCountSliderBar.Bar.TooltipText}";
            keyCountSliderBar.Bindable.TriggerChange();
        }

        public void TriggerAvailableModesChanged(AvailableModes a)
        {
            updateModes(a);
            AvailableModesChanged?.Invoke(a);
        }
        private void updateModes(AvailableModes a)
        {
            coOpModeCheckbox.Alpha = specialKeyStyleCheckbox.Alpha = keyCountLabel.Alpha = keyCountText.Alpha = keyCountSliderBar.Alpha = a == AvailableModes.Mania ? 1 : 0;
            ffc.Spacing = new Vector2(0, a == AvailableModes.Mania ? 15 : 0);
        }

        private EditorCheckbox createSettingCheckBox(string text) => new EditorCheckbox
        {
            LabelText = text,
        };
        private EditorCheckbox createSettingCheckBox(string text, float alpha) => new EditorCheckbox
        {
            LabelText = text,
            Alpha = alpha,
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
        private OsuSpriteText createSettingLabelText(string text, float alpha) => new OsuSpriteText
        {
            Anchor = Anchor.TopLeft,
            Origin = Anchor.TopLeft,
            Text = text,
            Alpha = alpha,
        };
        private OsuSpriteText createSettingLabelTextBold() => new OsuSpriteText
        {
            Anchor = Anchor.CentreRight,
            Origin = Anchor.CentreRight,
            Font = @"Exo2.0-Bold",
        };
        private OsuSpriteText createSettingLabelTextBold(float alpha) => new OsuSpriteText
        {
            Anchor = Anchor.CentreRight,
            Origin = Anchor.CentreRight,
            Font = @"Exo2.0-Bold",
            Alpha = alpha,
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
