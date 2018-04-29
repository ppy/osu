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
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Screens.Backgrounds;
using osu.Game.Screens.Edit.Components;
using osu.Game.Screens.Edit.Screens;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Screens.Edit.Screens.Setup.Screens
{
    public class ModeSettings : EditorSettingsGroup
    {
        private readonly EditorSliderBar<float> keyCountSliderBar;
        private readonly OsuSpriteText keyCountLabel;
        private readonly OsuSpriteText keyCountText;
        private readonly EditorCheckbox coOpModeCheckbox;
        private readonly EditorCheckbox specialKeyStyleCheckbox;
        private readonly EditorEnumDropdown<AvailableModes> modeDropdown;
        private readonly FillFlowContainer ffc;

        protected override string Title => @"mode";

        public ModeSettings()
        {
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
                                CreateSettingLabelText("Available Modes"),
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
                                            Items = new KeyValuePair<string, AvailableModes>[]
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
                        coOpModeCheckbox = CreateSettingCheckBox("Co-Op Mode", 0),
                        specialKeyStyleCheckbox = CreateSettingCheckBox("Use Special Key Style", 0),
                        new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Children = new Drawable[]
                            {
                                keyCountLabel = CreateSettingLabelText("Key Count", 0),
                                keyCountText = CreateSettingLabelTextBold(0),
                            },
                        },
                        keyCountSliderBar = new EditorSliderBar<float>
                        {
                            NormalPrecision = 1,
                            AlternatePrecision = 1,
                            Bindable = CreateBindable(4, 4, 1, 9, 1),
                            Alpha = 0,
                        },
                    }
                }
            };
            modeDropdown.Current.ValueChanged += UpdateStatus;
            modeDropdown.Current.TriggerChange();
            keyCountSliderBar.Bindable.ValueChanged += showValue => keyCountText.Text = $"{keyCountSliderBar.Bar.TooltipText}";
            keyCountSliderBar.Bindable.TriggerChange();
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
        }

        void UpdateStatus(AvailableModes a)
        {
            coOpModeCheckbox.Alpha = specialKeyStyleCheckbox.Alpha = keyCountLabel.Alpha = keyCountText.Alpha = keyCountSliderBar.Alpha = (modeDropdown.Current.Value == AvailableModes.Mania) ? 1 : 0;
            ffc.Spacing = new Vector2(0, modeDropdown.Current.Value == AvailableModes.Mania ? 15 : 0);
        }

        EditorCheckbox CreateSettingCheckBox(string text) => new EditorCheckbox
        {
            LabelText = text,
        };
        EditorCheckbox CreateSettingCheckBox(string text, float alpha) => new EditorCheckbox
        {
            LabelText = text,
            Alpha = alpha,
        };
        EditorCheckbox CreateSettingCheckBox(string text, bool defaultValue) => new EditorCheckbox
        {
            LabelText = text,
            Bindable = new BindableBool(defaultValue)
        };
        OsuSpriteText CreateSettingLabelText(string text) => new OsuSpriteText
        {
            Anchor = Anchor.TopLeft,
            Origin = Anchor.TopLeft,
            Text = text,
        };
        OsuSpriteText CreateSettingLabelText(string text, float alpha) => new OsuSpriteText
        {
            Anchor = Anchor.TopLeft,
            Origin = Anchor.TopLeft,
            Text = text,
            Alpha = alpha,
        };
        OsuSpriteText CreateSettingLabelTextBold() => new OsuSpriteText
        {
            Anchor = Anchor.CentreRight,
            Origin = Anchor.CentreRight,
            Font = @"Exo2.0-Bold",
        };
        OsuSpriteText CreateSettingLabelTextBold(float alpha) => new OsuSpriteText
        {
            Anchor = Anchor.CentreRight,
            Origin = Anchor.CentreRight,
            Font = @"Exo2.0-Bold",
            Alpha = alpha,
        };
        Bindable<float> CreateBindable(float value, float defaultValue, float min, float max, float precision) => new BindableFloat(value)
        {
            Default = defaultValue,
            MinValue = min,
            MaxValue = max,
            Precision = precision,
        };
    }
}
