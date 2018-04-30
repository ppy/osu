// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Globalization;
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
        private readonly ColouredEditorSliderBar<float> overallDifficultySliderBar;
        private readonly ColouredEditorSliderBar<float> hpDrainRateSliderBar;
        private readonly ColouredEditorSliderBar<float> approachRateSliderBar;
        private readonly ColouredEditorSliderBar<float> circleSizeSliderBar;
        private readonly OsuSpriteText overallDifficultyText;
        private readonly OsuSpriteText hpDrainText;
        private readonly OsuSpriteText approachRateText;
        private readonly OsuSpriteText circleSizeText;
        private readonly OsuSpriteText overallDifficultyLabel;
        private readonly OsuSpriteText hpDrainLabel;
        private readonly OsuSpriteText approachRateLabel;
        private readonly OsuSpriteText circleSizeLabel;
        private readonly FillFlowContainer ffc;

        protected override string Title => @"difficulty";

        public DifficultySettings()
        {
            Children = new Drawable[]
            {
                new Container
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Children = new Drawable[]
                    {
                        overallDifficultyLabel = CreateSettingLabelText("Overall Difficulty"),
                        overallDifficultyText = CreateSettingLabelTextBold(),
                    },
                },
                overallDifficultySliderBar = CreateSliderBar(5, 5, 0, 10),
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
                            Spacing = new Vector2(0, 15),
                            Children = new Drawable[]
                            {
                                new Container
                                {
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Children = new Drawable[]
                                    {
                                        hpDrainLabel = CreateSettingLabelText("HP Drain Rate"),
                                        hpDrainText = CreateSettingLabelTextBold(),
                                    },
                                },
                                hpDrainRateSliderBar = CreateSliderBar(5, 5, 0, 10),
                            }
                        },
                        new FillFlowContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Direction = FillDirection.Vertical,
                            Spacing = new Vector2(0, 15),
                            Children = new Drawable[]
                            {
                                new Container
                                {
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Children = new Drawable[]
                                    {
                                        approachRateLabel = CreateSettingLabelText("Approach Rate"),
                                        approachRateText = CreateSettingLabelTextBold(),
                                    },
                                },
                                approachRateSliderBar = CreateSliderBar(5, 5, 0, 10),
                            }
                        },
                        new FillFlowContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Direction = FillDirection.Vertical,
                            Spacing = new Vector2(0, 15),
                            Children = new Drawable[]
                            {
                                new Container
                                {
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Children = new Drawable[]
                                    {
                                        circleSizeLabel = CreateSettingLabelText("Circle Size"),
                                        circleSizeText = CreateSettingLabelTextBold(),
                                    },
                                },
                                circleSizeSliderBar = CreateSliderBar(5, 5, 2, 7),
                            }
                        }
                    }
                }
            };

            // I am really sorry for your eyes
            hpDrainRateSliderBar.Bindable.ValueChanged += showValue => hpDrainText.Text = $"{TrimUnnecessaryDecimalPart(hpDrainRateSliderBar.Bar.Current.Value.ToString("N1", CultureInfo.InvariantCulture))}";
            hpDrainRateSliderBar.Bindable.TriggerChange();
            circleSizeSliderBar.Bindable.ValueChanged += showValue => circleSizeText.Text = $"{TrimUnnecessaryDecimalPart(circleSizeSliderBar.Bar.Current.Value.ToString("N1", CultureInfo.InvariantCulture))}";
            circleSizeSliderBar.Bindable.TriggerChange();
            approachRateSliderBar.Bindable.ValueChanged += showValue => approachRateText.Text = $"{TrimUnnecessaryDecimalPart(approachRateSliderBar.Bar.Current.Value.ToString("N1", CultureInfo.InvariantCulture))}";
            approachRateSliderBar.Bindable.TriggerChange();
            overallDifficultySliderBar.Bindable.ValueChanged += showValue => overallDifficultyText.Text = $"{TrimUnnecessaryDecimalPart(overallDifficultySliderBar.Bar.Current.Value.ToString("N1", CultureInfo.InvariantCulture))}";
            overallDifficultySliderBar.Bindable.TriggerChange();
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
        }

        string TrimUnnecessaryDecimalPart(string s)
        {
            int index = -1;
            for (int i = 0; i < s.Length && index < 0; i++)
                if (s[i] == '.')
                    index = i;
            if (index > -1) // Number contains decimal part
            {
                int finalLength = s.Length;
                while (s[finalLength - 1] == '0')
                    finalLength--;
                if (s[finalLength - 1] == '.')
                    finalLength--;
                return s.Substring(0, finalLength);
            }
            else return s;
        }

        public void HideApproachRateAndCircleSize()
        {
            ffc.Spacing = new Vector2(0, 0);
            ChangeApproachRateAlpha(0);
            ChangeCircleSizeAlpha(0);
        }
        public void ShowApproachRateAndCircleSize()
        {
            ffc.Spacing = new Vector2(0, 15);
            ChangeApproachRateAlpha(1);
            ChangeCircleSizeAlpha(1);
        }
        public void ChangeApproachRateAlpha(float alpha) => approachRateSliderBar.Alpha = approachRateText.Alpha = approachRateLabel.Alpha = alpha;
        public void ChangeCircleSizeAlpha(float alpha) => circleSizeSliderBar.Alpha = circleSizeText.Alpha = circleSizeLabel.Alpha = alpha;

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
