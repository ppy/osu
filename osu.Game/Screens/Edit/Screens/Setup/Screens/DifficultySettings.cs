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
using osu.Game.Screens.Edit.Screens;
using osu.Game.Screens.Edit.Components;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Screens.Edit.Screens.Setup.Screens
{
    public class DifficultySettings : EditorSettingsGroup
    {
        private readonly ColouredEditorSliderBar<float> hpDrainRateSliderBar;
        private readonly ColouredEditorSliderBar<float> circleSizeSliderBar;
        private readonly ColouredEditorSliderBar<float> approachRateSliderBar;
        private readonly ColouredEditorSliderBar<float> overallDifficultySliderBar;
        private readonly OsuSpriteText hpDrainText;
        private readonly OsuSpriteText circleSizeText;
        private readonly OsuSpriteText approachRateText;
        private readonly OsuSpriteText overallDifficultyText;

        protected override string Title => @"difficulty";

        public DifficultySettings()
        {
            Children = new Drawable[]
            {
                // Also add support for changing precision when shift is held down
                new Container
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Children = new Drawable[]
                    {
                        CreateSettingLabelText("Overall Difficulty"),
                        overallDifficultyText = CreateSettingLabelTextBold(),
                    },
                },
                overallDifficultySliderBar = CreateSliderBar(5, 5, 0, 10),
                new Container
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Children = new Drawable[]
                    {
                        CreateSettingLabelText("HP Drain Rate"),
                        hpDrainText = CreateSettingLabelTextBold(),
                    },
                },
                hpDrainRateSliderBar = CreateSliderBar(5, 5, 0, 10),
                new Container
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Children = new Drawable[]
                    {
                        CreateSettingLabelText("Approach Rate"),
                        approachRateText = CreateSettingLabelTextBold(),
                    },
                },
                approachRateSliderBar = CreateSliderBar(5, 5, 0, 10),
                new Container
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Children = new Drawable[]
                    {
                        CreateSettingLabelText("Circle Size"),
                        circleSizeText = CreateSettingLabelTextBold(),
                    },
                },
                circleSizeSliderBar = CreateSliderBar(5, 5, 2, 7),
            };

            hpDrainRateSliderBar.Bindable.ValueChanged += showValue => hpDrainText.Text = $"{hpDrainRateSliderBar.Bar.TooltipText}";
            hpDrainRateSliderBar.Bindable.TriggerChange();
            circleSizeSliderBar.Bindable.ValueChanged += showValue => circleSizeText.Text = $"{circleSizeSliderBar.Bar.TooltipText}";
            circleSizeSliderBar.Bindable.TriggerChange();
            approachRateSliderBar.Bindable.ValueChanged += showValue => approachRateText.Text = $"{approachRateSliderBar.Bar.TooltipText}";
            approachRateSliderBar.Bindable.TriggerChange();
            overallDifficultySliderBar.Bindable.ValueChanged += showValue => overallDifficultyText.Text = $"{overallDifficultySliderBar.Bar.TooltipText}";
            overallDifficultySliderBar.Bindable.TriggerChange();
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
        }
        
        OsuSpriteText CreateSettingLabelText(string text) => new OsuSpriteText
        {
            Anchor = Anchor.CentreLeft,
            Origin = Anchor.CentreLeft,
            Text = text,
        };
        OsuSpriteText CreateSettingLabelTextBold() => new OsuSpriteText
        {
            Anchor = Anchor.CentreRight,
            Origin = Anchor.CentreRight,
            Font = @"Exo2.0-Bold",
        };
        ColouredEditorSliderBar<float> CreateSliderBar(float value, float defaultValue, float min, float max) => new ColouredEditorSliderBar<float>
        {
            NormalPrecision = 1,
            AlternatePrecision = 0.1f,
            Bindable = CreateBindable(value, defaultValue, min, max, 1),
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
